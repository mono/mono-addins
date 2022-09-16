//
// TestScanDataFileGeneration.cs
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
using Mono.Addins;
using Mono.Addins.Database;
using NUnit.Framework;
using System.Linq;
using Mono.Addins.Description;

namespace UnitTests
{
	public class TestScanDataFileGeneration
	{
		static AddinRegistry GetRegistry (string dir)
		{
			return new AddinRegistry (Path.Combine (dir, "Config"), Path.Combine (dir, "App"), Path.Combine (dir, "UserAddins"));
		}

		[Test]
		public void NormalScan ()
		{
			var dir = Util.GetSampleDirectory ("ScanDataFilesTest");
			var registry = GetRegistry (dir);

			var fs = new TestAddinFileSystemExtension ();
			var scanData = fs.FileList;

			registry.RegisterExtension (fs);
			registry.Update ();

			Assert.Contains (Path.Combine (dir, "App", "SimpleApp.addins"), scanData.OpenedFiles);
			Assert.Contains (Path.Combine (dir, "App", "SimpleAddin1.addin.xml"), scanData.OpenedFiles);
			Assert.Contains (Path.Combine (dir, "App", "SimpleApp.addin.xml"), scanData.OpenedFiles);
			Assert.Contains (Path.Combine (dir, "Addins", "SimpleAddin2.addin.xml"), scanData.OpenedFiles);
			Assert.Contains (Path.Combine (dir, "Addins", "SimpleAddin3.addin.xml"), scanData.OpenedFiles);
			Assert.Contains (Path.Combine (dir, "UserAddins", "SimpleAddin4.addin.xml"), scanData.OpenedFiles);

			var addins = registry.GetAddins ().Select (a => a.Id).ToArray ();
			Assert.AreEqual (4, addins.Length);
			Assert.Contains ("SimpleApp.Ext1,0.1.0", addins);
			Assert.Contains ("SimpleApp.Ext2,0.1.0", addins);
			Assert.Contains ("SimpleApp.Ext3,0.1.0", addins);
			Assert.Contains ("SimpleApp.Ext4,0.1.0", addins);
		}

		[Test]
		public void GenerateAndLoadScanDataFiles ()
		{
			var dir = Util.GetSampleDirectory ("ScanDataFilesTest");
			var registry = GetRegistry (dir);

			registry.GenerateAddinScanDataFiles (new ConsoleProgressStatus (true), recursive:true);

			// Check that data files have been generated

			Assert.IsTrue (File.Exists (Path.Combine (dir, "App", "dir.addindata")));
			Assert.IsTrue (File.Exists (Path.Combine (dir, "App", "SimpleAddin1.addin.xml.addindata")));
			Assert.IsTrue (File.Exists (Path.Combine (dir, "App", "SimpleApp.addin.xml.addindata")));
			Assert.IsTrue (File.Exists (Path.Combine (dir, "Addins", "SimpleAddin2.addin.xml.addindata")));
			Assert.IsTrue (File.Exists (Path.Combine (dir, "Addins", "SimpleAddin3.addin.xml.addindata")));

			// But not for user add-ins
			Assert.IsFalse (File.Exists (Path.Combine (dir, "UserAddins", "SimpleAddin4.addin.xml.addindata")));

			var fs = new TestAddinFileSystemExtension ();
			var scanData = fs.FileList;

			registry.RegisterExtension (fs);
			registry.Update ();

			// Check that add-in files are not loaded, with the exception of user add-ins
			Assert.AreEqual (1, scanData.OpenedFiles.Count);
			Assert.Contains (Path.Combine (dir, "UserAddins", "SimpleAddin4.addin.xml"), scanData.OpenedFiles);

			var addins = registry.GetAddins ().Select (a => a.Id).ToArray ();
			Assert.AreEqual (4, addins.Length);
			Assert.Contains ("SimpleApp.Ext1,0.1.0", addins);
			Assert.Contains ("SimpleApp.Ext2,0.1.0", addins);
			Assert.Contains ("SimpleApp.Ext3,0.1.0", addins);
			Assert.Contains ("SimpleApp.Ext4,0.1.0", addins);
		}

