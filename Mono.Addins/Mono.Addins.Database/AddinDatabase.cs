//
// AddinDatabase.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using System.Reflection;
using Mono.Addins.Description;

namespace Mono.Addins.Database
{
	class AddinDatabase
	{
		const string VersionTag = "000";

		ArrayList addinSetupInfos;
		internal static bool RunningSetupProcess;
		bool fatalDatabseError;
		Hashtable cachedAddinSetupInfos = new Hashtable ();
		AddinScanResult currentScanResult;
		AddinHostIndex hostIndex;
		FileDatabase fileDatabase;
		string addinDbDir;
		DatabaseConfiguration config = null;
		AddinRegistry registry;
		
		public AddinDatabase (AddinRegistry registry)
		{
			this.registry = registry;
			addinDbDir = Path.Combine (registry.RegistryPath, "addin-db-" + VersionTag);
			fileDatabase = new FileDatabase (AddinDbDir);
		}
		
		string AddinDbDir {
			get { return addinDbDir; }
		}
		
		public string AddinCachePath {
			get { return Path.Combine (AddinDbDir, "addin-data"); }
		}
		
		public string AddinFolderCachePath {
			get { return Path.Combine (AddinDbDir, "addin-dir-data"); }
		}
		
		string HostIndexFile {
			get { return Path.Combine (AddinDbDir, "host-index"); }
		}
		
		string ConfigFile {
			get { return Path.Combine (AddinDbDir, "config.xml"); }
		}
		
		internal bool IsGlobalRegistry {
			get {
				return registry.RegistryPath == AddinRegistry.GlobalRegistryPath;
			}
		}
		
		public void Clear ()
		{
			if (Directory.Exists (AddinCachePath))
				Directory.Delete (AddinCachePath, true);
			if (Directory.Exists (AddinFolderCachePath))
				Directory.Delete (AddinFolderCachePath, true);
		}
		
		public ExtensionNodeSet FindNodeSet (string addinId, string id)
		{
			return FindNodeSet (addinId, id, new Hashtable ());
		}
		
		ExtensionNodeSet FindNodeSet (string addinId, string id, Hashtable visited)
		{
			if (visited.Contains (addinId))
				return null;
			visited.Add (addinId, addinId);
			Addin addin = GetInstalledAddin (addinId, true, false);
			if (addin == null) {
				foreach (Addin root in GetAddinRoots ()) {
					if (root.Id == addinId) {
						addin = root;
						break;
					}
				}
				if (addin == null)
					return null;
			}
			AddinDescription desc = addin.Description;
			if (desc == null)
				return null;
			foreach (ExtensionNodeSet nset in desc.ExtensionNodeSets)
				if (nset.Id == id)
					return nset;
			
			// Not found in the add-in. Look on add-ins on which it depends
			
			foreach (Dependency dep in desc.MainModule.Dependencies) {
				AddinDependency adep = dep as AddinDependency;
				if (adep == null) continue;
				
				string aid = Addin.GetFullId (desc.Namespace, adep.AddinId, adep.Version);
				ExtensionNodeSet nset = FindNodeSet (aid, id, visited);
				if (nset != null)
					return nset;
			}
			return null;
		}
		
		public AddinDescription GetDescription (string id)
		{
			InternalCheck ();
			IDisposable dblock = fileDatabase.LockRead ();
			try {
				string path = GetDescriptionPath (id);
				AddinDescription desc = AddinDescription.ReadBinary (fileDatabase, path);
				if (desc != null)
					desc.OwnerDatabase = this;
				return desc;
			}
			catch (FileNotFoundException) {
				throw new InvalidOperationException ("Add-in not found: " + id);
			} finally {
				dblock.Dispose ();
			}
		}
		
		public ArrayList GetInstalledAddins ()
		{
			if (addinSetupInfos != null)
				return addinSetupInfos;
			
			InternalCheck ();
			using (fileDatabase.LockRead ()) {
				return InternalGetInstalledAddins ();
			}
		}
		
		public ArrayList GetAddinRoots ()
		{
			ArrayList list = new ArrayList ();
			foreach (string file in fileDatabase.GetDirectoryFiles (AddinCachePath, "*.mroot")) {
				list.Add (new Addin (this, file));
			}
			return list;
		}
		
		ArrayList InternalGetInstalledAddins ()
		{
			if (addinSetupInfos != null)
				return addinSetupInfos;

			addinSetupInfos = new ArrayList ();
			
			foreach (string file in fileDatabase.GetDirectoryFiles (AddinCachePath, "*.maddin")) {
				addinSetupInfos.Add (new Addin (this, file));
			}
			return addinSetupInfos;
		}
		
		public Addin GetInstalledAddin (string id)
		{
			return GetInstalledAddin (id, false, false);
		}
		
