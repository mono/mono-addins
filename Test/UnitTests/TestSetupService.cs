// 
// TestSetupService.cs
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
using System.Linq;
using NUnit.Framework;
using Mono.Addins.Setup;
using Mono.Addins;
using System.IO;
using System.Globalization;

namespace UnitTests
{
	[TestFixture]
	public class TestSetupService: TestBase
	{
		SetupService setup;
		string addinsDir;
		string repoDir;
		string repoExtrasDir;
		string baseDir;
		ConsoleProgressStatus monitor;
		bool repoBuilt;
		string repoUrl;
		string repoBaseUrl;
		
		[SetUp]
		public void TestSetup ()
		{
			setup = new SetupService ();
			baseDir = Path.GetDirectoryName (new Uri (typeof(TestBase).Assembly.CodeBase).LocalPath);
			addinsDir = new DirectoryInfo (baseDir).Parent.Parent.Parent.Parent.FullName;
			addinsDir = Path.Combine (addinsDir, "lib");
			
			repoDir = Path.Combine (TempDir, "repo");
			repoExtrasDir = Path.Combine (repoDir,"extras");
			monitor = new ConsoleProgressStatus (true);
			
			repoBaseUrl = new Uri (repoDir).ToString ();
			repoUrl = new Uri (Path.Combine (repoDir, "main.mrep")).ToString ();
		}
		
		[Test]
		public void AddinPackageCreation ()
		{
			repoBuilt = false;
			if (Directory.Exists (repoDir))
				Directory.Delete (repoDir, true);
			Directory.CreateDirectory (repoDir);
			Directory.CreateDirectory (repoExtrasDir);
			
			string asm = Path.Combine (baseDir, "SimpleApp.addin.xml");
			string[] p = setup.BuildPackage (monitor, repoDir, asm);
			Assert.IsTrue (File.Exists (p[0]));
			
			asm = Path.Combine (addinsDir, "HelloWorldExtension.dll");
			p = setup.BuildPackage (monitor, repoDir, asm);
			Assert.IsTrue (File.Exists (p[0]));
			
			asm = Path.Combine (addinsDir, "CommandExtension.addin.xml");
			p = setup.BuildPackage (monitor, repoDir, asm);
			Assert.IsTrue (File.Exists (p[0]));
			
			asm = Path.Combine (addinsDir, "SystemInfoExtension.addin.xml");
			p = setup.BuildPackage (monitor, repoDir, asm);
			Assert.IsTrue (File.Exists (p[0]));
			
			asm = Path.Combine (addinsDir, "MultiAssemblyAddin.dll");
			p = setup.BuildPackage (monitor, repoDir, asm);
			Assert.IsTrue (File.Exists (p[0]));
			
			string extras = Path.Combine (addinsDir, "extras");
			asm = Path.Combine (extras, "ExtraExtender.addin.xml");
			p = setup.BuildPackage (monitor, repoDir, asm);
			Assert.IsTrue (File.Exists (p[0]));
			
			asm = Path.Combine (extras, "FileContentExtension.addin.xml");
			p = setup.BuildPackage (monitor, repoDir, asm);
			Assert.IsTrue (File.Exists (p[0]));
		}
		
		public void InitRepository ()
		{
			if (repoBuilt)
				return;
			
			if (Directory.Exists (repoDir))
				Directory.Delete (repoDir, true);
			Directory.CreateDirectory (repoDir);
			Directory.CreateDirectory (repoExtrasDir);
			
			string asm = Path.Combine (baseDir, "SimpleApp.addin.xml");
			setup.BuildPackage (monitor, repoDir, asm);
			
			asm = Path.Combine (addinsDir, "HelloWorldExtension.dll");
			setup.BuildPackage (monitor, repoDir, asm);
			
			asm = Path.Combine (addinsDir, "CommandExtension.addin.xml");
			setup.BuildPackage (monitor, repoDir, asm);
			
			asm = Path.Combine (addinsDir, "SystemInfoExtension.addin.xml");
			setup.BuildPackage (monitor, repoDir, asm);
			
			asm = Path.Combine (addinsDir, "MultiAssemblyAddin.dll");
			setup.BuildPackage (monitor, repoDir, asm);
			
			string extras = Path.Combine (addinsDir, "extras");
			asm = Path.Combine (extras, "ExtraExtender.addin.xml");
			setup.BuildPackage (monitor, repoExtrasDir, asm);
			
			asm = Path.Combine (extras, "FileContentExtension.addin.xml");
			setup.BuildPackage (monitor, repoExtrasDir, asm);
			
			repoBuilt = true;
		}
		
