//
// AddinScanner.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Specialized;

using Mono.Addins.Description;

namespace Mono.Addins.Database
{
	class AddinScanner: IDisposable
	{
		AddinDatabase database;
		Dictionary<IAssemblyReflector,object> coreAssemblies = new Dictionary<IAssemblyReflector, object> ();
		IAssemblyLocator assemblyLocator;

		public AddinScanner (AddinDatabase database, IAssemblyLocator locator)
		{
			this.database = database;
			assemblyLocator = locator;
			SetupAssemblyResolver ();
		}

		public void Dispose ()
		{
			TearDownAssemblyResolver ();
			database.FileSystem.CleanupReflector ();
		}

		void SetupAssemblyResolver ()
		{
			ResolveEventHandler resolver = new ResolveEventHandler (OnResolveAddinAssembly);
			AppDomain.CurrentDomain.AssemblyResolve += OnResolveAddinAssembly;

			EventInfo einfo = typeof (AppDomain).GetEvent ("ReflectionOnlyAssemblyResolve");
			if (einfo != null)
				einfo.AddEventHandler (AppDomain.CurrentDomain, resolver);
		}

		void TearDownAssemblyResolver ()
		{
			ResolveEventHandler resolver = new ResolveEventHandler (OnResolveAddinAssembly);
			AppDomain.CurrentDomain.AssemblyResolve -= resolver;

			EventInfo einfo = typeof (AppDomain).GetEvent ("ReflectionOnlyAssemblyResolve");
			if (einfo != null)
				einfo.RemoveEventHandler (AppDomain.CurrentDomain, resolver);
			assemblyLocator = null;
		}

		Assembly OnResolveAddinAssembly (object s, ResolveEventArgs args)
		{
			string[] paths = Environment.GetEnvironmentVariable("MONO_ADDINS_RESOLVER_PATH")?.Split(Path.PathSeparator);
			if (paths != null)
			{
				foreach (string path in paths)
				{
					var assemblyName = new AssemblyName(args.Name).Name;
					var assemblyFileName = Path.Combine(path, assemblyName + ".dll");

					if (File.Exists(assemblyFileName))
					{
						return Util.LoadAssemblyForReflection (assemblyFileName);
					}
				}
			}

			string file = assemblyLocator != null ? assemblyLocator.GetAssemblyLocation (args.Name) : null;
			if (file != null)
				return Util.LoadAssemblyForReflection (file);
			else {
				if (!args.Name.StartsWith ("Mono.Addins.CecilReflector", StringComparison.Ordinal))
					Console.WriteLine ("Assembly not found: " + args.Name);
				return null;
			}
		}