		public Addin GetInstalledAddin (string id, bool exactVersionMatch)
		{
			return GetInstalledAddin (id, exactVersionMatch, false);
		}
		
		public Addin GetInstalledAddin (string id, bool exactVersionMatch, bool enabledOnly)
		{
			Addin sinfo = null;
			object ob = cachedAddinSetupInfos [id];
			if (ob != null) {
				sinfo = ob as Addin;
				if (sinfo != null) {
					if (!enabledOnly || sinfo.Enabled)
						return sinfo;
					if (exactVersionMatch)
						return null;
				}
				else
					return null;
			}
		
			InternalCheck ();
			
			using (fileDatabase.LockRead ())
			{
				string path = GetDescriptionPath (id);
				if (sinfo == null && fileDatabase.Exists (path)) {
					sinfo = new Addin (this, path);
					cachedAddinSetupInfos [id] = sinfo;
					if (!enabledOnly || sinfo.Enabled)
						return sinfo;
					if (exactVersionMatch) {
						// Cache lookups with negative result
						cachedAddinSetupInfos [id] = this;
						return null;
					}
				}
				
				// Exact version not found. Look for a compatible version
				if (!exactVersionMatch) {
					sinfo = null;
					string version, name, bestVersion = null;
					Addin.GetIdParts (id, out name, out version);
					
					// FIXME: Not very efficient, will load all descriptions
					foreach (Addin ia in InternalGetInstalledAddins ()) 
					{
						if (Addin.GetIdName (ia.Id) == name && 
						    (!enabledOnly || ia.Enabled) &&
						    (version.Length == 0 || ia.SupportsVersion (version)) && 
						    (bestVersion == null || Addin.CompareVersions (bestVersion, ia.Version) > 0)) 
						{
							bestVersion = ia.Version;
							sinfo = ia;
						}
					}
					if (sinfo != null) {
						cachedAddinSetupInfos [id] = sinfo;
						return sinfo;
					}
				}
				
				// Cache lookups with negative result
				cachedAddinSetupInfos [id] = this;
				return null;
			}
		}
		
		public Addin GetInstalledAddin (string id, string version)
		{
			foreach (Addin ia in GetInstalledAddins ()) {
				if ((id == null || ia.Id == id) && (version == null || ia.Version == version))
					return ia;
			}
			return null;
		}
		
		public void Shutdown ()
		{
			addinSetupInfos = null;
		}
		
		public Addin GetAddinForHostAssembly (string assemblyLocation)
		{
			InternalCheck ();
			Addin ainfo = null;
			
			object ob = cachedAddinSetupInfos [assemblyLocation];
			if (ob != null) {
				ainfo = ob as Addin;
				if (ainfo != null)
					return ainfo;
				else
					return null;
			}

			AddinHostIndex index = GetAddinHostIndex ();
			string addin, addinFile;
			if (index.GetAddinForAssembly (assemblyLocation, out addin, out addinFile)) {
				ainfo = new Addin (this, addin, addinFile);
				cachedAddinSetupInfos [assemblyLocation] = ainfo;
			}
			
			return ainfo;
		}
		
		
		public bool IsAddinEnabled (string id)
		{
			Addin ainfo = GetInstalledAddin (id);
			return ainfo.Enabled;
		}
		
		internal bool IsAddinEnabled (string id, bool exactVersionMatch)
		{
			if (!exactVersionMatch)
				return IsAddinEnabled (id);
			return !Configuration.DisabledAddins.Contains (id);
		}
		
		public void EnableAddin (string id)
		{
			EnableAddin (id, true);
		}
		
		internal void EnableAddin (string id, bool exactVersionMatch)
		{
			Addin ainfo = GetInstalledAddin (id, exactVersionMatch);
			if (ainfo == null)
				// It may be an add-in root
				return;

			if (IsAddinEnabled (id))
				return;
			
			Configuration.DisabledAddins.Remove (id);
			SaveConfiguration ();

			// Enable required add-ins
			
			foreach (Dependency dep in ainfo.AddinInfo.Dependencies) {
				if (dep is AddinDependency) {
					AddinDependency adep = dep as AddinDependency;
					string adepid = Addin.GetFullId (ainfo.AddinInfo.Namespace, adep.AddinId, adep.Version);
					EnableAddin (adepid, false);
				}
			}
			if (AddinManager.IsInitialized && AddinManager.Registry.RegistryPath == registry.RegistryPath)
				AddinManager.SessionService.ActivateAddin (id);
		}
		
