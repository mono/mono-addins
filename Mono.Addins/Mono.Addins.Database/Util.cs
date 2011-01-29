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
		static int isMono;
		static string monoVersion;
		
		public static bool IsWindows {
			get { return Path.DirectorySeparatorChar == '\\'; }
		}
		
		public static bool IsMono {
			get {
				if (isMono == 0)
					isMono = Type.GetType ("Mono.Runtime") != null ? 1 : -1;
				return isMono == 1;
			}
		}
		
		public static string MonoVersion {
			get {
				if (monoVersion == null) {
					if (!IsMono)
						throw new InvalidOperationException ();
					MethodInfo mi = Type.GetType ("Mono.Runtime").GetMethod ("GetDisplayName", BindingFlags.NonPublic|BindingFlags.Static);
					if (mi != null)
						monoVersion = (string) mi.Invoke (null, null);
					else
						monoVersion = string.Empty;
				}
				return monoVersion;
			}
		}
			
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
		
		public static string NormalizePath (string path)
		{
			if (path.Length > 2 && path [0] == '[') {
				int i = path.IndexOf (']', 1);
				if (i != -1) {
					try {
						string fname = path.Substring (1, i - 1);
						Environment.SpecialFolder sf = (Environment.SpecialFolder) Enum.Parse (typeof(Environment.SpecialFolder), fname, true);
						path = Environment.GetFolderPath (sf) + path.Substring (i + 1);
					} catch {
						// Ignore
					}
				}
			}
			if (IsWindows)
				return path.Replace ('/','\\');
			else
				return path.Replace ('\\','/');
		}
		
		// A private hash calculation method is used to be able to get consistent
		// results across different .NET versions and implementations.
		public static int GetStringHashCode (string s)
		{
			int h = 0;
			int n = 0;
			for (; n < s.Length - 1; n+=2) {
				h = unchecked ((h << 5) - h + s[n]);
				h = unchecked ((h << 5) - h + s[n+1]);
			}
			if (n < s.Length)
				h = unchecked ((h << 5) - h + s[n]);
			return h;
		}
		
		public static string GetGacPath (string fullName)
		{
			string gacDir = typeof(Uri).Assembly.Location;
			gacDir = Path.GetDirectoryName (gacDir);
			gacDir = Path.GetDirectoryName (gacDir);
			gacDir = Path.GetDirectoryName (gacDir);
			
			string[] parts = fullName.Split (',');
			if (parts.Length != 4) return null;
			string name = parts[0].Trim ();
			
			int i = parts[1].IndexOf ('=');
			string version = i != -1 ? parts[1].Substring (i+1).Trim () : parts[1].Trim ();
			
			i = parts[2].IndexOf ('=');
			string culture = i != -1 ? parts[2].Substring (i+1).Trim () : parts[2].Trim ();
			if (culture == "neutral") culture = "";
			
			i = parts[3].IndexOf ('=');
			string token = i != -1 ? parts[3].Substring (i+1).Trim () : parts[3].Trim ();
			
			string file = Path.Combine (gacDir, name);
			file = Path.Combine (file, version + "_" + culture + "_" + token);
			return file;
		}
	}
}