		public void ScanFile (IProgressStatus monitor, FileToScan fileToScan, AddinScanResult scanResult, bool cleanPreScanFile)
		{
			var file = fileToScan.File;
			var folderInfo = fileToScan.AddinScanFolderInfo;

			if (scanResult.ScanContext.IgnorePath (file)) {
				// The file must be ignored. Maybe it caused a crash in a previous scan, or it
				// might be included by a .addin file (in which case it will be scanned when processing
				// the .addin file).
				folderInfo.SetLastScanTime (file, null, false, database.FileSystem.GetLastWriteTime (file), true);
				return;
			}
			
			string scannedAddinId = null;
			bool scannedIsRoot = false;
			bool scanSuccessful = false;
			bool loadedFromScanDataFile = false;
			AddinDescription config = null;
			
			string ext = Path.GetExtension (file).ToLower ();
			bool isAssembly = ext == ".dll" || ext == ".exe";

			string addinScanDataFileMD5 = null;
			var scanDataFile = file + ".addindata";

			if (database.FileSystem.FileExists (scanDataFile)) {
				if (cleanPreScanFile)
					database.FileSystem.DeleteFile (scanDataFile);
				else {
					if (database.ReadAddinDescription (monitor, scanDataFile, out config)) {
						config.SetBasePath (Path.GetDirectoryName (file));
						config.AddinFile = file;
						scanSuccessful = true;
						loadedFromScanDataFile = true;
						addinScanDataFileMD5 = fileToScan.ScanDataMD5; // The md5 for this scan data file should be in the index
						if (monitor.LogLevel > 1)
							monitor.Log ("Loading add-in scan data file: " + file);
					} else if (monitor.LogLevel > 1)
						monitor.Log ("Add-in scan data file could not be loaded, ignoring: " + file);
				}
			}
			if (!loadedFromScanDataFile) {
				if (isAssembly && !Util.IsManagedAssembly (file)) {
					// Ignore dlls and exes which are not managed assemblies
					folderInfo.SetLastScanTime (file, null, false, database.FileSystem.GetLastWriteTime (file), true);
					return;
				}

				if (monitor.LogLevel > 1)
					monitor.Log ("Scanning file: " + file);
			}

			// Log the file to be scanned, so in case of a process crash the main process
			// will know what crashed
			var opMonitor = monitor as IOperationProgressStatus;
			opMonitor?.LogOperationStatus("scan:" + file);

			try {
				if (!loadedFromScanDataFile) {
					if (isAssembly)
						scanSuccessful = ScanAssembly (monitor, file, scanResult.ScanContext, out config);
					else
						scanSuccessful = ScanConfigAssemblies (monitor, file, scanResult.ScanContext, out config);
				}

				if (config != null) {
					
					if (scanSuccessful) {
						// Clean host data from the index. New data will be added.
						if (scanResult.HostIndex != null)
							scanResult.HostIndex.RemoveHostData (config.AddinId, config.AddinFile);
					}

					AddinFileInfo fi = folderInfo.GetAddinFileInfo (file);
					
					// If version is not specified, make up one
					if (config.Version.Length == 0) {
						config.Version = "0.0.0.0";
					}
					
					if (config.LocalId.Length == 0) {
						// Generate an internal id for this add-in
						config.LocalId = database.GetUniqueAddinId (file, fi?.AddinId, config.Namespace, config.Version);
						config.HasUserId = false;
					}
					
					// Check errors in the description
					StringCollection errors = config.Verify (database.FileSystem);
					
					if (database.IsGlobalRegistry && config.AddinId.IndexOf ('.') == -1) {
						errors.Add ("Add-ins registered in the global registry must have a namespace.");
					}
					    
					if (errors.Count > 0) {
						scanSuccessful = false;
						foreach (string err in errors)
							monitor.ReportError (string.Format ("{0}: {1}", file, err), null);
					}
				
					// Make sure all extensions sets are initialized with the correct add-in id
					
					config.SetExtensionsAddinId (config.AddinId);
					
					scanResult.ChangesFound = true;
					
					// If the add-in already existed, try to reuse the relation data it had.
					// Also, the dependencies of the old add-in need to be re-analyzed
					
					AddinDescription existingDescription = null;
					bool res = database.GetAddinDescription (monitor, folderInfo.Domain, config.AddinId, config.AddinFile, out existingDescription);
					
					// If we can't get information about the old assembly, just regenerate all relation data
					if (!res)
						scanResult.RegenerateRelationData = true;
					
					string replaceFileName = null;
					
					if (existingDescription != null) {
						// Reuse old relation data
						config.MergeExternalData (existingDescription);
						Util.AddDependencies (existingDescription, scanResult);
						replaceFileName = existingDescription.FileName;
					}
					
					// If the scanned file results in an add-in version different from the one obtained from
					// previous scans, the old add-in needs to be uninstalled.
					if (fileToScan.OldFileInfo != null && fileToScan.OldFileInfo.IsAddin && fileToScan.OldFileInfo.AddinId != config.AddinId) {
						database.UninstallAddin (monitor, folderInfo.Domain, fileToScan.OldFileInfo.AddinId, fileToScan.OldFileInfo.File, scanResult);
						
						// If the add-in version has changed, regenerate everything again since old data can't be reused
						if (Addin.GetIdName (fileToScan.OldFileInfo.AddinId) == Addin.GetIdName (config.AddinId))
							scanResult.RegenerateRelationData = true;
					}
					
					// If a description could be generated, save it now (if the scan was successful)
					if (scanSuccessful) {
						
						// Assign the domain
						if (config.IsRoot) {
							if (folderInfo.RootsDomain == null) {
								if (scanResult.Domain != null && scanResult.Domain != AddinDatabase.UnknownDomain && scanResult.Domain != AddinDatabase.GlobalDomain)
									folderInfo.RootsDomain = scanResult.Domain;
								else
									folderInfo.RootsDomain = database.GetUniqueDomainId ();
							}
							config.Domain = folderInfo.RootsDomain;
						} else
							config.Domain = folderInfo.Domain;
						
						if (config.IsRoot && scanResult.HostIndex != null) {
							// If the add-in is a root, register its assemblies
							foreach (string f in config.MainModule.Assemblies) {
								string asmFile = Path.Combine (config.BasePath, Util.NormalizePath (f));
								scanResult.HostIndex.RegisterAssembly (asmFile, config.AddinId, config.AddinFile, config.Domain);
							}
						}
						
						// Finally save
						
						if (database.SaveDescription (monitor, config, replaceFileName)) {
							// The new dependencies also have to be updated
							Util.AddDependencies (config, scanResult);
							scanResult.AddAddinToUpdate (config.AddinId);
							scannedAddinId = config.AddinId;
							scannedIsRoot = config.IsRoot;
							return;
						}
					}
				}
			}
			catch (Exception ex) {
				monitor.ReportError ("Unexpected error while scanning file: " + file, ex);
			}
			finally {
				AddinFileInfo ainfo = folderInfo.SetLastScanTime (file, scannedAddinId, scannedIsRoot, database.FileSystem.GetLastWriteTime (file), !scanSuccessful, addinScanDataFileMD5);
				
				if (scanSuccessful && config != null) {
					// Update the ignore list in the folder info object. To be used in the next scan
					foreach (string df in config.AllIgnorePaths) {
						string path = Path.Combine (config.BasePath, Util.NormalizePath (df));
						ainfo.AddPathToIgnore (Path.GetFullPath (path));
					}
				}

				opMonitor?.LogOperationStatus("endscan");
			}
		}
		
		public AddinDescription ScanSingleFile (IProgressStatus monitor, string file, AddinScanResult scanResult)
		{
			AddinDescription config = null;
			
			if (monitor.LogLevel > 1)
				monitor.Log ("Scanning file: " + file);

			var opMonitor = monitor as IOperationProgressStatus;

			opMonitor?.LogOperationStatus ("scan:" + file);
			
			try {
				string ext = Path.GetExtension (file).ToLower ();
				bool scanSuccessful;
				
				if (ext == ".dll" || ext == ".exe")
					scanSuccessful = ScanAssembly (monitor, file, scanResult.ScanContext, out config);
				else
					scanSuccessful = ScanConfigAssemblies (monitor, file, scanResult.ScanContext, out config);

				if (scanSuccessful && config != null) {
					
					config.Domain = "global";
					if (config.Version.Length == 0)
						config.Version = "0.0.0.0";
					
					if (config.LocalId.Length == 0) {
						// Generate an internal id for this add-in
						config.LocalId = database.GetUniqueAddinId (file, "", config.Namespace, config.Version);
					}
				}
			}
			catch (Exception ex) {
				monitor.ReportError ("Unexpected error while scanning file: " + file, ex);
			} finally {
				opMonitor?.LogOperationStatus ("endscan");
			}
			return config;
		}