		public void DisableAddin (string id)
		{
			Addin ai = GetInstalledAddin (id, true);
			if (ai == null)
				throw new InvalidOperationException ("Add-in '" + id + "' not installed.");

			if (!IsAddinEnabled (id))
				return;

			Configuration.DisabledAddins.Add (id);
			SaveConfiguration ();
			
			// Disable all add-ins which depend on it
			
			string idName = Addin.GetIdName (id);
			
			foreach (Addin ainfo in GetInstalledAddins ()) {
				foreach (Dependency dep in ainfo.AddinInfo.Dependencies) {
					AddinDependency adep = dep as AddinDependency;
					if (adep == null)
						continue;
					
					string adepid = Addin.GetFullId (ainfo.AddinInfo.Namespace, adep.AddinId, null);
					if (adepid != idName)
						continue;
					
					// The add-in that has been disabled, might be a requeriment of this one, or maybe not
					// if there is an older version available. Check it now.
					
					adepid = Addin.GetFullId (ainfo.AddinInfo.Namespace, adep.AddinId, adep.Version);
					Addin adepinfo = GetInstalledAddin (adepid, false, true);
					
					if (adepinfo == null) {
						DisableAddin (ainfo.Id);
						break;
					}
				}
			}
			if (AddinManager.IsInitialized && AddinManager.Registry.RegistryPath == registry.RegistryPath)
				AddinManager.SessionService.UnloadAddin (id);
		}		

		internal string GetDescriptionPath (string id)
		{
			return Path.Combine (AddinCachePath, id + ".maddin");
		}
		
		void InternalCheck ()
		{
			// If the database is broken, don't try to regenerate it at every check.
			if (fatalDatabseError)
				return;

			bool update = false;
			using (fileDatabase.LockRead ()) {
				if (!Directory.Exists (AddinCachePath)) {
					update = true;
				}
			}
			if (update)
				Update (null);
		}
		
		void GenerateAddinExtensionMapsInternal (IProgressStatus monitor, ArrayList addinsToUpdate, ArrayList removedAddins)
		{
			AddinUpdateData updateData = new AddinUpdateData (this);
			
			// Clear cached data
			cachedAddinSetupInfos.Clear ();
			
			// Collect all information
			
			Hashtable addinHash = new Hashtable ();
			
			relExtensionPoints = 0;
			relExtensions = 0;
			relNodeSetTypes = 0;
			relExtensionNodes = 0;
		
			if (monitor.VerboseLog)
				monitor.Log ("Generating add-in extension maps");
			
			Hashtable changedAddins = null;
			ArrayList descriptionsToSave = new ArrayList ();
			ArrayList files = new ArrayList ();
			
			bool partialGeneration = addinsToUpdate != null;
			
			// Get the files to be updated
			
			if (partialGeneration) {
				changedAddins = new Hashtable ();
				foreach (string s in addinsToUpdate) {
					changedAddins [s] = s;
					string mp = GetDescriptionPath (s);
					if (fileDatabase.Exists (mp))
						files.Add (mp);
					else
						files.AddRange (fileDatabase.GetObjectSharedFiles (this.AddinCachePath, s, ".mroot"));
				}
				foreach (string s in removedAddins)
					changedAddins [s] = s;
			}
			else {
				files.AddRange (fileDatabase.GetDirectoryFiles (AddinCachePath, "*.maddin"));
				files.AddRange (fileDatabase.GetDirectoryFiles (AddinCachePath, "*.mroot"));
			}
			
			// Load the descriptions.
			foreach (string file in files) {
			
				AddinDescription conf;
				if (!ReadAddinDescription (monitor, file, out conf)) {
					SafeDelete (monitor, file);
					continue;
				}

				// If the original file does not exist, the description can be deleted
				if (!File.Exists (conf.AddinFile)) {
					SafeDelete (monitor, file);
					continue;
				}
				
				// Remove old data from the description. If changedAddins==null, removes all data.
				// Otherwise, removes data only from the addins in the table.
				
				conf.UnmergeExternalData (changedAddins);
				descriptionsToSave.Add (conf);
				
				// Register extension points and node sets from root add-ins
				if (conf.IsRoot) {
					foreach (ExtensionPoint ep in conf.ExtensionPoints)
						updateData.RegisterAddinRootExtensionPoint (conf, ep);
					foreach (ExtensionNodeSet ns in conf.ExtensionNodeSets)
						updateData.RegisterAddinRootNodeSet (conf, ns);
				}
				else
					addinHash [conf.AddinId] = conf;
			}
			
			foreach (AddinDescription conf in addinHash.Values) {
				CollectExtensionData (conf, updateData);
			}
			
			updateData.ResolveExtensions (monitor, addinHash);
			
			// Update the extension points defined by this add-in			
			foreach (ExtensionPoint ep in updateData.GetUnresolvedExtensionPoints ()) {
				AddinDescription am = (AddinDescription) addinHash [ep.RootAddin];
				ExtensionPoint amep = am.ExtensionPoints [ep.Path];
				if (amep != null) {
					amep.MergeWith (am.AddinId, ep);
					amep.RootAddin = ep.RootAddin;
				}
			}

			// Now update the node sets
			foreach (ExtensionPoint ep in updateData.GetUnresolvedExtensionSets ()) {
				AddinDescription am = (AddinDescription) addinHash [ep.RootAddin];
				ExtensionNodeSet nset = am.ExtensionNodeSets [ep.Path];
				if (nset != null)
					nset.MergeWith (am.AddinId, ep.NodeSet);
			}
			
			// Save the maps
			foreach (AddinDescription conf in descriptionsToSave)
				conf.SaveBinary (fileDatabase);
			
			if (monitor.VerboseLog) {
				monitor.Log ("Addin relation map generated.");
				monitor.Log ("  Addins Updated: " + descriptionsToSave.Count);
				monitor.Log ("  Extension points: " + relExtensionPoints);
				monitor.Log ("  Extensions: " + relExtensions);
				monitor.Log ("  Extension nodes: " + relExtensionNodes);
				monitor.Log ("  Node sets: " + relNodeSetTypes);
			}
		}
		