		[Test]
		[TestCase (false, TestName ="Without scan data files")]
		[TestCase (true, TestName = "With scan data files")]
		public void ModifiedAddin (bool useScanDataFiles)
		{
			var dir = Util.GetSampleDirectory ("ScanDataFilesTest");
			var registry = GetRegistry (dir);

			if (useScanDataFiles)
				registry.GenerateAddinScanDataFiles (new ConsoleProgressStatus (true), recursive: true);

			registry.Update ();

			var addin = registry.GetAddin ("SimpleApp.Ext2,0.1.0");
			Assert.AreEqual ("FooValue", addin.Properties.GetPropertyValue ("Origin"));

			var addinFile = Path.Combine (dir, "Addins", "SimpleAddin2.addin.xml");
			var txt = File.ReadAllText (addinFile).Replace ("FooValue", "BarValue");
			File.WriteAllText (addinFile, txt);

			if (useScanDataFiles) {
				// Changing the add-in should not have any effect, since scan data files have not changed
				registry.Update ();
				addin = registry.GetAddin ("SimpleApp.Ext2,0.1.0");
				Assert.AreEqual ("FooValue", addin.Properties.GetPropertyValue ("Origin"));

				registry.GenerateAddinScanDataFiles (new ConsoleProgressStatus (true), recursive: true);
			}

			registry.Update (new ConsoleProgressStatus (true));
			addin = registry.GetAddin ("SimpleApp.Ext2,0.1.0");
			Assert.AreEqual ("BarValue", addin.Properties.GetPropertyValue ("Origin"));
		}

		[Test]
		[TestCase (false, TestName = "Without scan data files")]
		[TestCase (true, TestName = "With scan data files")]
		public void RemovedAddin (bool useScanDataFiles)
		{
			var dir = Util.GetSampleDirectory ("ScanDataFilesTest");
			var registry = GetRegistry (dir);

			if (useScanDataFiles)
				registry.GenerateAddinScanDataFiles (new ConsoleProgressStatus (true), recursive: true);

			registry.Update ();

			var addin = registry.GetAddin ("SimpleApp.Ext2,0.1.0");
			Assert.IsNotNull (addin);

			var addinFile = Path.Combine (dir, "Addins", "SimpleAddin2.addin.xml");
			File.Delete (addinFile);

			// Removing the add-in file should result on the add-in being unloaded, no matter if
			// scan data file is present or not

			registry.Update ();
			addin = registry.GetAddin ("SimpleApp.Ext2,0.1.0");
			Assert.IsNull (addin);

			registry.GenerateAddinScanDataFiles (new ConsoleProgressStatus (true), recursive: true);

			registry.Update (new ConsoleProgressStatus (true));
			addin = registry.GetAddin ("SimpleApp.Ext2,0.1.0");
			Assert.IsNull (addin);
		}

		[Test]
		[TestCase (false, TestName = "Without scan data files")]
		[TestCase (true, TestName = "With scan data files")]
		public void AddedAddin (bool useScanDataFiles)
		{
			var dir = Util.GetSampleDirectory ("ScanDataFilesTest");
			var registry = GetRegistry (dir);

			if (useScanDataFiles)
				registry.GenerateAddinScanDataFiles (new ConsoleProgressStatus (true), recursive: true);

			registry.Update ();

			var addinFile = Path.Combine (dir, "Addins", "SimpleAddin2.addin.xml");
			var txt = File.ReadAllText (addinFile).Replace ("Ext2", "Ext5");
			var newAddinFile = Path.Combine (dir, "Addins", "SimpleAddin5.addin.xml");
			File.WriteAllText (newAddinFile, txt);

			Addin addin;

			if (useScanDataFiles) {
				// Adding an add-in should not have any effect, since scan data files have not changed
				registry.Update ();
				addin = registry.GetAddin ("SimpleApp.Ext5,0.1.0");
				Assert.IsNull (addin);

				registry.GenerateAddinScanDataFiles (new ConsoleProgressStatus (true), recursive: true);
			}

			registry.Update (new ConsoleProgressStatus (true));
			addin = registry.GetAddin ("SimpleApp.Ext5,0.1.0");
			Assert.IsNotNull (addin);
		}

		[Test]
		public void Rescan ()
		{
			var dir = Util.GetSampleDirectory ("ScanDataFilesTest");

			// Generate the scan data files before initializing the engine
			var registry = GetRegistry (dir);
			registry.GenerateAddinScanDataFiles (new ConsoleProgressStatus (true), recursive: true);
			registry.Dispose ();

			AddinEngine engine = new AddinEngine ();
			engine.Initialize (Path.Combine (dir, "Config"), Path.Combine (dir, "UserAddins"), null, Path.Combine (dir, "App"));
			registry = engine.Registry;

			registry.Update (new ConsoleProgressStatus (false));

			engine.LoadAddin (null, "SimpleApp.Core,0.1.0");
			engine.LoadAddin (null, "SimpleApp.Ext2,0.1.0");

			File.Delete (Path.Combine (dir, "UserAddins", "SimpleAddin4.addin.xml"));

			registry.Update (new ConsoleProgressStatus (false));

			engine.Shutdown ();
		}