		public bool ScanConfigAssemblies (IProgressStatus monitor, string filePath, ScanContext scanContext, out AddinDescription config)
		{
			config = null;

			IAssemblyReflector reflector = null;
			try {
				reflector = GetReflector (monitor, assemblyLocator, filePath);

				string basePath = Path.GetDirectoryName (filePath);
				
				using (var s = database.FileSystem.OpenFile (filePath)) {
					config = AddinDescription.Read (s, basePath);
				}
				config.FileName = filePath;
				config.SetBasePath (basePath);
				config.AddinFile = filePath;
				
				return ScanDescription (monitor, reflector, config, null, scanContext);
			}
			catch (Exception ex) {
				// Something went wrong while scanning the assembly. We'll ignore it for now.
				monitor.ReportError ("There was an error while scanning add-in: " + filePath, ex);
				return false;
			}
		}
		
		IAssemblyReflector GetReflector (IProgressStatus monitor, IAssemblyLocator locator, string filePath)
		{
			IAssemblyReflector reflector = database.FileSystem.GetReflectorForFile (locator, filePath);
			object coreAssembly;
			if (!coreAssemblies.TryGetValue (reflector, out coreAssembly)) {
				if (monitor.LogLevel > 1)
					monitor.Log ("Using assembly reflector: " + reflector.GetType ());
				coreAssemblies [reflector] = coreAssembly = reflector.LoadAssembly (GetType().Assembly.Location);
			}
			return reflector;
		}

		public bool ScanAssembly (IProgressStatus monitor, string filePath, ScanContext scanContext, out AddinDescription config)
		{
			config = null;

			IAssemblyReflector reflector = null;
			object asm = null;
			try {
				reflector = GetReflector (monitor, assemblyLocator, filePath);
				asm = reflector.LoadAssembly (filePath);
				if (asm == null)
					throw new Exception ("Could not load assembly: " + filePath);
				
				// Get the config file from the resources, if there is one
				
				if (!ScanEmbeddedDescription (monitor, filePath, reflector, asm, out config))
					return false;
				
				if (config == null || config.IsExtensionModel) {
					// In this case, only scan the assembly if it has the Addin attribute.
					AddinAttribute att = (AddinAttribute) reflector.GetCustomAttribute (asm, typeof(AddinAttribute), false);
					if (att == null) {
						config = null;
						reflector.UnloadAssembly (asm);
						return true;
					}

					if (config == null)
						config = new AddinDescription ();
				}
				
				config.SetBasePath (Path.GetDirectoryName (filePath));
				config.AddinFile = filePath;
				
				string rasmFile = Path.GetFileName (filePath);
				if (!config.MainModule.Assemblies.Contains (rasmFile))
					config.MainModule.Assemblies.Insert (0, rasmFile);
				
				bool res = ScanDescription (monitor, reflector, config, asm, scanContext);
				if (!res)
					reflector.UnloadAssembly (asm);
				return res;
			}
			catch (Exception ex) {
				if (asm != null)
					reflector.UnloadAssembly (asm);
				// Something went wrong while scanning the assembly. We'll ignore it for now.
				monitor.ReportError ("There was an error while scanning assembly: " + filePath, ex);
				return false;
			}
		}

		static bool ScanEmbeddedDescription (IProgressStatus monitor, string filePath, IAssemblyReflector reflector, object asm, out AddinDescription config)
		{
			config = null;
			foreach (string res in reflector.GetResourceNames (asm)) {
				if (res.EndsWith (".addin", StringComparison.Ordinal) || res.EndsWith (".addin.xml", StringComparison.Ordinal)) {
					using (Stream s = reflector.GetResourceStream (asm, res)) {
						AddinDescription ad = AddinDescription.Read (s, Path.GetDirectoryName (filePath));
						if (config != null) {
							if (!config.IsExtensionModel && !ad.IsExtensionModel) {
								// There is more than one add-in definition
								monitor.ReportError ("Duplicate add-in definition found in assembly: " + filePath, null);
								return false;
							}
							config = AddinDescription.Merge (config, ad);
						}
						else
							config = ad;
					}
				}
			}
			return true;
		}