		int relExtensionPoints;
		int relExtensions;
		int relNodeSetTypes;
		int relExtensionNodes;
		
		
		// Collects extension data in a hash table. The key is the path, the value is a list
		// of add-ins ids that extend that path
		
		void CollectExtensionData (AddinDescription conf, AddinUpdateData updateData)
		{
			foreach (ExtensionNodeSet nset in conf.ExtensionNodeSets) {
				try {
					updateData.RegisterNodeSet (conf, nset);
					relNodeSetTypes++;
				} catch (Exception ex) {
					throw new InvalidOperationException ("Error reading node set: " + nset.Id, ex);
				}
			}
			
			foreach (ExtensionPoint ep in conf.ExtensionPoints) {
				try {
					updateData.RegisterExtensionPoint (conf, ep);
					relExtensionPoints++;
				} catch (Exception ex) {
					throw new InvalidOperationException ("Error reading extension point: " + ep.Path, ex);
				}
			}
			
			foreach (ModuleDescription module in conf.AllModules) {
				foreach (Extension ext in module.Extensions) {
					relExtensions++;
					updateData.RegisterExtension (conf, module, ext);
					AddChildExtensions (conf, module, updateData, ext.Path, ext.ExtensionNodes, false);
				}
			}
		}
		
		void AddChildExtensions (AddinDescription conf, ModuleDescription module, AddinUpdateData updateData, string path, ExtensionNodeDescriptionCollection nodes, bool conditionChildren)
		{
			// Don't register conditions as extension nodes.
			if (!conditionChildren)
				updateData.RegisterExtension (conf, module, path);
			
			foreach (ExtensionNodeDescription node in nodes) {
				if (node.NodeName == "ComplexCondition")
					continue;
				relExtensionNodes++;
				string id = node.GetAttribute ("id");
				if (id.Length != 0)
					AddChildExtensions (conf, module, updateData, path + "/" + id, node.ChildNodes, node.NodeName == "Condition");
			}
		}

		internal void ResetCachedData ()
		{
			addinSetupInfos = null;
			hostIndex = null;
			cachedAddinSetupInfos.Clear ();
		}
		
		
		public bool AddinDependsOn (string id1, string id2)
		{
			Addin addin1 = GetInstalledAddin (id1, false);
			
			// We can assumbe that if the add-in is not returned here, it may be a root addin.
			if (addin1 == null)
				return false;

			id2 = Addin.GetIdName (id2);
			foreach (Dependency dep in addin1.AddinInfo.Dependencies) {
				AddinDependency adep = dep as AddinDependency;
				if (adep == null)
					continue;
				string depid = Addin.GetFullId (addin1.AddinInfo.Namespace, adep.AddinId, null);
				if (depid == id2)
					return true;
				else if (AddinDependsOn (depid, id2))
					return true;
			}
			return false;
		}
		
		public void Repair (IProgressStatus monitor)
		{
			using (fileDatabase.LockWrite ()) {
				try {
					Directory.Delete (AddinCachePath, true);
					Directory.Delete (AddinFolderCachePath, true);
					File.Delete (HostIndexFile);
				}
				catch (Exception ex) {
					monitor.ReportError ("The add-in registry could not be rebuilt. It may be due to lack of write permissions to the directory: " + AddinDbDir, ex);
				}
			}
			Update (monitor);
		}
		
