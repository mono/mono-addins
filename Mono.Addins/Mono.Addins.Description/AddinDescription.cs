//
// AddinDescription.cs
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
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Specialized;
using Mono.Addins.Serialization;
using Mono.Addins.Database;

namespace Mono.Addins.Description
{
	// This class represent an add-in configuration file. It has properties for getting
	// all information, and methods for loading and saving files.
	public class AddinDescription: IBinaryXmlElement
	{
		XmlDocument configDoc;
		string configFile;
		bool fromBinaryFile;
		
		string id;
		string name;
		string ns;
		string version;
		string compatVersion;
		string author;
		string url;
		string copyright;
		string description;
		string category;
		string basePath;
		string sourceAddinFile;
		bool isroot;
		bool hasUserId;
		
		ModuleDescription mainModule;
		ModuleCollection optionalModules;
		ExtensionNodeSetCollection nodeSets;
		ConditionTypeDescriptionCollection conditionTypes;
		ExtensionPointCollection extensionPoints;
		
		internal static BinaryXmlTypeMap typeMap;
		
		static AddinDescription ()
		{
			typeMap = new BinaryXmlTypeMap ();
			typeMap.RegisterType (typeof(AddinDescription), "AddinDescription");
			typeMap.RegisterType (typeof(Extension), "Extension");
			typeMap.RegisterType (typeof(ExtensionNodeDescription), "Node");
			typeMap.RegisterType (typeof(ExtensionNodeSet), "NodeSet");
			typeMap.RegisterType (typeof(ExtensionNodeType), "NodeType");
			typeMap.RegisterType (typeof(ExtensionPoint), "ExtensionPoint");
			typeMap.RegisterType (typeof(ModuleDescription), "ModuleDescription");
			typeMap.RegisterType (typeof(ConditionTypeDescription), "ConditionType");
			typeMap.RegisterType (typeof(Condition), "Condition");
			typeMap.RegisterType (typeof(AddinDependency), "AddinDependency");
			typeMap.RegisterType (typeof(AssemblyDependency), "AssemblyDependency");
			typeMap.RegisterType (typeof(NodeTypeAttribute), "NodeTypeAttribute");
		}
		
		public string AddinFile {
			get { return sourceAddinFile; }
			set { sourceAddinFile = value; }
		}
		
		public string AddinId {
			get { return Addin.GetFullId (Namespace, LocalId, Version); }
		}
		
		public string LocalId {
			get { return id != null ? id : string.Empty; }
			set { id = value; }
		}

		public string Namespace {
			get { return ns != null ? ns : string.Empty; }
			set { ns = value; }
		}

		public string Name {
			get {
				if (name != null && name.Length > 0)
					return name;
				if (HasUserId)
					return AddinId;
				else if (sourceAddinFile != null)
					return Path.GetFileNameWithoutExtension (sourceAddinFile);
				else
					return string.Empty;
			}
			set { name = value; }
		}

		public string Version {
			get { return version != null ? version : string.Empty; }
			set { version = value; }
		}

		public string CompatVersion {
			get { return compatVersion != null ? compatVersion : string.Empty; }
			set { compatVersion = value; }
		}

		public string Author {
			get { return author != null ? author : string.Empty; }
			set { author = value; }
		}

		public string Url {
			get { return url != null ? url : string.Empty; }
			set { url = value; }
		}

		public string Copyright {
			get { return copyright != null ? copyright : string.Empty; }
			set { copyright = value; }
		}

		public string Description {
			get { return description != null ? description : string.Empty; }
			set { description = value; }
		}

		public string Category {
			get { return category != null ? category : string.Empty; }
			set { category = value; }
		}
		
		internal string BasePath {
			get { return basePath != null ? basePath : string.Empty; }
			set { basePath = value; }
		}
		
		internal bool IsRoot {
			get { return isroot; }
			set { isroot = value; }
		}
		
		internal bool HasUserId {
			get { return hasUserId; }
			set { hasUserId = value; }
		}
		
		public StringCollection AllFiles {
			get {
				StringCollection col = new StringCollection ();
				foreach (string s in MainModule.AllFiles)
					col.Add (s);

				foreach (ModuleDescription mod in OptionalModules) {
					foreach (string s in mod.AllFiles)
						col.Add (s);
				}
				return col;
			}
		}
		
