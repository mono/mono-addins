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
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Specialized;
using System.Xml;
using System.ComponentModel;

using Mono.Addins.Description;

namespace Mono.Addins.Database
{
	class AddinScanner: MarshalByRefObject
	{
		AddinDatabase database;
		
		public AddinScanner (AddinDatabase database)
		{
			this.database = database;
		}
		
		public void ScanFolder (IProgressStatus monitor, string path, AddinScanResult scanResult)
		{
			path = Util.GetFullPath (path);
			
			// Avoid folders including each other
			if (!scanResult.VisitFolder (path))
				return;
			
			if (monitor.VerboseLog && !scanResult.LocateAssembliesOnly)
				monitor.Log ("Checking: " + path);
			
			AddinScanFolderInfo folderInfo;
			if (!database.GetFolderInfoForPath (monitor, path, out folderInfo)) {
				// folderInfo file was corrupt.
				// Just in case, we are going to regenerate all relation data.
				if (!Directory.Exists (path))
					scanResult.RegenerateRelationData = true;
			} else {
				if (folderInfo == null && !Directory.Exists (path))
					return;
			}
			
			if (folderInfo == null)
				folderInfo = new AddinScanFolderInfo (path);
			
			if (Directory.Exists (path))
			{
				foreach (string file in Directory.GetFiles (path)) {
					if (file.EndsWith (".addin.xml")) {
						RegisterFileToScan (monitor, file, scanResult, folderInfo);
						continue;
					}
					switch (Path.GetExtension (file)) {
					case ".dll":
					case ".exe":
						RegisterFileToScan (monitor, file, scanResult, folderInfo);
						scanResult.AddAssemblyLocation (file);
						break;
					case ".addin":
						RegisterFileToScan (monitor, file, scanResult, folderInfo);
						break;
					case ".addins":
						ScanAddinsFile (monitor, file, scanResult);
						break;
					}
				}
			}
			else if (!scanResult.LocateAssembliesOnly) {
				// The folder has been deleted. All add-ins defined in that folder should also be deleted.
				scanResult.RegenerateRelationData = true;
				scanResult.ChangesFound = true;
				if (scanResult.CheckOnly)
					return;
				database.DeleteFolderInfo (monitor, folderInfo);
			}
			
			if (scanResult.LocateAssembliesOnly)
				return;
			
			// Look for deleted add-ins.
			
			UpdateDeletedAddins (monitor, folderInfo, scanResult);
		}
		
		public void UpdateDeletedAddins (IProgressStatus monitor, AddinScanFolderInfo folderInfo, AddinScanResult scanResult)
		{
			ArrayList missing = folderInfo.GetMissingAddins ();
			if (missing.Count > 0) {
				if (Directory.Exists (folderInfo.Folder))
					scanResult.ModifiedFolderInfos.Add (folderInfo);
				scanResult.ChangesFound = true;
				if (scanResult.CheckOnly)
					return;
					
				foreach (AddinFileInfo info in missing) {
					if (info.IsRoot)
						database.UninstallRootAddin (monitor, info.AddinId, info.File, scanResult);
					else
						database.UninstallAddin (monitor, info.AddinId, scanResult);
				}
			}
		}
		
		void RegisterFileToScan (IProgressStatus monitor, string file, AddinScanResult scanResult, AddinScanFolderInfo folderInfo)
		{
			if (scanResult.LocateAssembliesOnly)
				return;

			AddinFileInfo finfo = folderInfo.GetAddinFileInfo (file);
			bool added = false;
			   
			if (finfo != null && File.GetLastWriteTime (file) == finfo.LastScan && !scanResult.RegenerateAllData) {
				if (finfo.ScanError) {
					// Always schedule the file for scan if there was an error in a previous scan.
					// However, don't set ChangesFound=true, in this way if there isn't any other
					// change in the registry, the file won't be scanned again.
					scanResult.AddFileToScan (file, folderInfo);
					added = true;
				}
			
				if (finfo.AddinId == null || finfo.AddinId.Length == 0)
					return;
				if (!finfo.IsRoot) {
					if (database.AddinDescriptionExists (finfo.AddinId))
						return;
				} else {
					if (database.HostDescriptionExists (finfo.AddinId, file))
						return;
				}
			}
			
			scanResult.ChangesFound = true;
			
			if (!scanResult.CheckOnly && !added)
				scanResult.AddFileToScan (file, folderInfo);
		}
		