		public void Update (IProgressStatus monitor)
		{
			if (monitor == null)
				monitor = new ConsoleProgressStatus (false);

			if (RunningSetupProcess)
				return;
			
			fatalDatabseError = false;
			
			DateTime tim = DateTime.Now;
			
			Hashtable installed = new Hashtable ();
			bool changesFound = CheckFolders (monitor);
			
			if (monitor.IsCanceled)
				return;
			
			if (monitor.VerboseLog)
				monitor.Log ("Folders checked (" + (int) (DateTime.Now - tim).TotalMilliseconds + " ms)");
			
			if (changesFound) {
				// Something has changed, the add-ins need to be re-scanned, but it has
				// to be done in an external process
				
				using (fileDatabase.LockRead ()) {
					foreach (Addin ainfo in InternalGetInstalledAddins ()) {
						installed [ainfo.Id] = ainfo.Id;
					}
				}
				
				try {
					if (monitor.VerboseLog)
						monitor.Log ("Looking for addins");
					SetupProcess.ExecuteCommand (monitor, registry.RegistryPath, AddinManager.StartupDirectory, "scan");
				}
				catch (Exception ex) {
					fatalDatabseError = true;
					monitor.ReportError ("Add-in scan operation failed", ex);
					monitor.Cancel ();
					return;
				}
				ResetCachedData ();
			}
			
			if (fatalDatabseError)
				monitor.ReportError ("The add-in database could not be updated. It may be due to file corruption. Try running the setup repair utility", null);
			
			// Update the currently loaded add-ins
			if (changesFound && AddinManager.IsInitialized && AddinManager.Registry.RegistryPath == registry.RegistryPath) {
				Hashtable newInstalled = new Hashtable ();
				foreach (Addin ainfo in GetInstalledAddins ()) {
					newInstalled [ainfo.Id] = ainfo.Id;
				}
				
				foreach (string aid in installed.Keys) {
					if (!newInstalled.Contains (aid))
						AddinManager.SessionService.UnloadAddin (aid);
				}
				
				foreach (string aid in newInstalled.Keys) {
					if (!installed.Contains (aid)) {
						AddinManager.SessionService.ActivateAddin (aid);
					}
				}
			}
		}
		
		bool DatabaseInfrastructureCheck (IProgressStatus monitor)
		{
			// Do some sanity check, to make sure the basic database infrastructure can be created
			
			bool hasChanges = false;
			
			try {
			
				if (!Directory.Exists (AddinCachePath)) {
					Directory.CreateDirectory (AddinCachePath);
					hasChanges = true;
				}
			
				if (!Directory.Exists (AddinFolderCachePath)) {
					Directory.CreateDirectory (AddinFolderCachePath);
					hasChanges = true;
				}
			
				// Make sure we can write in those folders

				Util.CheckWrittableFloder (AddinCachePath);
				Util.CheckWrittableFloder (AddinFolderCachePath);
				
				fatalDatabseError = false;
			}
			catch (Exception ex) {
				monitor.ReportError ("Add-in cache directory could not be created", ex);
				fatalDatabseError = true;
				monitor.Cancel ();
			}
			return hasChanges;
		}
		
		
		internal bool CheckFolders (IProgressStatus monitor)
		{
			using (fileDatabase.LockRead ()) {
				AddinScanResult scanResult = new AddinScanResult ();
				scanResult.CheckOnly = true;
				InternalScanFolders (monitor, scanResult);
				return scanResult.ChangesFound;
			}
		}
		
		internal void ScanFolders (IProgressStatus monitor, string folderToScan)
		{
			ScanFolders (monitor, new AddinScanResult ());
		}
		
		internal void ScanFolders (IProgressStatus monitor, AddinScanResult scanResult)
		{
			IDisposable checkLock = null;
			
			if (scanResult.CheckOnly)
				checkLock = fileDatabase.LockRead ();
			else {
				// All changes are done in a transaction, which won't be committed until
				// all files have been updated.
				
				if (!fileDatabase.BeginTransaction ()) {
					// The database is already being updated. Can't do anything for now.
					return;
				}
			}
			
			EventInfo einfo = typeof(AppDomain).GetEvent ("ReflectionOnlyAssemblyResolve");
			ResolveEventHandler resolver = new ResolveEventHandler (OnResolveAddinAssembly);
			
			try
			{
				// Perform the add-in scan
				
				if (!scanResult.CheckOnly) {
					AppDomain.CurrentDomain.AssemblyResolve += resolver;
					if (einfo != null) einfo.AddEventHandler (AppDomain.CurrentDomain, resolver);
				}
				
				InternalScanFolders (monitor, scanResult);
				
				if (!scanResult.CheckOnly)
					fileDatabase.CommitTransaction ();
			}
			catch {
				if (!scanResult.CheckOnly)
					fileDatabase.RollbackTransaction ();
				throw;
			}
			finally {
				currentScanResult = null;
				
				if (scanResult.CheckOnly)
					checkLock.Dispose ();
				else {
					AppDomain.CurrentDomain.AssemblyResolve -= resolver;
					if (einfo != null) einfo.RemoveEventHandler (AppDomain.CurrentDomain, resolver);
				}
			}
		}
		