		[Test]
		public void BuildRepository ()
		{
			InitRepository ();
			setup.BuildRepository (monitor, repoDir);
			
			Assert.IsTrue (File.Exists (Path.Combine (repoDir, "main.mrep")));
			Assert.IsTrue (File.Exists (Path.Combine (repoDir, "root.mrep")));
			Assert.IsTrue (File.Exists (Path.Combine (repoExtrasDir, "main.mrep")));
		}
		
		[Test]
		public void ManageRepository ()
		{
			// Register without .mrep reference
			var ar = setup.Repositories.RegisterRepository (monitor, repoBaseUrl, false);
			Assert.IsTrue (ar.Enabled);
			Assert.AreEqual (repoUrl, ar.Url);
			Assert.IsTrue (setup.Repositories.ContainsRepository (repoUrl));
			
			setup.Repositories.RemoveRepository (repoUrl);
			Assert.IsFalse (setup.Repositories.ContainsRepository (repoUrl));
			
			// Register with .mrep reference
			ar = setup.Repositories.RegisterRepository (monitor, repoUrl, false);
			Assert.IsTrue (ar.Enabled);
			Assert.AreEqual (repoUrl, ar.Url);
			Assert.IsTrue (setup.Repositories.ContainsRepository (repoUrl));
			
			var reps = setup.Repositories.GetRepositories ();
			Assert.AreEqual (1, reps.Length);
			Assert.IsTrue (reps[0].Enabled);
			Assert.AreEqual (repoUrl, reps[0].Url);
			
			setup.Repositories.RemoveRepository (repoUrl);
			Assert.IsFalse (setup.Repositories.ContainsRepository (repoUrl));
		}
		
		[Test]
		public void QueryRepository ()
		{
			InitRepository ();
			setup.BuildRepository (monitor, repoDir);
			setup.Repositories.RegisterRepository (monitor, repoUrl, true);
			
			var addins = setup.Repositories.GetAvailableAddins ();
			Assert.AreEqual (7, addins.Length);
			
			var oldc = System.Threading.Thread.CurrentThread.CurrentCulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ca-ES");
			
//			CheckAddin (addins, "", "", "0.0.0.0", "", "", "", "", "", "");
			CheckAddin (addins, "SimpleApp.CommandExtension,0.1.0", "SimpleApp", "0.1.0", "", "CommandExtension", "SimpleApp/Extensions", "Lluis Sanchez", "GPL", "CommandExtension");
			CheckAddin (addins, "SimpleApp.Core,0.1.0", "SimpleApp", "0.1.0", "", "Una aplicació simple", "SimpleApp", "Lluis Sanchez", "GPL", "SimpleApp description");
			CheckProperties (addins, "SimpleApp.Core,0.1.0", "Prop1", "Val1Cat", "Prop2", "Val2Cat");
			CheckAddin (addins, "SimpleApp.HelloWorldExtension,0.1.0", "SimpleApp", "0.1.0", "", "SimpleApp.HelloWorldExtension", "", "", "", "");
			CheckAddin (addins, "SimpleApp.SystemInfoExtension,0.1.0", "SimpleApp", "0.1.0", "", "SystemInfoExtension", "SimpleApp/Extensions", "Lluis Sanchez", "GPL", "SystemInfoExtension");
			CheckAddin (addins, "SimpleApp.ExtraExtender,0.1.0", "SimpleApp", "0.1.0", "", "SimpleApp.ExtraExtender", "", "", "", "");
			CheckAddin (addins, "SimpleApp.FileContentExtension,0.1.0", "SimpleApp", "0.1.0", "", "FileContentExtension", "SimpleApp/Extensions", "Lluis Sanchez", "GPL", "FileContentExtension");
			
			System.Threading.Thread.CurrentThread.CurrentCulture = oldc;
			setup.Repositories.RemoveRepository (repoUrl);
		}
		
		void CheckAddin (AddinRepositoryEntry[] areps, string id, string ns, string version, string compat, string name, string category, string author, string copyright, string desc)
		{
			AddinRepositoryEntry arep = areps.FirstOrDefault (a => a.Addin.Id == id);
			Assert.IsNotNull (arep, "Add-in " + id + " not found");
			Assert.AreEqual (id, arep.Addin.Id);
			Assert.AreEqual (ns, arep.Addin.Namespace);
			Assert.AreEqual (version, arep.Addin.Version);
			Assert.AreEqual (compat, arep.Addin.BaseVersion);
			Assert.AreEqual (name, arep.Addin.Name);
			Assert.AreEqual (category, arep.Addin.Category);
			Assert.AreEqual (author, arep.Addin.Author);
			Assert.AreEqual (copyright, arep.Addin.Copyright);
			Assert.AreEqual (desc, arep.Addin.Description);
		}
		