		public void ScanFile (IProgressStatus monitor, string file, AddinScanFolderInfo folderInfo, AddinScanResult scanResult)
		{
			if (monitor.VerboseLog)
				monitor.Log ("Scanning file: " + file);
				
			string scannedAddinId = null;
			bool scannedIsRoot = false;
			bool scanSuccessful = false;
			
			try {
				string ext = Path.GetExtension (file);
				AddinDescription config = null;
				
				if (ext == ".dll" || ext == ".exe")
					scanSuccessful = ScanAssembly (monitor, file, scanResult, out config);
				else
					scanSuccessful = ScanConfigAssemblies (monitor, file, scanResult, out config);

				if (config != null && config.AddinId.Length > 0) {
					
					AddinFileInfo fi = folderInfo.GetAddinFileInfo (file);
					
					// If version is not specified, make up one
					if (config.Version.Length == 0) {
						config.Version = "0.0.0.0";
					}
					
					if (config.LocalId.Length == 0) {
						// Generate an internal id for this add-in
						config.LocalId = database.GetUniqueAddinId (file, (fi != null ? fi.AddinId : null), config.Namespace, config.Version);
					}
					
					// Check errors in the description
					StringCollection errors = config.Verify ();
					
					if (database.IsGlobalRegistry && config.AddinId.IndexOf ('.') == -1) {
						errors.Add ("Add-ins registered in the global registry must have a namespace.");
					}
					    
					if (errors.Count > 0) {
						scanSuccessful = false;
						monitor.ReportError ("Errors found in add-in '" + file + ":", null);
						foreach (string err in errors)
							monitor.ReportError (err, null);
					}
				
					// Make sure all extensions sets are initialized with the correct add-in id
					
					config.SetExtensionsAddinId (config.AddinId);
					
					scanResult.ChangesFound = true;
					
					// If the add-in already existed, try to reuse the relation data it had.
					// Also, the dependencies of the old add-in need to be re-analized
					
					AddinDescription existingDescription = null;
					bool res;
					
					if (config.IsRoot)
						res = database.GetHostDescription (monitor, config.AddinId, config.AddinFile, out existingDescription);
					else
						res = database.GetAddinDescription (monitor, config.AddinId, out existingDescription);
					
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
					if (fi != null && fi.AddinId != null && fi.AddinId != config.AddinId) {
						if (fi.IsRoot)
							database.UninstallRootAddin (monitor, fi.AddinId, file, scanResult);
						else
							database.UninstallAddin (monitor, fi.AddinId, scanResult);
						
						// If the add-in version has changed, regenerate everything again since old data can't be reused
						if (Addin.GetIdName (fi.AddinId) == Addin.GetIdName (config.AddinId))
							scanResult.RegenerateRelationData = true;
					}
					
					// If a description could be generated, save it now (if the scan was successful)
					if (scanSuccessful) {
						if (database.SaveDescription (monitor, config, replaceFileName)) {
							// The new dependencies also have to be updated
							Util.AddDependencies (config, scanResult);
							scanResult.AddAddinToUpdateRelations (config.AddinId);
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
				folderInfo.SetLastScanTime (file, scannedAddinId, scannedIsRoot, File.GetLastWriteTime (file), !scanSuccessful);
			}
		}
		
		public AddinDescription ScanSingleFile (IProgressStatus monitor, string file, AddinScanResult scanResult)
		{
			AddinDescription config = null;
			
			if (monitor.VerboseLog)
				monitor.Log ("Scanning file: " + file);
				
			try {
				string ext = Path.GetExtension (file);
				bool scanSuccessful;
				
				if (ext == ".dll" || ext == ".exe")
					scanSuccessful = ScanAssembly (monitor, file, scanResult, out config);
				else
					scanSuccessful = ScanConfigAssemblies (monitor, file, scanResult, out config);

				if (scanSuccessful && config != null) {
					
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
			}
			return config;
		}
		
		public void ScanAddinsFile (IProgressStatus monitor, string file, AddinScanResult scanResult)
		{
			XmlTextReader r = null;
			StringCollection directories = new StringCollection ();
			StringCollection directoriesWithSubdirs = new StringCollection ();
			try {
				r = new XmlTextReader (new StreamReader (file));
				r.MoveToContent ();
				if (r.IsEmptyElement)
					return;
				r.ReadStartElement ();
				r.MoveToContent ();
				while (r.NodeType != XmlNodeType.EndElement) {
					if (r.NodeType == XmlNodeType.Element && r.LocalName == "Directory") {
						string subs = r.GetAttribute ("include-subdirs");
						string path = r.ReadElementString ().Trim ();
						if (path.Length > 0) {
							if (subs == "true")
								directoriesWithSubdirs.Add (path);
							else
								directories.Add (path);
						}
					}
					else
						r.Skip ();
					r.MoveToContent ();
				}
			} catch (Exception ex) {
				monitor.ReportError ("Could not process addins file: " + file, ex);
				return;
			} finally {
				if (r != null)
					r.Close ();
			}
			foreach (string d in directories) {
				string dir = d;
				if (!Path.IsPathRooted (dir))
					dir = Path.Combine (Path.GetDirectoryName (file), dir);
				ScanFolder (monitor, dir, scanResult);
			}
			foreach (string d in directoriesWithSubdirs) {
				string dir = d;
				if (!Path.IsPathRooted (dir))
					dir = Path.Combine (Path.GetDirectoryName (file), dir);
				ScanFolderRec (monitor, dir, scanResult);
			}
		}
		
		public void ScanFolderRec (IProgressStatus monitor, string dir, AddinScanResult scanResult)
		{
			ScanFolder (monitor, dir, scanResult);
			
			if (!Directory.Exists (dir))
				return;
				
			foreach (string sd in Directory.GetDirectories (dir))
				ScanFolderRec (monitor, sd, scanResult);
		}
		
		bool ScanConfigAssemblies (IProgressStatus monitor, string filePath, AddinScanResult scanResult, out AddinDescription config)
		{
			config = null;
			
			try {
				string basePath = Path.GetDirectoryName (filePath);
				
				config = AddinDescription.Read (filePath);
				config.BasePath = basePath;
				config.AddinFile = filePath;
				
				return ScanDescription (monitor, config, null, scanResult);
			}
			catch (Exception ex) {
				// Something went wrong while scanning the assembly. We'll ignore it for now.
				monitor.ReportError ("There was an error while scanning add-in: " + filePath, ex);
				return false;
			}
		}
		
		bool ScanAssembly (IProgressStatus monitor, string filePath, AddinScanResult scanResult, out AddinDescription config)
		{
			config = null;
				
			try {
				Assembly asm = Util.LoadAssemblyForReflection (filePath);
				
				// Get the config file from the resources, if there is one
				
				string configFile = null;
				foreach (string res in asm.GetManifestResourceNames ()) {
					if (res.EndsWith (".addin") || res.EndsWith (".addin.xml")) {
						configFile = res;
						break;
					}
				}
				
				if (configFile != null) {
					using (Stream s = asm.GetManifestResourceStream (configFile)) {
						string asmFile = new Uri (asm.CodeBase).LocalPath;
						config = AddinDescription.Read (s, Path.GetDirectoryName (asmFile));
					}
				}
				else {
					// On this case, only scan the assembly if it has the Addin attribute.
					AddinAttribute att = (AddinAttribute) Attribute.GetCustomAttribute (asm, typeof(AddinAttribute), false);
					if (att == null) {
						config = null;
						return true;
					}
					config = new AddinDescription ();
				}
				
				config.BasePath = Path.GetDirectoryName (filePath);
				config.AddinFile = filePath;
				
				string rasmFile = Path.GetFileName (filePath);
				if (!config.MainModule.Assemblies.Contains (rasmFile))
					config.MainModule.Assemblies.Add (rasmFile);
				
				return ScanDescription (monitor, config, asm, scanResult);
			}
			catch (Exception ex) {
				// Something went wrong while scanning the assembly. We'll ignore it for now.
				monitor.ReportError ("There was an error while scanning assembly: " + filePath, ex);
				return false;
			}
		}

		bool ScanDescription (IProgressStatus monitor, AddinDescription config, Assembly rootAssembly, AddinScanResult scanResult)
		{
			// First of all scan the main module
			
			ArrayList assemblies = new ArrayList ();
			ArrayList hostExtensionClasses = new ArrayList ();
			
			try {
				foreach (string s in config.MainModule.Assemblies) {
					string asmFile = Path.Combine (config.BasePath, s);
					Assembly asm = Util.LoadAssemblyForReflection (asmFile);
					assemblies.Add (asm);
				}
				
				foreach (Assembly asm in assemblies)
					ScanAssemblyAddinHeaders (config, asm, scanResult);
				
				// The add-in id and version must be already assigned at this point
				
				// Clean host data from the index. New data will be added.
				if (scanResult.HostIndex != null)
					scanResult.HostIndex.RemoveHostData (config.AddinId, config.AddinFile);

				foreach (Assembly asm in assemblies)
					ScanAssemblyContents (config, asm, hostExtensionClasses, scanResult);
				
				if (config.IsRoot && scanResult.HostIndex != null) {
					// If the add-in is a root, register its assemblies
					foreach (Assembly asm in assemblies) {
						string asmFile = new Uri (asm.CodeBase).LocalPath;
						scanResult.HostIndex.RegisterAssembly (asmFile, config.AddinId, config.AddinFile);
					}
				}
				
			} catch (Exception ex) {
				if (monitor.VerboseLog)
					monitor.Log ("Could not load some add-in assemblies: " + ex.Message);
				scanResult.AddFileToWithFailure (config.AddinFile);
				return false;
			}
			
			foreach (Type t in hostExtensionClasses) {
				RegisterHostTypeNode (config, t, assemblies);
			}
			
			// Extension node types may have child nodes declared as attributes. Find them.
			
			Hashtable internalNodeSets = new Hashtable ();
			foreach (ExtensionPoint ep in config.ExtensionPoints) {
				ScanNodeSet (config, ep.NodeSet, assemblies, internalNodeSets);
			}
		
			// Now scan all modules
			
			if (!config.IsRoot) {
				foreach (ModuleDescription mod in config.OptionalModules) {
					try {
						assemblies.Clear ();
						foreach (string s in mod.Assemblies) {
							string asmFile = Path.Combine (config.BasePath, s);
							Assembly asm = Util.LoadAssemblyForReflection (asmFile);
							assemblies.Add (asm);
						}
						foreach (Assembly asm in assemblies)
							ScanAssemblyContents (config, asm, null, scanResult);
				
						if (config.IsRoot && scanResult.HostIndex != null) {
							// If the add-in is a root, register its assemblies
							foreach (Assembly asm in assemblies) {
								string asmFile = new Uri (asm.CodeBase).LocalPath;
								scanResult.HostIndex.RegisterAssembly (asmFile, config.AddinId, config.AddinFile);
							}
						}
						
					} catch (Exception ex) {
						if (monitor.VerboseLog)
							monitor.Log ("Could not load some add-in assemblies: " + ex.Message);
						scanResult.AddFileToWithFailure (config.AddinFile);
					}
				}
			}
			
			return true;
		}
		
		void ScanAssemblyAddinHeaders (AddinDescription config, Assembly asm, AddinScanResult scanResult)
		{
			// Get basic add-in information
			AddinAttribute att = (AddinAttribute) Attribute.GetCustomAttribute (asm, typeof(AddinAttribute), false);
			if (att != null) {
				if (att.Id.Length > 0)
					config.LocalId = att.Id;
				if (att.Version.Length > 0)
					config.Version = att.Version;
				if (att.Namespace.Length > 0)
					config.Namespace = att.Namespace;
				if (att.Category.Length > 0)
					config.Category = att.Category;
				config.IsRoot = att is AddinRootAttribute;
			}
		}
		
		void ScanAssemblyContents (AddinDescription config, Assembly asm, ArrayList hostExtensionClasses, AddinScanResult scanResult)
		{
			// Get dependencies
			
			object[] deps = asm.GetCustomAttributes (typeof(AddinDependencyAttribute), false);
			foreach (AddinDependencyAttribute dep in deps) {
				AddinDependency adep = new AddinDependency ();
				adep.AddinId = dep.Id;
				adep.Version = dep.Version;
				config.MainModule.Dependencies.Add (adep);
			}
			
			// Get extension points
			
			object[] extPoints = asm.GetCustomAttributes (typeof(ExtensionPointAttribute), false);
			foreach (ExtensionPointAttribute ext in extPoints) {
				ExtensionPoint ep = config.AddExtensionPoint (ext.Path);
				ep.Description = ext.Description;
				ep.Name = ext.Name;
				ep.AddExtensionNode (ext.NodeName, ext.NodeType.FullName);
			}
			
			foreach (Type t in asm.GetTypes ()) {
				
				if (Attribute.IsDefined (t, typeof(ExtensionAttribute))) {
					foreach (ExtensionAttribute eatt in t.GetCustomAttributes (typeof(ExtensionAttribute), false)) {
						string path;
						string nodeName;
						
						if (eatt.Path.Length == 0) {
							if (config.IsRoot) {
								// The extension point must be one of the defined by the assembly
								// Look for it later, when the assembly has been fully scanned.
								hostExtensionClasses.Add (t);
								continue;
							}
							else {
								path = GetBaseTypeNameList (t);
								if (path == "$") {
									// The type does not implement any interface and has no superclass.
									// Will be reported later as an error.
									path = "$" + t.FullName;
								}
								nodeName = "Type";
							}
						} else {
							path = eatt.Path;
							nodeName = eatt.NodeName;
						}
							
						ExtensionNodeDescription elem = config.MainModule.AddExtensionNode (path, nodeName);
						if (eatt.Id.Length > 0) {
							elem.SetAttribute ("id", eatt.Id);
							elem.SetAttribute ("type", t.FullName);
						} else {
							elem.SetAttribute ("id", t.FullName);
						}
						if (eatt.InsertAfter.Length > 0)
							elem.SetAttribute ("insertafter", eatt.InsertAfter);
						if (eatt.InsertBefore.Length > 0)
							elem.SetAttribute ("insertbefore", eatt.InsertAfter);
					}
				}
				else if (Attribute.IsDefined (t, typeof(TypeExtensionPointAttribute))) {
					foreach (TypeExtensionPointAttribute epa in t.GetCustomAttributes (typeof(TypeExtensionPointAttribute), false)) {
						ExtensionPoint ep;
						
						ExtensionNodeType nt = new ExtensionNodeType ();
						
						if (epa.Path.Length > 0) {
							ep = config.AddExtensionPoint (epa.Path);
						}
						else {
							ep = config.AddExtensionPoint (GetDefaultTypeExtensionPath (config, t));
							nt.ObjectTypeName = t.FullName;
						}
						nt.Id = epa.NodeName;
						nt.TypeName = epa.NodeType.FullName;
						ep.NodeSet.NodeTypes.Add (nt);
						ep.Description = epa.Description;
						ep.Name = epa.Name;
						ep.RootAddin = config.AddinId;
						ep.SetExtensionsAddinId (config.AddinId);
					}
				}
			}
		}
		
		void ScanNodeSet (AddinDescription config, ExtensionNodeSet nset, ArrayList assemblies, Hashtable internalNodeSets)
		{
			foreach (ExtensionNodeType nt in nset.NodeTypes) {
				if (nt.TypeName.Length == 0)
					nt.TypeName = "Mono.Addins.TypeExtensionNode";
				
				Type ntype = FindAddinType (nt.TypeName, assemblies);
				if (ntype == null)
					continue;

				// Add type information declared with attributes in the code
				ExtensionNodeAttribute nodeAtt = (ExtensionNodeAttribute) Attribute.GetCustomAttribute (ntype, typeof(ExtensionNodeAttribute), false);
				if (nodeAtt != null) {
					if (nt.Id.Length == 0 && nodeAtt.NodeName.Length > 0)
						nt.Id = nodeAtt.NodeName;
					if (nt.Description.Length == 0 && nodeAtt.Description.Length > 0)
						nt.Description = nodeAtt.Description;
				} else {
					// Use the node type name as default name
					if (nt.Id.Length == 0)
						nt.Id = ntype.Name;
				}
				
				// Add information about attributes
				object[] fieldAtts = ntype.GetCustomAttributes (typeof(NodeAttributeAttribute), true);
				foreach (NodeAttributeAttribute fatt in fieldAtts) {
					NodeTypeAttribute natt = new NodeTypeAttribute ();
					natt.Name = fatt.Name;
					natt.Required = fatt.Required;
					if (fatt.Type != null)
						natt.Type = fatt.Type.FullName;
					if (fatt.Description.Length > 0)
						natt.Description = fatt.Description;
					nt.Attributes.Add (natt);
				}
				
				// Check if the type has NodeAttribute attributes applied to fields.
				foreach (FieldInfo field in ntype.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
					NodeAttributeAttribute fatt = (NodeAttributeAttribute) Attribute.GetCustomAttribute (field, typeof(NodeAttributeAttribute));
					if (fatt != null) {
						NodeTypeAttribute natt = new NodeTypeAttribute ();
						if (fatt.Name.Length > 0)
							natt.Name = fatt.Name;
						else
							natt.Name = field.Name;
						if (fatt.Description.Length > 0)
							natt.Description = fatt.Description;
						natt.Type = field.FieldType.FullName;
						natt.Required = fatt.Required;
						nt.Attributes.Add (natt);
					}
				}
				
				// Check if the extension type allows children by looking for [ExtensionNodeChild] attributes.
				// First of all, look in the internalNodeSets hashtable, which is being used as cache
				
				string childSet = (string) internalNodeSets [nt.TypeName];
				if (childSet != null) {
					if (childSet.Length == 0) {
						// The extension type does not declare children.
						continue;
					}
					// The extension type can have children. The allowed children are
					// defined in this extension set.
					nt.NodeSets.Add (childSet);
					continue;
				}
				
				object[] ats = ntype.GetCustomAttributes (typeof(ExtensionNodeChildAttribute), true);
				if (ats.Length > 0) {
					// Create a new node set for this type. It is necessary to create a new node set
					// instead of just adding child ExtensionNodeType objects to the this node type
					// because child types references can be recursive.
					ExtensionNodeSet internalSet = new ExtensionNodeSet ();
					internalSet.Id = ntype.Name + "_" + Guid.NewGuid().ToString ();
					foreach (ExtensionNodeChildAttribute at in ats) {
						ExtensionNodeType internalType = new ExtensionNodeType ();
						internalType.Id = at.NodeName;
						internalType.TypeName = at.ExtensionNodeType.FullName;
						internalSet.NodeTypes.Add (internalType);
					}
					config.ExtensionNodeSets.Add (internalSet);
					nt.NodeSets.Add (internalSet.Id);
					
					// Register the new set in a hashtable, to allow recursive references to the
					// same internal set.
					internalNodeSets [nt.TypeName] = internalSet.Id;
					internalNodeSets [ntype.AssemblyQualifiedName] = internalSet.Id;
					ScanNodeSet (config, internalSet, assemblies, internalNodeSets);
				}
			}
		}
		
		string GetBaseTypeNameList (Type type)
		{
			StringBuilder sb = new StringBuilder ("$");
			Type btype = type.BaseType;
			while (btype != typeof(object)) {
				sb.Append (btype.FullName).Append (',');
				btype = btype.BaseType;
			}
			foreach (Type iterf in type.GetInterfaces ()) {
				sb.Append (iterf.FullName).Append (',');
			}
			if (sb.Length > 0)
				sb.Remove (sb.Length - 1, 1);
			return sb.ToString ();
		}
		
		void RegisterHostTypeNode (AddinDescription config, Type t, ArrayList assemblies)
		{
			foreach (ExtensionAttribute eatt in t.GetCustomAttributes (typeof(ExtensionAttribute), false)) {
				if (eatt.Path.Length > 0)
					continue;
				
				foreach (ExtensionPoint ep in config.ExtensionPoints) {
					foreach (ExtensionNodeType nt in ep.NodeSet.NodeTypes) {
						if (nt.ObjectTypeName.Length == 0)
							continue;
						Type etype = FindAddinType (nt.ObjectTypeName, assemblies);
						if (etype != null && etype.IsAssignableFrom (t)) {
							RegisterTypeNode (config, eatt, ep.Path, nt.Id, t);
							return;
						}
					}
				}
			}
		}
		
		Type FindAddinType (string typeName, ArrayList assemblies)
		{
			Type etype = Type.GetType (typeName, false);
			if (etype != null)
				return etype;
			foreach (Assembly asm in assemblies) {
				etype = asm.GetType (typeName);
				if (etype != null)
					return etype;
			}
			return null;
		}

		void RegisterTypeNode (AddinDescription config, ExtensionAttribute eatt, string path, string nodeName, Type t)
		{
			ExtensionNodeDescription elem = config.MainModule.AddExtensionNode (path, nodeName);
			if (eatt.Id.Length > 0) {
				elem.SetAttribute ("id", eatt.Id);
				elem.SetAttribute ("type", t.FullName);
			} else {
				elem.SetAttribute ("id", t.FullName);
			}
			if (eatt.InsertAfter.Length > 0)
				elem.SetAttribute ("insertafter", eatt.InsertAfter);
			if (eatt.InsertBefore.Length > 0)
				elem.SetAttribute ("insertbefore", eatt.InsertAfter);
		}
		
		internal string GetDefaultTypeExtensionPath (AddinDescription desc, Type type)
		{
			return "/" + Addin.GetIdName (desc.AddinId) + "/TypeExtensions/" + type.FullName;
		}
	}
}
