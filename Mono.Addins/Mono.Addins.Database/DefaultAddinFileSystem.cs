// 
// DefaultAddinFileSystem.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2011 Novell, Inc (http://www.novell.com)
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

namespace Mono.Addins.Database
{
	class DefaultAddinFileSystem: AddinFileSystemExtension
	{
		IAssemblyReflector reflector;
		
		public DefaultAddinFileSystem ()
		{
		}

		#region IAddinFileSystem implementation
		public bool HandlesDirectory (string path)
		{
			return true;
		}
		
		public bool HandlesFile (string path)
		{
			return true;
		}
		
		public bool DirectoryExists (string path)
		{
			return Directory.Exists (path);
		}

		public bool FileExists (string path)
		{
			return File.Exists (path);
		}

		public System.Collections.Generic.IEnumerable<string> GetFiles (string path)
		{
			return Directory.GetFiles (path);
		}

		public System.Collections.Generic.IEnumerable<string> GetDirectories (string path)
		{
			return Directory.GetDirectories (path);
		}

		public DateTime GetLastWriteTime (string filePath)
		{
			return File.GetLastWriteTime (filePath);
		}

		public System.IO.StreamReader OpenTextFile (string path)
		{
			return new StreamReader (path);
		}

		public System.IO.Stream OpenFile (string path)
		{
			return File.OpenRead (path);
		}

		public IAssemblyReflector GetReflectorForFile (IAssemblyLocator locator, string path)
		{
			if (reflector != null)
				return reflector;
			
			// If there is a local copy of the cecil reflector, use it instead of the one in the gac
			Type t;
			string asmFile = Path.Combine (Path.GetDirectoryName (GetType().Assembly.Location), "Mono.Addins.CecilReflector.dll");
			if (File.Exists (asmFile)) {
				Assembly asm = Assembly.LoadFrom (asmFile);
				t = asm.GetType ("Mono.Addins.CecilReflector.Reflector");
			}
			else {
				string refName = GetType().Assembly.FullName;
				int i = refName.IndexOf (',');
				refName = "Mono.Addins.CecilReflector.Reflector, Mono.Addins.CecilReflector" + refName.Substring (i);
				t = Type.GetType (refName, false);
			}
			if (t != null)
				reflector = (IAssemblyReflector) Activator.CreateInstance (t);
			else
				reflector = new DefaultAssemblyReflector ();
			
			reflector.Initialize (locator);
			return reflector;
		}
		
		#endregion
	}
}

