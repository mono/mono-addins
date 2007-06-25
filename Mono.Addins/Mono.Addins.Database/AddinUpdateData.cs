//
// AddinUpdateData.cs
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
using Mono.Addins.Description;

namespace Mono.Addins.Database
{
	class AddinUpdateData
	{
		// This table collects information about extensions. For each path (key)
		// has a ExtensionInfo object with information about the addin that
		// defines the extension point and the addins which extend it
		Hashtable pathHash = new Hashtable ();
		
		// Collects globally defined node sets. Key is node set name. Value is
		// a ExtensionInfo
		Hashtable nodeSetHash = new Hashtable ();
		
		Hashtable objectTypeExtensions = new Hashtable ();
		
		internal int RelExtensionPoints;
		internal int RelExtensions;
		internal int RelNodeSetTypes;
		internal int RelExtensionNodes;
		
		class RootExtensionPoint
		{
			public AddinDescription Description;
			public ExtensionPoint ExtensionPoint;
		}
		
		AddinDatabase database;
		
		public AddinUpdateData (AddinDatabase database)
		{
			this.database = database;
		}
		
		public void RegisterAddinRootExtensionPoint (AddinDescription description, ExtensionPoint ep)
		{
			RelExtensionPoints++;
			ArrayList list = (ArrayList) pathHash [ep.Path];
			if (list == null) {
				list = new ArrayList ();
				pathHash [ep.Path] = list;
			}
			
			RootExtensionPoint rep = new RootExtensionPoint ();
			rep.Description = description;
			rep.ExtensionPoint = ep;
			ep.RootAddin = description.AddinId;
			list.Add (rep);
		}

		public void RegisterAddinRootNodeSet (AddinDescription description, ExtensionNodeSet nodeSet)
		{
			ArrayList list = (ArrayList) nodeSetHash [nodeSet.Id];
			if (list == null) {
				list = new ArrayList ();
				nodeSetHash [nodeSet.Id] = list;
			}
			
			RootExtensionPoint rep = new RootExtensionPoint ();
			rep.Description = description;
			ExtensionPoint ep = new ExtensionPoint ();
			ep.RootAddin = description.AddinId;
			ep.SetNodeSet (nodeSet);
			rep.ExtensionPoint = ep;
			list.Add (rep);
		}

		public void RegisterNodeSet (AddinDescription description, ExtensionNodeSet nset)
		{
			string id = Addin.GetFullId (description.Namespace, nset.Id, description.Version);
			foreach (ExtensionPoint einfo in GetExtensionInfo (nodeSetHash, id, description, description.MainModule, false)) {
				if (einfo.RootAddin == null || database.AddinDependsOn (einfo.RootAddin, description.AddinId))
					einfo.RootAddin = description.AddinId;
				einfo.NodeSet.MergeWith (null, nset);
			}
		}
		
		public void RegisterExtensionPoint (AddinDescription description, ExtensionPoint ep)
		{
			foreach (ExtensionPoint einfo in GetExtensionInfo (pathHash, ep.Path, description, description.MainModule, false)) {
				if (einfo.RootAddin == null || database.AddinDependsOn (einfo.RootAddin, description.AddinId))
					einfo.RootAddin = description.AddinId;
				einfo.MergeWith (null, ep);
			}
		}

		public void RegisterExtension (AddinDescription description, ModuleDescription module, Extension extension)
		{
			if (extension.Path.StartsWith ("$")) {
				UnresolvedObjectTypeExtension extData = new UnresolvedObjectTypeExtension ();
				extData.Description = description;
				extData.ModuleDescription = module;
				extData.Extension = extension;
				string[] objectTypes = extension.Path.Substring (1).Split (',');
				foreach (string s in objectTypes) {
					ArrayList list = (ArrayList) objectTypeExtensions [s];
					if (list == null) {
						list = new ArrayList ();
						objectTypeExtensions [s] = list;
					}
					list.Add (extData);
				}
			}
		}
		
		void CollectObjectTypeExtensions (AddinDescription desc, ExtensionPoint ep, string objectTypeName)
		{
			ArrayList list = (ArrayList) objectTypeExtensions [objectTypeName];
			if (list == null)
				return;
			
			foreach (UnresolvedObjectTypeExtension data in list) {
				if (IsAddinCompatible (desc, data.Description, data.ModuleDescription)) {
					data.Extension.Path = ep.Path;
					RegisterExtension (data.Description, data.ModuleDescription, ep.Path);
					data.FoundExtensionPoint = true;
				}
			}
		}
		
		public void RegisterExtension (AddinDescription description, ModuleDescription module, string path)
		{
			foreach (ExtensionPoint einfo in GetExtensionInfo (pathHash, path, description, module, true)) {
				if (!einfo.Addins.Contains (description.AddinId))
					einfo.Addins.Add (description.AddinId);
			}
		}
		
		public IEnumerable GetUnresolvedExtensionPoints ()
		{
			ArrayList list = new ArrayList ();
			foreach (object ob in pathHash.Values)
				if (ob is ExtensionPoint)
					list.Add (ob);
			return list;
		}

		public IEnumerable GetUnresolvedExtensionSets ()
		{
			ArrayList list = new ArrayList ();
			foreach (object ob in nodeSetHash.Values)
				if (ob is ExtensionPoint)
					list.Add (ob);
			return list;
		}
		
