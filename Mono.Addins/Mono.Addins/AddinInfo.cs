//
// AddinInfo.cs
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
using System.IO;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using Mono.Addins.Description;
using System.Collections.Immutable;
using System.Linq;

namespace Mono.Addins
{
	internal class AddinInfo
	{
		string id = "";
		string namspace = "";
		string name = "";
		string version = "";
		string baseVersion = "";
		string author = "";
		string copyright = "";
		string url = "";
		string description = "";
		string category = "";
		bool defaultEnabled = true;
		bool isroot;
		ImmutableArray<Dependency> dependencies;
		ImmutableArray<Dependency> optionalDependencies;
		AddinPropertyCollection properties;
		
		private AddinInfo ()
		{
			dependencies = ImmutableArray<Dependency>.Empty;
			optionalDependencies = ImmutableArray<Dependency>.Empty;
		}
		
		public string Id {
			get { return Addin.GetFullId (namspace, id, version); }
		}
		
		public string LocalId {
			get { return id; }
		}
		
		public string Namespace {
			get { return namspace; }
		}
		
		public bool IsRoot {
			get { return isroot; }
		}
		
		public string Name {
			get {
				string s = Properties.GetPropertyValue ("Name");
				if (s.Length > 0)
					return s;
				if (name != null && name.Length > 0)
					return name;
				string sid = id;
				if (sid.StartsWith ("__"))
					sid = sid.Substring (2);
				return Addin.GetFullId (namspace, sid, null); 
			}
		}
		
		public string Version {
			get { return version; }
		}
		
		public string BaseVersion {
			get { return baseVersion; }
		}
		
		public string Author {
			get {
				string s = Properties.GetPropertyValue ("Author");
				if (s.Length > 0)
					return s;
				return author;
			}
		}
		
		public string Copyright {
			get {
				string s = Properties.GetPropertyValue ("Copyright");
				if (s.Length > 0)
					return s;
				return copyright;
			}
		}
		
		public string Url {
			get {
				string s = Properties.GetPropertyValue ("Url");
				if (s.Length > 0)
					return s;
				return url;
			}
		}
		
		public string Description {
			get {
				string s = Properties.GetPropertyValue ("Description");
				if (s.Length > 0)
					return s;
				return description;
			}
		}
		
		public string Category {
			get {
				string s = Properties.GetPropertyValue ("Category");
				if (s.Length > 0)
					return s;
				return category;
			}
		}
		
		public bool EnabledByDefault {
			get { return defaultEnabled; }
		}
		
		public ImmutableArray<Dependency> Dependencies {
			get { return dependencies; }
		}
		
		public ImmutableArray<Dependency> OptionalDependencies {
			get { return optionalDependencies; }
		}
		
		public AddinPropertyCollection Properties {
			get { return properties; }
		}
		
		internal static AddinInfo ReadFromDescription (AddinDescription description)
		{
			AddinInfo info = new AddinInfo ();
			info.id = description.LocalId;
			info.namspace = description.Namespace;
			info.name = description.Name;
			info.version = description.Version;
			info.author = description.Author;
			info.copyright = description.Copyright;
			info.url = description.Url;
			info.description = description.Description;
			info.category = description.Category;
			info.baseVersion = description.CompatVersion;
			info.isroot = description.IsRoot;
			info.defaultEnabled = description.EnabledByDefault;

			info.dependencies = ImmutableArray<Dependency>.Empty.AddRange (description.MainModule.Dependencies);
			info.optionalDependencies = ImmutableArray<Dependency>.Empty.AddRange (description.OptionalModules.SelectMany(module => module.Dependencies));
			info.properties = description.Properties;
			
			return info;
		}
		
		public bool SupportsVersion (string version)
		{
			if (Addin.CompareVersions (Version, version) > 0)
				return false;
			if (baseVersion == "")
				return true;
			return Addin.CompareVersions (BaseVersion, version) >= 0;
		}
		
		public int CompareVersionTo (AddinInfo other)
		{
			return Addin.CompareVersions (this.version, other.Version);
		}
	}
}