		bool ScanDescription (IProgressStatus monitor, IAssemblyReflector reflector, AddinDescription config, object rootAssembly, ScanContext scanContext)
		{
			// First of all scan the main module
			
			ArrayList assemblies = new ArrayList ();
			
			try {
				string rootAsmFile = null;
				
				if (rootAssembly != null) {
					ScanAssemblyAddinHeaders (reflector, config, rootAssembly);
					ScanAssemblyImports (reflector, config.MainModule, rootAssembly);
					assemblies.Add (rootAssembly);
					rootAsmFile = Path.GetFileName (config.AddinFile);
				}
				
				// The assembly list may be modified while scanning the headers, so
				// we use a for loop instead of a foreach
				for (int n=0; n<config.MainModule.Assemblies.Count; n++) {
					string s = config.MainModule.Assemblies [n];
					string asmFile = Path.GetFullPath (Path.Combine (config.BasePath, Util.NormalizePath (s)));
					scanContext.AddPathToIgnore (asmFile);
					if (s == rootAsmFile || config.MainModule.IgnorePaths.Contains (s))
						continue;
					object asm = reflector.LoadAssembly (asmFile);
					assemblies.Add (asm);
					ScanAssemblyAddinHeaders (reflector, config, asm);
					ScanAssemblyImports (reflector, config.MainModule, asm);
				}
				
				// Add all data files to the ignore file list. It avoids scanning assemblies
				// which are included as 'data' in an add-in.
				foreach (string df in config.MainModule.DataFiles) {
					string file = Path.Combine (config.BasePath, Util.NormalizePath (df));
					scanContext.AddPathToIgnore (Path.GetFullPath (file));
				}
				foreach (string df in config.MainModule.IgnorePaths) {
					string path = Path.Combine (config.BasePath, Util.NormalizePath (df));
					scanContext.AddPathToIgnore (Path.GetFullPath (path));
				}
				
				// The add-in id and version must be already assigned at this point
				
				foreach (object asm in assemblies)
					ScanAssemblyContents (reflector, config, config.MainModule, asm);
				
			} catch (Exception ex) {
				ReportReflectionException (monitor, ex, config);
				return false;
			}
			
			// Extension node types may have child nodes declared as attributes. Find them.
			
			Hashtable internalNodeSets = new Hashtable ();
			
			var setsCopy = new List<ExtensionNodeSet> ();
			setsCopy.AddRange (config.ExtensionNodeSets);
			foreach (ExtensionNodeSet eset in setsCopy)
				ScanNodeSet (reflector, config, eset, assemblies, internalNodeSets);
			
			foreach (ExtensionPoint ep in config.ExtensionPoints) {
				ScanNodeSet (reflector, config, ep.NodeSet, assemblies, internalNodeSets);
			}
		
			// Now scan all modules
			
			if (!config.IsRoot) {
				foreach (ModuleDescription mod in config.OptionalModules) {
					try {
						var asmList = new List<Tuple<string,object>> ();
						for (int n=0; n<mod.Assemblies.Count; n++) {
							string s = mod.Assemblies [n];
							if (mod.IgnorePaths.Contains (s))
								continue;
							string asmFile = Path.Combine (config.BasePath, Util.NormalizePath (s));
							object asm = reflector.LoadAssembly (asmFile);
							asmList.Add (new Tuple<string,object> (asmFile,asm));
							scanContext.AddPathToIgnore (Path.GetFullPath (asmFile));
							ScanAssemblyImports (reflector, mod, asm);
						}
						// Add all data files to the ignore file list. It avoids scanning assemblies
						// which are included as 'data' in an add-in.
						foreach (string df in mod.DataFiles) {
							string file = Path.Combine (config.BasePath, Util.NormalizePath (df));
							scanContext.AddPathToIgnore (Path.GetFullPath (file));
						}
						foreach (string df in mod.IgnorePaths) {
							string path = Path.Combine (config.BasePath, Util.NormalizePath (df));
							scanContext.AddPathToIgnore (Path.GetFullPath (path));
						}
						
						foreach (var asm in asmList)
							ScanSubmodule (monitor, mod, reflector, config, asm.Item1, asm.Item2);

					} catch (Exception ex) {
						ReportReflectionException (monitor, ex, config);
					}
				}
			}

			// Fix up ModuleDescription so it adds assembly names.
			foreach (ModuleDescription module in config.AllModules) { 
				foreach (var s in module.Assemblies) { 
					string asmFile = Path.Combine (config.BasePath, Util.NormalizePath (s));
					var asm = AssemblyName.GetAssemblyName (asmFile);
					module.AssemblyNames.Add (asm.FullName);
				}
			}

			config.StoreFileInfo ();
			return true;
		}

		bool ScanSubmodule (IProgressStatus monitor, ModuleDescription mod, IAssemblyReflector reflector, AddinDescription config, string assemblyName, object asm)
		{
			AddinDescription mconfig;
			ScanEmbeddedDescription (monitor, assemblyName, reflector, asm, out mconfig);
			if (mconfig != null) {
				if (!mconfig.IsExtensionModel) {
					monitor.ReportError ("Submodules can't define new add-ins: " + assemblyName, null);
					return false;
				}
				if (mconfig.OptionalModules.Count != 0) {
					monitor.ReportError ("Submodules can't define nested submodules: " + assemblyName, null);
					return false;
				}
				if (mconfig.ConditionTypes.Count != 0) {
					monitor.ReportError ("Submodules can't define condition types: " + assemblyName, null);
					return false;
				}
				if (mconfig.ExtensionNodeSets.Count != 0) {
					monitor.ReportError ("Submodules can't define extension node sets: " + assemblyName, null);
					return false;
				}
				if (mconfig.ExtensionPoints.Count != 0) {
					monitor.ReportError ("Submodules can't define extension points sets: " + assemblyName, null);
					return false;
				}
				mod.MergeWith (mconfig.MainModule);
			}
			ScanAssemblyContents (reflector, config, mod, asm);
			return true;
		}

		void ReportReflectionException (IProgressStatus monitor, Exception ex, AddinDescription config)
		{
			monitor.ReportWarning ("[" + config.AddinId + "] Could not load some add-in assemblies: " + ex.Message);
			if (monitor.LogLevel <= 1)
			    return;
			
			ReflectionTypeLoadException rex = ex as ReflectionTypeLoadException;
			if (rex != null) {
				foreach (Exception e in rex.LoaderExceptions)
					monitor.Log ("Load exception: " + e);
			}
		}
		
