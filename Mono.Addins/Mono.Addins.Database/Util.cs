//
// Util.cs
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
using Mono.Addins.Description;
using Mono.Addins.Serialization;

namespace Mono.Addins.Database
{
	internal class Util
	{
		public static void CheckWrittableFloder (string path)
		{
			string testFile = null;
			int n = 0;
			do {
				testFile = Path.Combine (path, new Random ().Next ().ToString ());
				n++;
			} while (File.Exists (testFile) && n < 100);
			if (n == 100)
				throw new InvalidOperationException ("Could not create file in directory: " + path);
			
			StreamWriter w = new StreamWriter (testFile);
			w.Close ();
			File.Delete (testFile);
		}
		
		public static void AddDependencies (AddinDescription desc, AddinScanResult scanResult)
		{
			// Not implemented in AddinScanResult to avoid making AddinDescription remotable
			foreach (ModuleDescription mod in desc.AllModules) {
				foreach (Dependency dep in mod.Dependencies) {
					AddinDependency adep = dep as AddinDependency;
					if (adep == null) continue;
					string depid = Addin.GetFullId (desc.Namespace, adep.AddinId, adep.Version);
					scanResult.AddAddinToUpdateRelations (depid);
				}
			}
		}
		
		public static Assembly LoadAssemblyForReflection (string fileName)
		{
/*			if (!gotLoadMethod) {
				reflectionOnlyLoadFrom = typeof(Assembly).GetMethod ("ReflectionOnlyLoadFrom");
				gotLoadMethod = true;
				LoadAssemblyForReflection (typeof(Util).Assembly.Location);
			}
			
			if (reflectionOnlyLoadFrom != null)
				return (Assembly) reflectionOnlyLoadFrom.Invoke (null, new string [] { fileName });
			else
*/				return Assembly.LoadFile (fileName);
		}
		
		// Works like Path.GetFullPath, but it does not require the path to exist
		public static string GetFullPath (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
				
			if (!Path.IsPathRooted (path))
				path = Path.Combine (Environment.CurrentDirectory, path);
			
			string root = Path.GetPathRoot (path);
			path = path.Substring (root.Length);
			
			string[] parts = path.Split (Path.DirectorySeparatorChar);
			string[] newParts = new string [parts.Length];
			int i = 0;
			for (int n=0; n<parts.Length; n++) {
				string p = parts [n];
				if (p == null || p.Length == 0 || p == ".")
					continue;
				if (p == "..") {
					if (i > 0)
						i--;
				} else {
					newParts [i++] = p;
				}
			}
			return root + string.Join (new string (Path.DirectorySeparatorChar, 1), newParts, 0, i);
		}
	}
}