		void InternalScanFolders (IProgressStatus monitor, AddinScanResult scanResult)
		{
			DateTime tim = DateTime.Now;
			
			DatabaseInfrastructureCheck (monitor);
			if (monitor.IsCanceled)
				return;
			
			try {
				scanResult.HostIndex = GetAddinHostIndex ();
			}
			catch (Exception ex) {
				if (scanResult.CheckOnly) {
					scanResult.ChangesFound = true;
					return;
				}
				monitor.ReportError ("Add-in root index is corrupt. The add-in database will be regenerated.", ex);
				scanResult.RegenerateAllData = true;
			}
			
			AddinScanner scanner = new AddinScanner (this);
			
			// Check if any of the previously scanned folders has been deleted
			
			foreach (string file in Directory.GetFiles (AddinFolderCachePath, "*.data")) {
				AddinScanFolderInfo folderInfo;
				bool res = ReadFolderInfo (monitor, file, out folderInfo);
				if (!res || !Directory.Exists (folderInfo.Folder)) {
					if (res) {
						// Folder has been deleted. Remove the add-ins it had.
						scanner.UpdateDeletedAddins (monitor, folderInfo, scanResult);
					}
					else {
						// Folder info file corrupt. Regenerate all.
						scanResult.ChangesFound = true;
						scanResult.RegenerateRelationData = true;
					}
					
					if (!scanResult.CheckOnly)
						SafeDelete (monitor, file);
					else
						return;
				}
			}
			
			// Look for changes in the add-in folders
			
			foreach (string dir in registry.AddinDirectories) {
				if (dir == registry.DefaultAddinsFolder)
					scanner.ScanFolderRec (monitor, dir, scanResult);
				else
					scanner.ScanFolder (monitor, dir, scanResult);
				if (scanResult.CheckOnly) {
					if (scanResult.ChangesFound || monitor.IsCanceled)
						return;
				}
			}
			
			if (scanResult.CheckOnly)
				return;
			
			// Scan the files which have been modified
			
			currentScanResult = scanResult;

			foreach (FileToScan file in scanResult.FilesToScan)
				scanner.ScanFile (monitor, file.File, file.AddinScanFolderInfo, scanResult);

			// Save folder info
			
			foreach (AddinScanFolderInfo finfo in scanResult.ModifiedFolderInfos)
				SaveFolderInfo (monitor, finfo);

			if (monitor.VerboseLog)
				monitor.Log ("Folders scan completed (" + (int) (DateTime.Now - tim).TotalMilliseconds + " ms)");

			SaveAddinHostIndex ();
			ResetCachedData ();
			
			if (!scanResult.ChangesFound) {
				if (monitor.VerboseLog)
					monitor.Log ("No changes found");
				return;
			}
			
			tim = DateTime.Now;
			try {
				if (scanResult.RegenerateRelationData)
					scanResult.AddinsToUpdateRelations = null;
				
				GenerateAddinExtensionMapsInternal (monitor, scanResult.AddinsToUpdateRelations, scanResult.RemovedAddins);
			}
			catch (Exception ex) {
				fatalDatabseError = true;
				monitor.ReportError ("The add-in database could not be updated. It may be due to file corruption. Try running the setup repair utility", ex);
			}
			
			if (monitor.VerboseLog)
				monitor.Log ("Add-in relations analyzed (" + (int) (DateTime.Now - tim).TotalMilliseconds + " ms)");
			
			SaveAddinHostIndex ();
		}
		
		public void ParseAddin (IProgressStatus progressStatus, string file, string outFile, bool inProcess)
		{
			if (!inProcess) {
				SetupProcess.ExecuteCommand (progressStatus, registry.RegistryPath, AddinManager.StartupDirectory, "get-desc", Path.GetFullPath (file), outFile);
				return;
			}
			
			using (fileDatabase.LockRead ())
			{
				// First of all, check if the file belongs to a registered add-in
				AddinScanFolderInfo finfo;
				if (GetFolderInfoForPath (progressStatus, Path.GetDirectoryName (file), out finfo) && finfo != null) {
					AddinFileInfo afi = finfo.GetAddinFileInfo (file);
					if (afi != null && afi.AddinId != null) {
						AddinDescription adesc;
						if (afi.IsRoot)
							GetHostDescription (progressStatus, afi.AddinId, file, out adesc);
						else
							GetAddinDescription (progressStatus, afi.AddinId, out adesc);
						if (adesc != null)
							adesc.Save (outFile);
						return;
					}
				}
				
				
				AddinScanner scanner = new AddinScanner (this);
				
				SingleFileAssemblyResolver res = new SingleFileAssemblyResolver (progressStatus, registry, scanner);
				ResolveEventHandler resolver = new ResolveEventHandler (res.Resolve);

				EventInfo einfo = typeof(AppDomain).GetEvent ("ReflectionOnlyAssemblyResolve");
				
				try {
					AppDomain.CurrentDomain.AssemblyResolve += resolver;
					if (einfo != null) einfo.AddEventHandler (AppDomain.CurrentDomain, resolver);
				
					AddinDescription desc = scanner.ScanSingleFile (progressStatus, file, new AddinScanResult ());
					if (desc != null)
						desc.Save (outFile);
				}
				finally {
					AppDomain.CurrentDomain.AssemblyResolve -= resolver;
					if (einfo != null) einfo.RemoveEventHandler (AppDomain.CurrentDomain, resolver);
				}
			}
		}
		