		public void ResolveExtensions (IProgressStatus monitor, Hashtable descriptions)
		{
			// Make a copy of the extensions found, sice the hash may change while being scanned
			object[] extensionPointsFound = new object [pathHash.Count];
			pathHash.Values.CopyTo (extensionPointsFound, 0);

			foreach (object ob in extensionPointsFound) {
				ExtensionPoint ep = ob as ExtensionPoint;
				
				if (ep == null) {
					// It is a list of extension from a root add-in
					ArrayList rootExtensionPoints = (ArrayList) ob;
					foreach (RootExtensionPoint rep in rootExtensionPoints) {
						foreach (ExtensionNodeType nt in rep.ExtensionPoint.NodeSet.NodeTypes) {
							if (nt.ObjectTypeName.Length > 0)
								CollectObjectTypeExtensions (rep.Description, rep.ExtensionPoint, nt.ObjectTypeName);
						}
					}
					continue;
				}
				
				if (ep.RootAddin == null) {
					// Ignore class extensions
					if (!ep.Path.StartsWith ("$")) {
						// No add-in has defined this extension point, but some add-in
						// is trying to extend it. A parent extension may exist. Check it now.
						ExtensionPoint pep = GetParentExtensionPoint (ep.Path);
						if (pep != null) {
							foreach (string a in ep.Addins)
								if (!pep.Addins.Contains (a))
									pep.Addins.Add (a);
						} else {
							foreach (string s in ep.Addins)
								monitor.ReportWarning ("The add-in '" + s + "' is trying to extend '" + ep.Path + "', but there isn't any add-in defining this extension point");
						}
					}
					pathHash.Remove (ep.Path);
				}
				else {
					foreach (ExtensionNodeType nt in ep.NodeSet.NodeTypes) {
						if (nt.ObjectTypeName.Length > 0) {
							AddinDescription desc = (AddinDescription) descriptions [ep.RootAddin];
							CollectObjectTypeExtensions (desc, ep, nt.ObjectTypeName);
						}
					}
				}
			}
			
			foreach (ArrayList list in objectTypeExtensions.Values) {
				foreach (UnresolvedObjectTypeExtension data in list) {
					if (!data.FoundExtensionPoint) {
						monitor.ReportWarning ("The add-in '" + data.Description.AddinId + "' is trying to register the class '" + data.Extension.Path + "', but there isn't any add-in defining a suitable extension point");
						// The type extensions may be registered using different base classes.
						// Make sure the warning is shown only once
						data.FoundExtensionPoint = true;
					}
				}
			}
		}
		
		IEnumerable GetExtensionInfo (Hashtable hash, string path, AddinDescription description, ModuleDescription module, bool lookInParents)
		{
			ArrayList list = new ArrayList ();
			
			object data = hash [path];
			if (data == null && lookInParents) {
				// Root add-in extension points are registered before any other kind of extension,
				// so we should find it now.
				data = GetParentExtensionInfo (path);
			}
			
			if (data is ArrayList) {
				// Extension point which belongs to a root assembly.
				list.AddRange (GetRootExtensionInfo (hash, path, description, module, (ArrayList) data));
			}
			else {
				ExtensionPoint info = (ExtensionPoint) data;
				if (info == null) {
					info = new ExtensionPoint ();
					info.Path = path;
					hash [path] = info;
				}
				list.Add (info);
			}
			return list;
		}
	
		ArrayList GetRootExtensionInfo (Hashtable hash, string path, AddinDescription description, ModuleDescription module, ArrayList rootExtensionPoints)
		{
			ArrayList list = new ArrayList ();
			foreach (RootExtensionPoint rep in rootExtensionPoints) {
				
				// Find an extension point defined in a root add-in which is compatible with the version of the extender dependency
				if (IsAddinCompatible (rep.Description, description, module))
					list.Add (rep.ExtensionPoint);
			}
			return list;
		}
		
		ExtensionPoint GetParentExtensionPoint (string path)
		{
			return GetParentExtensionInfo (path) as ExtensionPoint;
		}
		
		object GetParentExtensionInfo (string path)
		{
			int i = path.LastIndexOf ('/');
			if (i == -1)
				return null;
			string np = path.Substring (0, i);
			object ep = pathHash [np];
			if (ep != null)
				return ep;
			else
				return GetParentExtensionInfo (np);
		}
		
		bool IsAddinCompatible (AddinDescription installedDescription, AddinDescription description, ModuleDescription module)
		{
			string addinId = Addin.GetFullId (installedDescription.Namespace, installedDescription.LocalId, null);
			string requiredVersion = null;
			
			for (int n = module.Dependencies.Count - 1; n >= 0; n--) {
				AddinDependency adep = module.Dependencies [n] as AddinDependency;
				if (adep != null && Addin.GetFullId (description.Namespace, adep.AddinId, null) == addinId) {
					requiredVersion = adep.Version;
					break;
				}
			}
			if (requiredVersion == null)
				return false;

			// Check if the required version is between rep.Description.CompatVersion and rep.Description.Version
			if (Addin.CompareVersions (installedDescription.Version, requiredVersion) > 0)
				return false;
			if (installedDescription.CompatVersion.Length > 0 && Addin.CompareVersions (installedDescription.CompatVersion, requiredVersion) < 0)
				return false;
			
			return true;
		}
	}
	
	class UnresolvedObjectTypeExtension
	{
		public AddinDescription Description;
		public ModuleDescription ModuleDescription;
		public Extension Extension;
		public bool FoundExtensionPoint;
	}
}