		void ScanAssemblyAddinHeaders (IAssemblyReflector reflector, AddinDescription config, object asm)
		{
			// Get basic add-in information
			AddinAttribute att = (AddinAttribute) reflector.GetCustomAttribute (asm, typeof(AddinAttribute), false);
			if (att != null) {
				if (att.Id.Length > 0)
					config.LocalId = att.Id;
				if (att.Version.Length > 0)
					config.Version = att.Version;
				if (att.Namespace.Length > 0)
					config.Namespace = att.Namespace;
				if (att.Category.Length > 0)
					config.Category = att.Category;
				if (att.CompatVersion.Length > 0)
					config.CompatVersion = att.CompatVersion;
				if (att.Url.Length > 0)
					config.Url = att.Url;
				config.IsRoot = att is AddinRootAttribute;
				config.EnabledByDefault = att.EnabledByDefault;
				config.Flags = att.Flags;
			}
			
			// Author attributes
			
			object[] atts = reflector.GetCustomAttributes (asm, typeof(AddinAuthorAttribute), false);
			foreach (AddinAuthorAttribute author in atts) {
				if (config.Author.Length == 0)
					config.Author = author.Name;
				else
					config.Author += ", " + author.Name;
			}
			
			// Name
			
			atts = reflector.GetCustomAttributes (asm, typeof(AddinNameAttribute), false);
			foreach (AddinNameAttribute at in atts) {
				if (string.IsNullOrEmpty (at.Locale))
					config.Name = at.Name;
				else
					config.Properties.SetPropertyValue ("Name", at.Name, at.Locale);
			}
			
			// Description
			
			object catt = reflector.GetCustomAttribute (asm, typeof(AssemblyDescriptionAttribute), false);
			if (catt != null && config.Description.Length == 0)
				config.Description = ((AssemblyDescriptionAttribute)catt).Description;
			
			atts = reflector.GetCustomAttributes (asm, typeof(AddinDescriptionAttribute), false);
			foreach (AddinDescriptionAttribute at in atts) {
				if (string.IsNullOrEmpty (at.Locale))
					config.Description = at.Description;
				else
					config.Properties.SetPropertyValue ("Description", at.Description, at.Locale);
			}
			
			// Copyright
			
			catt = reflector.GetCustomAttribute (asm, typeof(AssemblyCopyrightAttribute), false);
			if (catt != null && config.Copyright.Length == 0)
				config.Copyright = ((AssemblyCopyrightAttribute)catt).Copyright;
			
			// Category

			catt = reflector.GetCustomAttribute (asm, typeof(AddinCategoryAttribute), false);
			if (catt != null && config.Category.Length == 0)
				config.Category = ((AddinCategoryAttribute)catt).Category;
			
			// Url

			catt = reflector.GetCustomAttribute (asm, typeof(AddinUrlAttribute), false);
			if (catt != null && config.Url.Length == 0)
				config.Url = ((AddinUrlAttribute)catt).Url;
			
			// Flags

			catt = reflector.GetCustomAttribute (asm, typeof(AddinFlagsAttribute), false);
			if (catt != null)
				config.Flags |= ((AddinFlagsAttribute)catt).Flags;

			// Localizer
			
			AddinLocalizerGettextAttribute locat = (AddinLocalizerGettextAttribute) reflector.GetCustomAttribute (asm, typeof(AddinLocalizerGettextAttribute), false);
			if (locat != null) {
				ExtensionNodeDescription node = new ExtensionNodeDescription ("Localizer");
				node.SetAttribute ("type", "Gettext");
				if (!string.IsNullOrEmpty (locat.Catalog))
					node.SetAttribute ("catalog", locat.Catalog);
				if (!string.IsNullOrEmpty (locat.Location))
					node.SetAttribute ("location", locat.Location);
				config.Localizer = node;
			}

			var customLocat = (AddinLocalizerAttribute) reflector.GetCustomAttribute (asm, typeof(AddinLocalizerAttribute), false);
			if (customLocat != null) {
				var node = new ExtensionNodeDescription ("Localizer");

				node.SetAttribute ("type", customLocat.TypeName);

				config.Localizer = node;
			}
			
			// Optional modules
			
			atts = reflector.GetCustomAttributes (asm, typeof(AddinModuleAttribute), false);
			foreach (AddinModuleAttribute mod in atts) {
				if (mod.AssemblyFile.Length > 0) {
					ModuleDescription module = new ModuleDescription ();
					module.Assemblies.Add (mod.AssemblyFile);
					config.OptionalModules.Add (module);
				}
			}
		}
		
		void ScanAssemblyImports (IAssemblyReflector reflector, ModuleDescription module, object asm)
		{
			object[] atts = reflector.GetCustomAttributes (asm, typeof(ImportAddinAssemblyAttribute), false);
			foreach (ImportAddinAssemblyAttribute import in atts) {
				if (!string.IsNullOrEmpty (import.FilePath)) {
					module.Assemblies.Add (import.FilePath);
					if (!import.Scan)
						module.IgnorePaths.Add (import.FilePath);
				}
			}
			atts = reflector.GetCustomAttributes (asm, typeof(ImportAddinFileAttribute), false);
			foreach (ImportAddinFileAttribute import in atts) {
				if (!string.IsNullOrEmpty (import.FilePath))
					module.DataFiles.Add (import.FilePath);
			}
		}
		