		Assembly OnResolveAddinAssembly (object s, ResolveEventArgs args)
		{
			string file = currentScanResult.GetAssemblyLocation (args.Name);
			if (file != null)
				return Util.LoadAssemblyForReflection (file);
			else
				return null;
		}
		
		public string GetFolderConfigFile (string path)
		{
			path = Path.GetFullPath (path);
			
			string s = path.Replace ("_", "__");
			s = s.Replace (Path.DirectorySeparatorChar, '_');
			s = s.Replace (Path.AltDirectorySeparatorChar, '_');
			s = s.Replace (Path.VolumeSeparatorChar, '_');
			
			return Path.Combine (AddinFolderCachePath, s + ".data");
		}
		
		internal void UninstallAddin (IProgressStatus monitor, string addinId, AddinScanResult scanResult)
		{
			scanResult.AddRemovedAddin (addinId);
			string file = GetDescriptionPath (addinId);
			DeleteAddin (monitor, file, scanResult);
		}
		
		internal void UninstallRootAddin (IProgressStatus monitor, string addinId, string addinFile, AddinScanResult scanResult)
		{
			string file = fileDatabase.GetSharedObjectFile (AddinCachePath, addinId, ".mroot", addinFile);
			DeleteAddin (monitor, file, scanResult);
		}
		
		void DeleteAddin (IProgressStatus monitor, string file, AddinScanResult scanResult)
		{
			if (!fileDatabase.Exists (file))
				return;
			
			// Add-in already existed. The dependencies of the old add-in need to be re-analized
						
			AddinDescription desc;
			if (ReadAddinDescription (monitor, file, out desc)) {
				Util.AddDependencies (desc, scanResult);
				if (desc.IsRoot)
					scanResult.HostIndex.RemoveHostData (desc.AddinId, desc.AddinFile);
			} else
				// If we can't get information about the old assembly, just regenerate all relation data
				scanResult.RegenerateRelationData = true;

			SafeDelete (monitor, file);
		}
		
		public bool GetHostDescription (IProgressStatus monitor, string addinId, string fileName, out AddinDescription description)
		{
			try {
				description = AddinDescription.ReadHostBinary (fileDatabase, AddinCachePath, addinId, fileName);
				if (description != null)
					description.OwnerDatabase = this;
				return true;
			}
			catch (Exception ex) {
				if (monitor == null)
					throw;
				description = null;
				monitor.ReportError ("Could not read folder info file", ex);
				return false;
			}
		}
		
		public bool GetAddinDescription (IProgressStatus monitor, string addinId, out AddinDescription description)
		{
			string file = GetDescriptionPath (addinId);
			return ReadAddinDescription (monitor, file, out description);
		}
		
		public bool ReadAddinDescription (IProgressStatus monitor, string file, out AddinDescription description)
		{
			try {
				description = AddinDescription.ReadBinary (fileDatabase, file);
				if (description != null)
					description.OwnerDatabase = this;
				return true;
			}
			catch (Exception ex) {
				if (monitor == null)
					throw;
				description = null;
				monitor.ReportError ("Could not read folder info file", ex);
				return false;
			}
		}
		
		public bool SaveDescription (IProgressStatus monitor, AddinDescription desc, string replaceFileName)
		{
			try {
				if (replaceFileName != null)
					desc.SaveBinary (fileDatabase, replaceFileName);
				else if (desc.IsRoot)
					desc.SaveHostBinary (fileDatabase, AddinCachePath);
				else
					desc.SaveBinary (fileDatabase, GetDescriptionPath (desc.AddinId));
				return true;
			}
			catch (Exception ex) {
				monitor.ReportError ("Add-in info file could not be saved", ex);
				return false;
			}
		}
		
		public bool AddinDescriptionExists (string addinId)
		{
			return fileDatabase.Exists (GetDescriptionPath (addinId));
		}
		