		void CheckProperties (AddinRepositoryEntry[] areps, string id, params string[] props)
		{
			AddinRepositoryEntry arep = areps.FirstOrDefault (a => a.Addin.Id == id);
			for (int n=0; n<props.Length; n+=2)
				Assert.AreEqual (props[n+1], arep.Addin.Properties.GetPropertyValue (props[n]));
		}
		
		[Test]
		public void GetSupportFile ()
		{
			InitRepository ();
			setup.BuildRepository (monitor, repoDir);
			setup.Repositories.RegisterRepository (monitor, repoUrl, true);
			
			AddinRepositoryEntry arep = setup.Repositories.GetAvailableAddin ("SimpleApp.Core", "0.1.0")[0];
			IAsyncResult res = arep.BeginDownloadSupportFile (arep.Addin.Properties.GetPropertyValue ("Prop3"), null, null);
			Stream s = arep.EndDownloadSupportFile (res);
			StreamReader sr = new StreamReader (s);
			Assert.AreEqual ("Some support file", sr.ReadToEnd ());
			sr.Close ();
			
			setup.Repositories.RemoveRepository (repoUrl);
		}
		
		[Test]
		public void UpgradeCoreAddin ()
		{
			InitRepository ();
			CreateTestPackage ("10.3");
			CreateTestPackage ("10.4");
			
			ExtensionNodeList list = AddinManager.GetExtensionNodes ("/SimpleApp/InstallUninstallTest");
			Assert.AreEqual (1, list.Count);
			Assert.AreEqual ("10.2", ((ItemSetNode)list[0]).Label);
			
			Addin adn = AddinManager.Registry.GetAddin ("SimpleApp.AddRemoveTest,10.2");
			Assert.IsTrue (adn.Enabled);
			Assert.IsFalse (adn.Description.CanUninstall); // Core add-in can't be uninstalled
			
			setup.Repositories.RegisterRepository (monitor, repoUrl, true);
			
			var pkg = setup.Repositories.GetAvailableAddinUpdates ("SimpleApp.AddRemoveTest", RepositorySearchFlags.LatestVersionsOnly).FirstOrDefault ();
			Assert.IsNotNull (pkg);
			Assert.AreEqual ("SimpleApp.AddRemoveTest,10.4", pkg.Addin.Id);
			
			// Install the add-in
			
			setup.Install (new ConsoleProgressStatus (true), pkg);
			
			adn = AddinManager.Registry.GetAddin ("SimpleApp.AddRemoveTest,10.2");
			Assert.IsFalse (adn.Enabled);
			
			list = AddinManager.GetExtensionNodes ("/SimpleApp/InstallUninstallTest");
			Assert.AreEqual (1, list.Count);
			Assert.AreEqual ("10.4", ((ItemSetNode)list[0]).Label);
			
			adn = AddinManager.Registry.GetAddin ("SimpleApp.AddRemoveTest");
			Assert.IsTrue (adn.Enabled);
			Assert.IsTrue (adn.Description.CanUninstall);
			
			// Uninstall the add-in
			
			setup.Uninstall (new ConsoleProgressStatus (true), "SimpleApp.AddRemoveTest,10.4");
			
			list = AddinManager.GetExtensionNodes ("/SimpleApp/InstallUninstallTest");
			Assert.AreEqual (0, list.Count);
			
			adn = AddinManager.Registry.GetAddin ("SimpleApp.AddRemoveTest");
			Assert.IsFalse (adn.Enabled);
			
			adn.Enabled = true;
			
			list = AddinManager.GetExtensionNodes ("/SimpleApp/InstallUninstallTest");
			Assert.AreEqual (1, list.Count);
			Assert.AreEqual ("10.2", ((ItemSetNode)list[0]).Label);
		}
		
		void CreateTestPackage (string newVersion)
		{
			string file = AddinManager.CurrentAddin.GetFilePath (Path.Combine ("SampleAddins","AddRemoveTest1.addin.xml"));
			string txt = File.ReadAllText (file);
			txt = txt.Replace ("10.1", newVersion);
			
			string tmpFile = Path.Combine (TempDir, "AddRemoveTest_" + newVersion + ".addin.xml");
			File.WriteAllText (tmpFile, txt);
			
			setup.BuildPackage (monitor, repoDir, tmpFile);
			setup.BuildRepository (monitor, repoDir);
		}
	}
}