		public ModuleDescription MainModule {
			get {
				if (mainModule == null) {
					if (RootElement == null)
						mainModule = new ModuleDescription ();
					else
						mainModule = new ModuleDescription (RootElement);
				}
				return mainModule;
			}
		}
		
		public ModuleCollection OptionalModules {
			get {
				if (optionalModules == null) {
					optionalModules = new ModuleCollection ();
					if (RootElement != null) {
						foreach (XmlElement mod in RootElement.SelectNodes ("Module"))
							optionalModules.Add (new ModuleDescription (mod));
					}
				}
				return optionalModules;
			}
		}
		
		public ModuleCollection AllModules {
			get {
				ModuleCollection col = new ModuleCollection ();
				col.Add (MainModule);
				foreach (ModuleDescription mod in OptionalModules)
					col.Add (mod);
				return col;
			}
		}
		
		public ExtensionNodeSetCollection ExtensionNodeSets {
			get {
				if (nodeSets == null) {
					nodeSets = new ExtensionNodeSetCollection ();
					if (RootElement != null) {
						foreach (XmlElement elem in RootElement.SelectNodes ("ExtensionNodeSet"))
							nodeSets.Add (new ExtensionNodeSet (elem));
					}
				}
				return nodeSets;
			}
		}
		
		public ExtensionPointCollection ExtensionPoints {
			get {
				if (extensionPoints == null) {
					extensionPoints = new ExtensionPointCollection ();
					if (RootElement != null) {
						foreach (XmlElement elem in RootElement.SelectNodes ("ExtensionPoint"))
							extensionPoints.Add (new ExtensionPoint (elem));
					}
				}
				return extensionPoints;
			}
		}
		
		public ConditionTypeDescriptionCollection ConditionTypes {
			get {
				if (conditionTypes == null) {
					conditionTypes = new ConditionTypeDescriptionCollection ();
					if (RootElement != null) {
						foreach (XmlElement elem in RootElement.SelectNodes ("ConditionType"))
							conditionTypes.Add (new ConditionTypeDescription (elem));
					}
				}
				return conditionTypes;
			}
		}
		
		public ExtensionPoint AddExtensionPoint (string path)
		{
			ExtensionPoint ep = new ExtensionPoint ();
			ep.Path = path;
			ExtensionPoints.Add (ep);
			return ep;
		}
		
		XmlElement RootElement {
			get {
				if (configDoc != null)
					return configDoc.DocumentElement;
				else
					return null;
			}
		}
		
		public string FileName {
			get { return configFile; }
			set { configFile = value; }
		}
		
		public void Save (string fileName)
		{
			configFile = fileName;
			Save ();
		}
		
		public void Save ()
		{
			if (configFile == null)
				throw new InvalidOperationException ("File name not specified.");
			
			SaveXml ();

			configDoc.Save (configFile);
		}
		
		public XmlDocument SaveToXml ()
		{
			SaveXml ();
			return configDoc;
		}
		
		void SaveXml ()
		{
			XmlElement elem;
			
			if (configDoc == null) {
				configDoc = new XmlDocument ();
				configDoc.AppendChild (configDoc.CreateElement ("Addin"));
			}
			
			elem = configDoc.DocumentElement;
			
			if (HasUserId)
				elem.SetAttribute ("id", id);
			else
				elem.RemoveAttribute ("id");
			
			elem.SetAttribute ("version", version);
			elem.SetAttribute ("namespace", ns);
			
			if (isroot)
				elem.SetAttribute ("isroot", "true");
			else
				elem.RemoveAttribute ("isroot");
			
			// Name will return the file name when HasUserId=false
			if (Name.Length > 0)
				elem.SetAttribute ("name", Name);
			else
				elem.RemoveAttribute ("name");
				
			if (compatVersion != null && compatVersion.Length > 0)
				elem.SetAttribute ("compatVersion", compatVersion);
			else
				elem.RemoveAttribute ("compatVersion");
				
			if (author != null && author.Length > 0)
				elem.SetAttribute ("author", author);
			else
				elem.RemoveAttribute ("author");
				
			if (url != null && url.Length > 0)
				elem.SetAttribute ("url", url);
			else
				elem.RemoveAttribute ("url");
				
			if (copyright != null && copyright.Length > 0)
				elem.SetAttribute ("copyright", copyright);
			else
				elem.RemoveAttribute ("copyright");
				
			if (description != null && description.Length > 0)
				elem.SetAttribute ("description", description);
			else
				elem.RemoveAttribute ("description");
				
			if (category != null && category.Length > 0)
				elem.SetAttribute ("category", category);
			else
				elem.RemoveAttribute ("category");
				
			if (mainModule != null) {
				mainModule.Element = elem;
				mainModule.SaveXml (elem);
			}
				
			if (optionalModules != null)
				optionalModules.SaveXml (elem);
				
			if (nodeSets != null)
				nodeSets.SaveXml (elem);
				
			if (extensionPoints != null)
				extensionPoints.SaveXml (elem);
		}
		