		public bool HostDescriptionExists (string addinId, string sourceFile)
		{
			return fileDatabase.SharedObjectExists (AddinCachePath, addinId, ".mroot", sourceFile);
		}
		
		public bool ReadFolderInfo (IProgressStatus monitor, string file, out AddinScanFolderInfo folderInfo)
		{
			try {
				folderInfo = AddinScanFolderInfo.Read (fileDatabase, file);
				return true;
			}
			catch (Exception ex) {
				folderInfo = null;
				monitor.ReportError ("Could not read folder info file", ex);
				return false;
			}
		}
		
		public bool GetFolderInfoForPath (IProgressStatus monitor, string path, out AddinScanFolderInfo folderInfo)
		{
			try {
				folderInfo = AddinScanFolderInfo.Read (fileDatabase, AddinFolderCachePath, path);
				return true;
			}
			catch (Exception ex) {
				folderInfo = null;
				monitor.ReportError ("Could not read folder info file", ex);
				return false;
			}
		}

		public bool SaveFolderInfo (IProgressStatus monitor, AddinScanFolderInfo folderInfo)
		{
			try {
				folderInfo.Write (fileDatabase, AddinFolderCachePath);
				return true;
			}
			catch (Exception ex) {
				monitor.ReportError ("Could not write folder info file", ex);
				return false;
			}
		}
		
		public bool DeleteFolderInfo (IProgressStatus monitor, AddinScanFolderInfo folderInfo)
		{
			return SafeDelete (monitor, folderInfo.FileName);
		}
		
		public bool SafeDelete (IProgressStatus monitor, string file)
		{
			try {
				fileDatabase.Delete (file);
				return true;
			}
			catch (Exception ex) {
				if (monitor.VerboseLog) {
					monitor.Log ("Could not delete file: " + file);
					monitor.Log (ex.ToString ());
				}
				return false;
			}
		}
		
		AddinHostIndex GetAddinHostIndex ()
		{
			if (hostIndex != null)
				return hostIndex;
			
			using (fileDatabase.LockRead ()) {
				if (fileDatabase.Exists (HostIndexFile))
					hostIndex = AddinHostIndex.Read (fileDatabase, HostIndexFile);
				else
					hostIndex = new AddinHostIndex ();
			}
			return hostIndex;
		}
		
		void SaveAddinHostIndex ()
		{
			if (hostIndex != null)
				hostIndex.Write (fileDatabase, HostIndexFile);
		}
		
		internal string GetUniqueAddinId (string file, string oldId, string ns, string version)
		{
			string baseId = "__" + Path.GetFileNameWithoutExtension (file);

			if (Path.GetExtension (baseId) == ".addin")
				baseId = Path.GetFileNameWithoutExtension (baseId);
			
			string name = baseId;
			string id = Addin.GetFullId (ns, name, version);
			
			// If the old Id is already an automatically generated one, reuse it
			if (oldId != null && oldId.StartsWith (id))
				return name;
			
			int n = 1;
			while (fileDatabase.Exists (GetDescriptionPath (id))) {
				name = baseId + "_" + n;
				id = Addin.GetFullId (ns, name, version);
				n++;
			}
			return name;
		}
		
		public void ResetConfiguration ()
		{
			if (File.Exists (ConfigFile))
				File.Delete (ConfigFile);
		}
		
		DatabaseConfiguration Configuration {
			get {
				if (config == null) {
					using (fileDatabase.LockRead ()) {
						if (fileDatabase.Exists (ConfigFile))
							config = DatabaseConfiguration.Read (ConfigFile);
						else
							config = new DatabaseConfiguration ();
					}
				}
				return config;
			}
		}
		
		void SaveConfiguration ()
		{
			if (config != null) {
				using (fileDatabase.LockWrite ()) {
					config.Write (ConfigFile);
				}
			}
		}
	}
	
	class SingleFileAssemblyResolver
	{
		AddinScanResult scanResult;
		AddinScanner scanner;
		AddinRegistry registry;
		IProgressStatus progressStatus;
		
		public SingleFileAssemblyResolver (IProgressStatus progressStatus, AddinRegistry registry, AddinScanner scanner)
		{
			this.scanner = scanner;
			this.registry = registry;
			this.progressStatus = progressStatus;
		}
		
		public Assembly Resolve (object s, ResolveEventArgs args)
		{
			if (scanResult == null) {
				scanResult = new AddinScanResult ();
				scanResult.LocateAssembliesOnly = true;
			
				foreach (string dir in registry.AddinDirectories)
					scanner.ScanFolder (progressStatus, dir, scanResult);
			}
		
			string afile = scanResult.GetAssemblyLocation (args.Name);
			if (afile != null)
				return Util.LoadAssemblyForReflection (afile);
			else
				return null;
		}
	}
}