		void ScanAssemblyContents (IAssemblyReflector reflector, AddinDescription config, ModuleDescription module, object asm)
		{
			bool isMainModule = module == config.MainModule;
			
			// Get dependencies
			
			object[] deps = reflector.GetCustomAttributes (asm, typeof(AddinDependencyAttribute), false);
			foreach (AddinDependencyAttribute dep in deps) {
				AddinDependency adep = new AddinDependency ();
				adep.AddinId = dep.Id;
				adep.Version = dep.Version;
				module.Dependencies.Add (adep);
			}
			
			if (isMainModule) {
				
				// Get properties
				
				object[] props = reflector.GetCustomAttributes (asm, typeof(AddinPropertyAttribute), false);
				foreach (AddinPropertyAttribute prop in props)
					config.Properties.SetPropertyValue (prop.Name, prop.Value, prop.Locale);
			
				// Get extension points
				
				object[] extPoints = reflector.GetCustomAttributes (asm, typeof(ExtensionPointAttribute), false);
				foreach (ExtensionPointAttribute ext in extPoints) {
					ExtensionPoint ep = config.AddExtensionPoint (ext.Path);
					ep.Description = ext.Description;
					ep.Name = ext.Name;
					ep.DefaultInsertBefore = ext.DefaultInsertBefore;
					ep.DefaultInsertAfter = ext.DefaultInsertAfter;
					ExtensionNodeType nt = ep.AddExtensionNode (ext.NodeName, ext.NodeTypeName);
					nt.ExtensionAttributeTypeName = ext.ExtensionAttributeTypeName;
				}
			}
			
			// Look for extension nodes declared using assembly attributes
			
			foreach (CustomAttribute att in reflector.GetRawCustomAttributes (asm, typeof(CustomExtensionAttribute), true))
				AddCustomAttributeExtension (module, att, "Type", null);
			
			// Get extensions or extension points applied to types
			
			foreach (object t in reflector.GetAssemblyTypes (asm)) {

				string typeFullName = reflector.GetTypeFullName (t);
				string typeQualifiedName = reflector.GetTypeAssemblyQualifiedName (t);

				//condition attributes apply independently but identically to all extension attributes on this node
				//depending on ordering is too messy due to inheritance etc
				var conditionAtts = new Lazy<List<CustomAttribute>> (() => reflector.GetRawCustomAttributes (t, typeof (CustomConditionAttribute), false));

				// Look for extensions

				object[] extensionAtts = reflector.GetCustomAttributes (t, typeof(ExtensionAttribute), false);
				if (extensionAtts.Length > 0) {
					Dictionary<string,ExtensionNodeDescription> nodes = new Dictionary<string, ExtensionNodeDescription> ();
					ExtensionNodeDescription uniqueNode = null;
					foreach (ExtensionAttribute eatt in extensionAtts) {
						string path;
						string nodeName = eatt.NodeName;
						
						if (eatt.TypeName.Length > 0) {
							path = "$" + eatt.TypeFullName;
						}
						else if (eatt.Path.Length == 0) {
							path = GetBaseTypeNameList (reflector, t);
							if (path == "$") {
								// The type does not implement any interface and has no superclass.
								// Will be reported later as an error.
								path = "$" + typeFullName;
							}
						} else {
							path = eatt.Path;
						}

						ExtensionNodeDescription elem = AddConditionedExtensionNode (module, path, nodeName, conditionAtts.Value);
						nodes [path] = elem;
						uniqueNode = elem;
						
						if (eatt.Id.Length > 0) {
							elem.SetAttribute ("id", eatt.Id);
							elem.SetAttribute ("type", typeQualifiedName);
						} else {
							elem.SetAttribute ("id", typeFullName);
							elem.SetAttribute ("type", typeQualifiedName);
						}
						if (eatt.InsertAfter.Length > 0)
							elem.SetAttribute ("insertafter", eatt.InsertAfter);
						if (eatt.InsertBefore.Length > 0)
							elem.SetAttribute ("insertbefore", eatt.InsertBefore);
					}
					
					// Get the node attributes
					
					foreach (ExtensionAttributeAttribute eat in reflector.GetCustomAttributes (t, typeof(ExtensionAttributeAttribute), false)) {
						ExtensionNodeDescription node;
						if (!string.IsNullOrEmpty (eat.Path))
							nodes.TryGetValue (eat.Path, out node);
						else if (eat.TypeName.Length > 0)
							nodes.TryGetValue ("$" + eat.TypeName, out node);
						else {
							if (nodes.Count > 1)
								throw new Exception ("Missing type or extension path value in ExtensionAttribute for type '" + typeQualifiedName + "'.");
							node = uniqueNode;
						}
						if (node == null)
							throw new Exception ("Invalid type or path value in ExtensionAttribute for type '" + typeQualifiedName + "'.");
							
						node.SetAttribute (eat.Name ?? string.Empty, eat.Value ?? string.Empty);
					}
				}
				else {
					// Look for extension points
					
					extensionAtts = reflector.GetCustomAttributes (t, typeof(TypeExtensionPointAttribute), false);
					if (extensionAtts.Length > 0 && isMainModule) {
						foreach (TypeExtensionPointAttribute epa in extensionAtts) {
							ExtensionPoint ep;
							
							ExtensionNodeType nt = new ExtensionNodeType ();
							
							if (epa.Path.Length > 0) {
								ep = config.AddExtensionPoint (epa.Path);
							}
							else {
								ep = config.AddExtensionPoint (GetDefaultTypeExtensionPath (config, typeFullName));
								nt.ObjectTypeName = typeQualifiedName;
							}
							nt.Id = epa.NodeName;
							nt.TypeName = epa.NodeTypeName;
							nt.ExtensionAttributeTypeName = epa.ExtensionAttributeTypeName;
							ep.NodeSet.NodeTypes.Add (nt);
							ep.Description = epa.Description;
							ep.Name = epa.Name;
							ep.RootAddin = config.AddinId;
							ep.SetExtensionsAddinId (config.AddinId);
						}
					}
					else {
						// Look for custom extension attribtues
						foreach (CustomAttribute att in reflector.GetRawCustomAttributes (t, typeof(CustomExtensionAttribute), false)) {
							ExtensionNodeDescription elem = AddCustomAttributeExtension (module, att, "Type", conditionAtts.Value);
							elem.SetAttribute ("type", typeQualifiedName);
							if (string.IsNullOrEmpty (elem.GetAttribute ("id")))
								elem.SetAttribute ("id", typeQualifiedName);
						}
					}
				}
			}
		}

