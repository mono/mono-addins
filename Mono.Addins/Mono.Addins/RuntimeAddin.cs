//
// RuntimeAddin.cs
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
using System.Reflection;
using System.Xml;
using Mono.Addins.Description;

namespace Mono.Addins
{
	public class RuntimeAddin
	{
		string id;
		string baseDirectory;
		Addin ainfo;
		
		Assembly[] assemblies;
		RuntimeAddin[] depAddins;
		
		internal RuntimeAddin()
		{
		}
		
		internal Assembly[] Assemblies {
			get { return assemblies; }
		}
		
		public string Id {
			get { return Addin.GetIdName (id); }
		}
		
		public string Version {
			get { return Addin.GetIdVersion (id); }
		}
		
		internal Addin Addin {
			get { return ainfo; }
		}

		public Type GetType (string typeName)
		{
			return GetType (typeName, true);
		}
		
		public Type GetType (string typeName, bool throwIfNotFound)
		{
			// Look in the addin assemblies
			
			Type at = Type.GetType (typeName, false);
			if (at != null)
				return at;
			
			foreach (Assembly asm in assemblies) {
				Type t = asm.GetType (typeName, false);
				if (t != null)
					return t;
			}
			
			// Look in the dependent add-ins
			foreach (RuntimeAddin addin in depAddins) {
				Type t = addin.GetType (typeName, false);
				if (t != null)
					return t;
			}
			
			if (throwIfNotFound)
				throw new InvalidOperationException ("Type '" + typeName + "' not found in add-in '" + id + "'");
			return null;
		}
		
		public object CreateInstance (string typeName)
		{
			return CreateInstance (typeName, true);
		}
		
		public object CreateInstance (string typeName, bool throwIfNotFound)
		{
			Type type = GetType (typeName, throwIfNotFound);
			if (type == null)
				return null;
			else
				return Activator.CreateInstance (type, true);
		}
		
		public string GetFilePath (string fileName)
		{
			return Path.Combine (baseDirectory, fileName);
		}
		
		public Stream GetResource (string resourceName)
		{
			// Look in the addin assemblies
			
			foreach (Assembly asm in assemblies) {
				Stream res = asm.GetManifestResourceStream (resourceName);
				if (res != null)
					return res;
			}
			
			// Look in the dependent add-ins
			foreach (RuntimeAddin addin in depAddins) {
				Stream res = addin.GetResource (resourceName);
				if (res != null)
					return res;
			}
			
			return null;
		}
		
		internal AddinDescription Load (Addin iad)
		{
			ainfo = iad;
			
			ArrayList plugList = new ArrayList ();
			ArrayList asmList = new ArrayList ();
			
			AddinDescription description = iad.Description;
			id = description.AddinId;
			baseDirectory = description.BasePath;
			
			// Load the main modules
			LoadModule (description.MainModule, description.Namespace, plugList, asmList);
			
			// Load the optional modules, if the dependencies are present
			foreach (ModuleDescription module in description.OptionalModules) {
				if (CheckAddinDependencies (module))
					LoadModule (module, description.Namespace, plugList, asmList);
			}
			
			depAddins = (RuntimeAddin[]) plugList.ToArray (typeof(RuntimeAddin));
			assemblies = (Assembly[]) asmList.ToArray (typeof(Assembly));
			
			return description;
		}
		
		void LoadModule (ModuleDescription module, string ns, ArrayList plugList, ArrayList asmList)
		{
			// Load the assemblies
			foreach (string s in module.Assemblies)
				asmList.Add (Assembly.LoadFrom (Path.Combine (baseDirectory, s)));
				
			// Collect dependent ids
			foreach (Dependency dep in module.Dependencies) {
				AddinDependency pdep = dep as AddinDependency;
				if (pdep != null)
					plugList.Add (AddinManager.SessionService.GetAddin (Addin.GetFullId (ns, pdep.AddinId, pdep.Version)));
			}
		}
		
		internal void UnloadExtensions ()
		{
			// Create the extension points (but do not load them)
			AddinDescription emap = Addin.Description;
			if (emap == null) return;
				
			foreach (ExtensionNodeSet rel in emap.ExtensionNodeSets)
				AddinManager.SessionService.UnregisterNodeSet (rel);
		}
		
		bool CheckAddinDependencies (ModuleDescription module)
		{
			foreach (Dependency dep in module.Dependencies) {
				AddinDependency pdep = dep as AddinDependency;
				if (pdep != null && !AddinManager.SessionService.IsAddinLoaded (pdep.FullAddinId))
					return false;
			}
			return true;
		}
	}
}
