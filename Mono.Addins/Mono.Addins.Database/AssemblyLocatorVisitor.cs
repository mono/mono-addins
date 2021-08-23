//
// AssemblyLocatorVisitor.cs
//
// Author:
//       Lluis Sanchez <llsan@microsoft.com>
//
// Copyright (c) 2018 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Mono.Addins.Database
{
	class AssemblyLocatorVisitor: AddinFolderVisitor, IAssemblyLocator
	{
		AddinRegistry registry;
		AssemblyIndex index;
		bool usePreScanDataFiles;

		public AssemblyLocatorVisitor (AddinDatabase database, AddinRegistry registry, bool usePreScanDataFiles): base (database)
		{
			this.registry = registry;
			this.usePreScanDataFiles = usePreScanDataFiles;
		}

		public string GetAssemblyLocation (string fullName)
		{
			if (index == null) {
				index = new AssemblyIndex ();
				if (registry.StartupDirectory != null)
					VisitFolder (null, registry.StartupDirectory, null, false);
				foreach (string dir in registry.GlobalAddinDirectories)
					VisitFolder (null, dir, AddinDatabase.GlobalDomain, true);
			}

			return index.GetAssemblyLocation (fullName);
		}

		protected override void OnVisitFolder (IProgressStatus monitor, string path, string domain, bool recursive)
		{
			if (usePreScanDataFiles) {
				var scanDataIndex = AddinScanDataIndex.LoadFromFolder (monitor, path);
				if (scanDataIndex != null) {
					foreach (var file in scanDataIndex.Assemblies)
						index.AddAssemblyLocation (file);
					return;
				}
			}
			base.OnVisitFolder (monitor, path, domain, recursive);
		}

		protected override void OnVisitAssemblyFile (IProgressStatus monitor, string file)
		{
			index.AddAssemblyLocation (file);
		}
	}

	class AssemblyIndex: IAssemblyLocator
	{
		Dictionary<string,List<string>> assemblyLocations = new Dictionary<string, List<string>> ();
		Dictionary<string,string> assemblyLocationsByFullName = new Dictionary<string, string> (); 

		public void AddAssemblyLocation (string file)
		{
			string name = Path.GetFileNameWithoutExtension (file);
			if (!assemblyLocations.TryGetValue (name, out var list)) {
				list = new List<string> ();
				assemblyLocations [name] = list;
			}
			list.Add (file);
		}

		public string GetAssemblyLocation (string fullName)
		{
			if (assemblyLocationsByFullName.TryGetValue (fullName, out var loc))
				return loc;

			int i = fullName.IndexOf (',');
			string name = fullName.Substring (0, i);
			if (name == "Mono.Addins")
				return typeof (AssemblyIndex).Assembly.Location;

			if (!assemblyLocations.TryGetValue (name, out var list))
				return null;

			string lastAsm = null;
			for (int n = list.Count - 1; n >= 0; --n) {
				try {
					var file = list[n];
					list.RemoveAt(n);

					AssemblyName aname = AssemblyName.GetAssemblyName (file);
					lastAsm = file;
					assemblyLocationsByFullName [aname.FullName] = file;
					if (aname.FullName == fullName)
						return file;
				} catch {
					// Could not get the assembly name. The file either doesn't exist or it is not a valid assembly.
					// In this case, just ignore it.
				}
			}

			// If we got here, we removed all the list's items.
			assemblyLocations.Remove (name);

			if (lastAsm != null) {
				// If an exact version is not found, just take any of them
				assemblyLocationsByFullName[fullName] = lastAsm;
				return lastAsm;
			}
			return null;
		}
	}
}