		[Test]
		[TestCase (true, true, TestName = "DowngradeAddins - with scan index")]
		[TestCase (true, false, TestName = "DowngradeAddins - from scan to no scan index")]
		[TestCase (false, true, TestName = "DowngradeAddins - from no scan to scan index")]
		[TestCase (false, false, TestName = "DowngradeAddins - with no scan index")]
		public void DowngradeAddins (bool hasScaIndexBefore, bool hasScanIndexAfter)
		{
			// Tests that the database is properly updated when add-ins are downgraded.

			var dir = Util.GetSampleDirectory ("ScanDataFilesTest");

			AddinRegistry registry;

			if (hasScaIndexBefore) {
				// Generate the scan data files before initializing the engine
				registry = GetRegistry (dir);
				registry.GenerateAddinScanDataFiles (new ConsoleProgressStatus (true), recursive: true);
				registry.Dispose ();
			}

			AddinEngine engine = new AddinEngine ();
			engine.Initialize (Path.Combine (dir, "Config"), Path.Combine (dir, "UserAddins"), null, Path.Combine (dir, "App"));
			registry = engine.Registry;

			var addins = registry.GetAddins ().Select (a => a.Id).ToArray ();
			Assert.AreEqual (4, addins.Length);
			Assert.Contains ("SimpleApp.Ext1,0.1.0", addins);
			Assert.Contains ("SimpleApp.Ext2,0.1.0", addins);
			Assert.Contains ("SimpleApp.Ext3,0.1.0", addins);
			Assert.Contains ("SimpleApp.Ext4,0.1.0", addins);
			engine.Shutdown ();

			// Downgrade add-ins

			SetAddinVersions (dir, "0.1.0", "0.0.1");

			if (hasScanIndexAfter) {
				// Regenerate the data files
				registry = GetRegistry (dir);
				registry.GenerateAddinScanDataFiles (new ConsoleProgressStatus (true), recursive: true);
				registry.Dispose ();
			} else {
				CleanAddinData (dir);
			}

			engine = new AddinEngine ();
			engine.Initialize (Path.Combine (dir, "Config"), Path.Combine (dir, "UserAddins"), null, Path.Combine (dir, "App"));
			registry = engine.Registry;
			registry.Update ();

			addins = registry.GetAddins ().Select (a => a.Id).ToArray ();
			Assert.AreEqual (4, addins.Length);
			Assert.Contains ("SimpleApp.Ext1,0.0.1", addins);
			Assert.Contains ("SimpleApp.Ext2,0.0.1", addins);
			Assert.Contains ("SimpleApp.Ext3,0.0.1", addins);
			Assert.Contains ("SimpleApp.Ext4,0.0.1", addins);
			engine.Shutdown ();
		}

		void SetAddinVersions (string path, string oldVersion, string newVersion)
		{
			foreach (var file in Directory.GetFiles (path, "*.xml"))
				File.WriteAllText (file, File.ReadAllText (file).Replace (oldVersion, newVersion));
			foreach (var dir in Directory.GetDirectories (path))
				SetAddinVersions (dir, oldVersion, newVersion);
		}

		void CleanAddinData (string path)
		{
			foreach (var file in Directory.GetFiles (path, "*.addindata"))
				File.Delete (file);
			foreach (var dir in Directory.GetDirectories (path))
				CleanAddinData (dir);
		}
	}

	class FileList: MarshalByRefObject
	{
		public List<string> OpenedFiles { get; } = new List<string> ();

		public void AddFile (string file)
		{
			if (!OpenedFiles.Contains (file))
				OpenedFiles.Add (file);
		}
	}

	[Serializable]
	class TestAddinFileSystemExtension: AddinFileSystemExtension
	{
		public FileList FileList = new FileList ();

		public override StreamReader OpenTextFile (string path)
		{
			FileList.AddFile (path);
			return base.OpenTextFile (path);
		}

		public override Stream OpenFile (string path)
		{
			FileList.AddFile (path);
			return base.OpenFile (path);
		}

		public override bool RequiresIsolation => false;
    }
}