		public static AddinDescription Read (string configFile)
		{
			AddinDescription config;
			using (Stream s = File.OpenRead (configFile)) {
				config = Read (s, Path.GetDirectoryName (configFile));
			}
			config.configFile = configFile;
			return config;
		}
		
		public static AddinDescription Read (Stream stream, string basePath)
		{
			AddinDescription config = new AddinDescription ();
			
			try {
				config.configDoc = new XmlDocument ();
				config.configDoc.PreserveWhitespace = true;
				config.configDoc.Load (stream);
				config.configDoc.PreserveWhitespace = false;
			} catch (Exception ex) {
				throw new InvalidOperationException ("The add-in configuration file is invalid.", ex);
			}
			
			XmlElement elem = config.configDoc.DocumentElement;
			config.id = elem.GetAttribute ("id");
			config.ns = elem.GetAttribute ("namespace");
			config.name = elem.GetAttribute ("name");
			config.version = elem.GetAttribute ("version");
			config.compatVersion = elem.GetAttribute ("compatVersion");
			config.author = elem.GetAttribute ("author");
			config.url = elem.GetAttribute ("url");
			config.copyright = elem.GetAttribute ("copyright");
			config.description = elem.GetAttribute ("description");
			config.category = elem.GetAttribute ("category");
			config.basePath = elem.GetAttribute ("basePath");
			config.isroot = elem.GetAttribute ("isroot") == "true" || elem.GetAttribute ("isroot") == "yes";
			
			return config;
		}
		
		internal static AddinDescription ReadBinary (FileDatabase fdb, string configFile)
		{
			AddinDescription description = (AddinDescription) fdb.ReadSharedObject (configFile, typeMap);
			if (description != null) {
				description.FileName = configFile;
				description.fromBinaryFile = true;
			}
			return description;
		}
		
		internal static AddinDescription ReadHostBinary (FileDatabase fdb, string basePath, string addinId, string addinFile)
		{
			string fileName;
			AddinDescription description = (AddinDescription) fdb.ReadSharedObject (basePath, addinId, ".mroot", Util.GetFullPath (addinFile), typeMap, out fileName);
			if (description != null) {
				description.FileName = fileName;
				description.fromBinaryFile = true;
			}
			return description;
		}
		
		internal void SaveBinary (FileDatabase fdb, string file)
		{
			configFile = file;
			SaveBinary (fdb);
		}
		
		internal void SaveBinary (FileDatabase fdb)
		{
			fdb.WriteSharedObject (AddinFile, FileName, typeMap, this);
//			BinaryXmlReader.DumpFile (configFile);
		}
		
		internal void SaveHostBinary (FileDatabase fdb, string basePath)
		{
			if (!fromBinaryFile)
				FileName = null;
			FileName = fdb.WriteSharedObject (basePath, AddinId, ".mroot", AddinFile, FileName, typeMap, this);
		}
		
		public StringCollection Verify ()
		{
			StringCollection errors = new StringCollection ();
			
			if (IsRoot) {
				if (OptionalModules.Count > 0)
					errors.Add ("Root add-in hosts can't have optional modules.");
				if (MainModule.Dependencies.Count > 0)
					errors.Add ("Root add-in hosts can't have dependencies.");
			}
			
			if (AddinId.Length == 0 || Version.Length == 0) {
				if (ExtensionPoints.Count > 0)
					errors.Add ("Add-ins which define new extension points must have an Id and Version.");
			}

			MainModule.Verify ("", errors);
			OptionalModules.Verify ("", errors);
			ExtensionNodeSets.Verify ("", errors);
			ExtensionPoints.Verify ("", errors);
			ConditionTypes.Verify ("", errors);
			
			foreach (ExtensionNodeSet nset in ExtensionNodeSets) {
				if (nset.Id.Length == 0)
					errors.Add ("Attribute 'id' can't be empty for global node sets.");
			}
			
			foreach (string file in AllFiles) {
				string asmFile = Path.Combine (BasePath, file);
				if (!File.Exists (asmFile))
					errors.Add ("The file '" + file + "' referenced in the manifest could not be found.");
			}
			
			return errors;
		}
		
