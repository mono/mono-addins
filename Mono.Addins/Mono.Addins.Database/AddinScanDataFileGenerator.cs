//
// AddinScanDataFileGenerator.cs
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
using System.Collections.Generic;
using System.IO;
using Mono.Addins.Description;

namespace Mono.Addins.Database
{
	class AddinScanDataFileGenerator: AddinFolderVisitor, IDisposable
	{
		string rootFolder;
		AddinScanner scanner;
		AddinDatabase database;
		AddinScanDataIndex scanDataIndex;
		AssemblyLocator locator;
		AssemblyIndex assemblyIndex;

		List<string> foundFiles = new List<string> ();
		List<string> foundAssemblies = new List<string> ();

		public AddinScanDataFileGenerator (AddinDatabase database, AddinRegistry registry, string rootFolder): base (database)
		{
			this.database = database;
			this.rootFolder = Path.GetFullPath (rootFolder);

			assemblyIndex = new AssemblyIndex ();
			locator = new AssemblyLocator (database, registry, assemblyIndex);
			scanner = new AddinScanner (database, locator);
		}

		protected override void OnVisitFolder (IProgressStatus monitor, string path, string domain, bool recursive)
		{
			if (path == rootFolder) {

				// Create an index
				scanDataIndex = new AddinScanDataIndex ();

				base.OnVisitFolder (monitor, path, domain, recursive);

				// Scan the files after visiting the folder tree. At this point the assembly index will be complete
				// and will be able to resolve assemblies during the add-in scan

				foreach (var file in foundFiles) {
					if (scanner.ScanConfigAssemblies (monitor, file, ScanContext, out var config) && config != null)
						StoreScanDataFile (monitor, file, config);
				}
				foreach (var file in foundAssemblies) {
					if (scanner.ScanAssembly (monitor, file, ScanContext, out var config) && config != null)
						StoreScanDataFile (monitor, file, config);

					// The index contains a list of all assemblies, no matter if they are add-ins or not
					scanDataIndex.Assemblies.Add (file);
				}

				foundFiles.Clear ();
				foundAssemblies.Clear ();

				scanDataIndex.SaveToFolder (path);
				scanDataIndex = null;
			} else
				base.OnVisitFolder (monitor, path, domain, recursive);
		}

		protected override void OnVisitAddinManifestFile (IProgressStatus monitor, string file)
		{
			if (scanDataIndex != null)
				foundFiles.Add (file);
		}

		protected override void OnVisitAssemblyFile (IProgressStatus monitor, string file)
		{
			if (!Util.IsManagedAssembly (file))
				return;
			
			assemblyIndex.AddAssemblyLocation (file);

			if (scanDataIndex != null)
				foundAssemblies.Add (file);
		}

		void StoreScanDataFile (IProgressStatus monitor, string file, AddinDescription config)
		{
			// Save a binary data file next to the scanned file
			var scanDataFile = file + ".addindata";
			database.SaveDescription (monitor, config, scanDataFile);
			var md5 = Util.GetMD5 (scanDataFile);
			scanDataIndex.Files.Add (new AddinScanData (file, md5));
		}

		public void Dispose ()
		{
			scanner.Dispose ();
		}

		class AssemblyLocator : IAssemblyLocator
		{
			// This is a custom assembly locator that will look first at the
			// assembly index being generated during the add-in lookup, and
			// will use a global assembly locator as fallback.

			AssemblyLocatorVisitor globalLocator;
			AssemblyIndex index;

			public AssemblyLocator (AddinDatabase database, AddinRegistry registry, AssemblyIndex index)
			{
				this.index = index;
				globalLocator = new AssemblyLocatorVisitor (database, registry, false);
			}

			public string GetAssemblyLocation (string fullName)
			{
				var res = index.GetAssemblyLocation (fullName);
				if (res != null)
					return res;

				// Fallback to a global visitor

				return globalLocator.GetAssemblyLocation (fullName);
			}
		}
	}
}