		static ExtensionNodeDescription AddConditionedExtensionNode (ModuleDescription module, string path, string nodeName, List<CustomAttribute> conditionAtts)
		{
			if (conditionAtts == null || conditionAtts.Count == 0) {
				return module.AddExtensionNode (path, nodeName);
			}

			ExtensionNodeDescription conditionNode;

			if (conditionAtts.Count == 1) {
				conditionNode = CreateConditionNode (conditionAtts[0]);
				module.GetExtension (path).ExtensionNodes.Add (conditionNode);
			}
			else {
				conditionNode = new ExtensionNodeDescription ("ComplexCondition");
				ExtensionNodeDescription andNode = new ExtensionNodeDescription ("And");
				conditionNode.ChildNodes.Add (andNode);
				foreach (var catt in conditionAtts) {
					var cnode = CreateConditionNode (catt);
					andNode.ChildNodes.Add (cnode);
				}
			}

			var node = new ExtensionNodeDescription (nodeName);
			conditionNode.ChildNodes.Add (node);
			return node;
		}

		static ExtensionNodeDescription CreateConditionNode (CustomAttribute conditionAtt)
		{
			var conditionNode = new ExtensionNodeDescription ("Condition");

			var id = GetConditionId (conditionAtt);
			conditionNode.SetAttribute ("id", id);

			foreach (KeyValuePair<string, string> prop in conditionAtt) {
				if (string.IsNullOrEmpty (prop.Key)) {
					throw new Exception ("Empty key in attribute '" + conditionAtt.TypeName + "'.");
				}
				conditionNode.SetAttribute (prop.Key, prop.Value);
			}

			return conditionNode;
		}

		static string GetConditionId (CustomAttribute conditionAtt)
		{
			var id = conditionAtt.TypeName;
			var start = id.LastIndexOf ('.') + 1;
			int length = id.Length - start;

			if (id.EndsWith ("ConditionAttribute", StringComparison.Ordinal)) {
				length -= "ConditionAttribute".Length;
			} else if (id.EndsWith ("Attribute", StringComparison.Ordinal)) {
				length -= "Attribute".Length;
			}

			id = id.Substring (start, length);
			return id;
		}

		ExtensionNodeDescription AddCustomAttributeExtension (ModuleDescription module, CustomAttribute att, string nameName, List<CustomAttribute> conditionAtts)
		{
			string path;
			if (!att.TryGetValue (CustomExtensionAttribute.PathFieldKey, out path))
				path = "%" + att.TypeName;
			ExtensionNodeDescription elem = AddConditionedExtensionNode (module, path, nameName, conditionAtts);
			foreach (KeyValuePair<string,string> prop in att) {
				if (string.IsNullOrEmpty (prop.Key)) {
					throw new Exception ("Empty key in attribute '" + att.TypeName + "'.");
				}
				if (prop.Key != CustomExtensionAttribute.PathFieldKey)
					elem.SetAttribute (prop.Key, prop.Value);
			}
			return elem;
		}
		
		void ScanNodeSet (IAssemblyReflector reflector, AddinDescription config, ExtensionNodeSet nset, ArrayList assemblies, Hashtable internalNodeSets)
		{
			foreach (ExtensionNodeType nt in nset.NodeTypes)
				ScanNodeType (reflector, config, nt, assemblies, internalNodeSets);
		}
		