		internal void SetExtensionsAddinId (string addinId)
		{
			foreach (ExtensionPoint ep in ExtensionPoints)
				ep.SetExtensionsAddinId (addinId);
				
			foreach (ExtensionNodeSet ns in ExtensionNodeSets)
				ns.SetExtensionsAddinId (addinId);
		}
		
		internal void UnmergeExternalData (Hashtable addins)
		{
			// Removes extension types and extension sets coming from other add-ins.
			foreach (ExtensionPoint ep in ExtensionPoints)
				ep.UnmergeExternalData (AddinId, addins);
				
			foreach (ExtensionNodeSet ns in ExtensionNodeSets)
				ns.UnmergeExternalData (AddinId, addins);
		}
		
		internal void MergeExternalData (AddinDescription other)
		{
			// Removes extension types and extension sets coming from other add-ins.
			foreach (ExtensionPoint ep in other.ExtensionPoints) {
				ExtensionPoint tep = ExtensionPoints [ep.Path];
				if (tep != null)
					tep.MergeWith (AddinId, ep);
			}
				
			foreach (ExtensionNodeSet ns in other.ExtensionNodeSets) {
				ExtensionNodeSet tns = ExtensionNodeSets [ns.Id];
				if (tns != null)
					tns.MergeWith (AddinId, ns);
			}
		}
		
		void IBinaryXmlElement.Write (BinaryXmlWriter writer)
		{
			writer.WriteValue ("id", id);
			writer.WriteValue ("ns", ns);
			writer.WriteValue ("isroot", isroot);
			writer.WriteValue ("name", name);
			writer.WriteValue ("version", version);
			writer.WriteValue ("compatVersion", compatVersion);
			writer.WriteValue ("hasUserId", hasUserId);
			writer.WriteValue ("author", author);
			writer.WriteValue ("url", url);
			writer.WriteValue ("copyright", copyright);
			writer.WriteValue ("description", description);
			writer.WriteValue ("category", category);
			writer.WriteValue ("basePath", basePath);
			writer.WriteValue ("sourceAddinFile", sourceAddinFile);
			writer.WriteValue ("MainModule", MainModule);
			writer.WriteValue ("OptionalModules", OptionalModules);
			writer.WriteValue ("NodeSets", ExtensionNodeSets);
			writer.WriteValue ("ExtensionPoints", ExtensionPoints);
			writer.WriteValue ("ConditionTypes", ConditionTypes);
		}
		
		void IBinaryXmlElement.Read (BinaryXmlReader reader)
		{
			id = reader.ReadStringValue ("id");
			ns = reader.ReadStringValue ("ns");
			isroot = reader.ReadBooleanValue ("isroot");
			name = reader.ReadStringValue ("name");
			version = reader.ReadStringValue ("version");
			compatVersion = reader.ReadStringValue ("compatVersion");
			hasUserId = reader.ReadBooleanValue ("hasUserId");
			author = reader.ReadStringValue ("author");
			url = reader.ReadStringValue ("url");
			copyright = reader.ReadStringValue ("copyright");
			description = reader.ReadStringValue ("description");
			category = reader.ReadStringValue ("category");
			basePath = reader.ReadStringValue ("basePath");
			sourceAddinFile = reader.ReadStringValue ("sourceAddinFile");
			mainModule = (ModuleDescription) reader.ReadValue ("MainModule");
			optionalModules = (ModuleCollection) reader.ReadValue ("OptionalModules", new ModuleCollection ());
			nodeSets = (ExtensionNodeSetCollection) reader.ReadValue ("NodeSets", new ExtensionNodeSetCollection ());
			extensionPoints = (ExtensionPointCollection) reader.ReadValue ("ExtensionPoints", new ExtensionPointCollection ());
			conditionTypes = (ConditionTypeDescriptionCollection) reader.ReadValue ("ConditionTypes", new ConditionTypeDescriptionCollection ());
		}
	}
}