		void ScanNodeType (IAssemblyReflector reflector, AddinDescription config, ExtensionNodeType nt, ArrayList assemblies, Hashtable internalNodeSets)
		{
			if (nt.TypeName.Length == 0)
				nt.TypeName = typeof (TypeExtensionNode).AssemblyQualifiedName;
			
			object ntype = FindAddinType (reflector, nt.TypeName, assemblies);
			if (ntype == null)
				return;
			
			// Add type information declared with attributes in the code
			ExtensionNodeAttribute nodeAtt = (ExtensionNodeAttribute) reflector.GetCustomAttribute (ntype, typeof(ExtensionNodeAttribute), true);
			if (nodeAtt != null) {
				if (nt.Id.Length == 0 && nodeAtt.NodeName.Length > 0)
					nt.Id = nodeAtt.NodeName;
				if (nt.Description.Length == 0 && nodeAtt.Description.Length > 0)
					nt.Description = nodeAtt.Description;
				if (nt.ExtensionAttributeTypeName.Length == 0 && nodeAtt.ExtensionAttributeTypeName.Length > 0)
					nt.ExtensionAttributeTypeName = nodeAtt.ExtensionAttributeTypeName;
			} else {
				// Use the node type name as default name
				if (nt.Id.Length == 0)
					nt.Id = reflector.GetTypeName (ntype);
			}
			
			// Add information about attributes
			object[] fieldAtts = reflector.GetCustomAttributes (ntype, typeof(NodeAttributeAttribute), true);
			foreach (NodeAttributeAttribute fatt in fieldAtts) {
				NodeTypeAttribute natt = new NodeTypeAttribute ();
				natt.Name = fatt.Name;
				natt.Required = fatt.Required;
				if (fatt.TypeName != null)
					natt.Type = fatt.TypeName;
				if (fatt.Description.Length > 0)
					natt.Description = fatt.Description;
				nt.Attributes.Add (natt);
			}
			
			// Check if the type has NodeAttribute attributes applied to fields.
			foreach (object field in reflector.GetFields (ntype)) {
				NodeAttributeAttribute fatt = (NodeAttributeAttribute) reflector.GetCustomAttribute (field, typeof(NodeAttributeAttribute), false);
				if (fatt != null) {
					NodeTypeAttribute natt = new NodeTypeAttribute ();
					if (fatt.Name.Length > 0)
						natt.Name = fatt.Name;
					else
						natt.Name = reflector.GetFieldName (field);
					if (fatt.Description.Length > 0)
						natt.Description = fatt.Description;
					natt.Type = reflector.GetFieldTypeFullName (field);
					natt.Required = fatt.Required;
					nt.Attributes.Add (natt);
				}
			}
			
			// Check if the extension type allows children by looking for [ExtensionNodeChild] attributes.
			// First of all, look in the internalNodeSets hashtable, which is being used as cache
			
			string childSet = (string) internalNodeSets [nt.TypeName];
			
			if (childSet == null) {
				object[] ats = reflector.GetCustomAttributes (ntype, typeof(ExtensionNodeChildAttribute), true);
				if (ats.Length > 0) {
					// Create a new node set for this type. It is necessary to create a new node set
					// instead of just adding child ExtensionNodeType objects to the this node type
					// because child types references can be recursive.
					ExtensionNodeSet internalSet = new ExtensionNodeSet ();
					internalSet.Id = reflector.GetTypeName (ntype) + "_" + Guid.NewGuid().ToString ();
					foreach (ExtensionNodeChildAttribute at in ats) {
						ExtensionNodeType internalType = new ExtensionNodeType ();
						internalType.Id = at.NodeName;
						internalType.TypeName = at.ExtensionNodeTypeName;
						internalSet.NodeTypes.Add (internalType);
					}
					config.ExtensionNodeSets.Add (internalSet);
					nt.NodeSets.Add (internalSet.Id);
					
					// Register the new set in a hashtable, to allow recursive references to the
					// same internal set.
					internalNodeSets [nt.TypeName] = internalSet.Id;
					internalNodeSets [reflector.GetTypeAssemblyQualifiedName (ntype)] = internalSet.Id;
					ScanNodeSet (reflector, config, internalSet, assemblies, internalNodeSets);
				}
			}
			else {
				if (childSet.Length == 0) {
					// The extension type does not declare children.
					return;
				}
				// The extension type can have children. The allowed children are
				// defined in this extension set.
				nt.NodeSets.Add (childSet);
				return;
			}
			
			ScanNodeSet (reflector, config, nt, assemblies, internalNodeSets);
		}
		
		string GetBaseTypeNameList (IAssemblyReflector reflector, object type)
		{
			StringBuilder sb = new StringBuilder ("$");
			foreach (string tn in reflector.GetBaseTypeFullNameList (type))
				sb.Append (tn).Append (',');
			if (sb.Length > 0)
				sb.Remove (sb.Length - 1, 1);
			return sb.ToString ();
		}
		
		object FindAddinType (IAssemblyReflector reflector, string type, ArrayList assemblies)
		{
			if (!Util.TryParseTypeName (type, out var typeName, out var assemblyName))
				return null;

			if (!string.IsNullOrEmpty (assemblyName) && assemblyName != "Mono.Addins") {
				// Look in the specified assembly
				foreach (var a in assemblies) {
					if (reflector.GetAssemblyName (a) == assemblyName)
						return reflector.GetType (a, typeName);
				}
				return null;
			}

			// Look in the current assembly
			object etype = reflector.GetType (coreAssemblies [reflector], typeName);
			if (etype != null)
				return etype;
			
			// Look in referenced assemblies
			foreach (object asm in assemblies) {
				etype = reflector.GetType (asm, typeName);
				if (etype != null)
					return etype;
			}
			
			Hashtable visited = new Hashtable ();
			
			// Look in indirectly referenced assemblies
			foreach (object asm in assemblies) {
				foreach (object aref in reflector.GetAssemblyReferences (asm)) {
					if (visited.Contains (aref))
						continue;
					visited.Add (aref, aref);
					object rasm = reflector.LoadAssemblyFromReference (aref);
					if (rasm != null) {
						etype = reflector.GetType (rasm, typeName);
						if (etype != null)
							return etype;
					}
				}
			}
			return null;
		}

		void RegisterTypeNode (AddinDescription config, ExtensionAttribute eatt, string path, string nodeName, string typeFullName)
		{
			ExtensionNodeDescription elem = config.MainModule.AddExtensionNode (path, nodeName);
			if (eatt.Id.Length > 0) {
				elem.SetAttribute ("id", eatt.Id);
				elem.SetAttribute ("type", typeFullName);
			} else {
				elem.SetAttribute ("id", typeFullName);
			}
			if (eatt.InsertAfter.Length > 0)
				elem.SetAttribute ("insertafter", eatt.InsertAfter);
			if (eatt.InsertBefore.Length > 0)
				elem.SetAttribute ("insertbefore", eatt.InsertBefore);
		}
		
		string GetDefaultTypeExtensionPath (AddinDescription desc, string typeFullName)
		{
			return "/" + Addin.GetIdName (desc.AddinId) + "/TypeExtensions/" + typeFullName;
		}
	}
}
