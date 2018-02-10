// 
// TestAddinDescription.cs
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
using NUnit.Framework;
using System.IO;
using Mono.Addins.Description;
using System.Globalization;
using Mono.Addins;
using System.Xml;

namespace UnitTests
{
	[TestFixture]
	public class TestAddinDescription: TestBase
	{
		CultureInfo oldc;
		
		[SetUp]
		public void TestSetup ()
		{
			oldc = System.Threading.Thread.CurrentThread.CurrentCulture;
			CultureInfo ci = CultureInfo.GetCultureInfo("ca-ES");
			System.Threading.Thread.CurrentThread.CurrentCulture = ci;
		}
		
		[TearDown]
		public void TestTeardown ()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = oldc;
		}
		
		[Test]
		public void PropertyLocalization ()
		{
			AddinDescription desc = new AddinDescription ();
			
			desc.Properties.SetPropertyValue ("prop1", "val1");
			Assert.AreEqual ("val1", desc.Properties.GetPropertyValue ("prop1"));
			Assert.AreEqual ("val1", desc.Properties.GetPropertyValue ("prop1", "en"));
			Assert.AreEqual ("val1", desc.Properties.GetPropertyValue ("prop1", "en-US"));
			Assert.AreEqual ("val1", desc.Properties.GetPropertyValue ("prop1", "en_US"));
			Assert.AreEqual ("val1", desc.Properties.GetPropertyValue ("prop1", null));
			
			desc.Properties.SetPropertyValue ("prop2", "valCa", "ca");
			Assert.AreEqual ("valCa", desc.Properties.GetPropertyValue ("prop2"));
			Assert.AreEqual ("valCa", desc.Properties.GetPropertyValue ("prop2", "ca"));
			Assert.AreEqual ("valCa", desc.Properties.GetPropertyValue ("prop2", "ca-ES"));
			Assert.AreEqual ("valCa", desc.Properties.GetPropertyValue ("prop2", "ca_ES"));
			Assert.AreEqual ("valCa", desc.Properties.GetPropertyValue ("prop2", "ca-AN"));
			Assert.AreEqual ("valCa", desc.Properties.GetPropertyValue ("prop2", "ca_AN"));
			Assert.IsEmpty (desc.Properties.GetPropertyValue ("prop2", "en"));
			Assert.IsEmpty (desc.Properties.GetPropertyValue ("prop2", "en-US"));
			Assert.IsEmpty (desc.Properties.GetPropertyValue ("prop2", "en_US"));
			Assert.IsEmpty (desc.Properties.GetPropertyValue ("prop2", null));
			
			desc.Properties.SetPropertyValue ("prop2", "valCaEs", "ca_ES");
			Assert.AreEqual ("valCaEs", desc.Properties.GetPropertyValue ("prop2"));
			Assert.AreEqual ("valCa", desc.Properties.GetPropertyValue ("prop2", "ca"));
			Assert.AreEqual ("valCaEs", desc.Properties.GetPropertyValue ("prop2", "ca-ES"));
			Assert.AreEqual ("valCaEs", desc.Properties.GetPropertyValue ("prop2", "ca_ES"));
			Assert.AreEqual ("valCa", desc.Properties.GetPropertyValue ("prop2", "ca-AN"));
			Assert.AreEqual ("valCa", desc.Properties.GetPropertyValue ("prop2", "ca_AN"));
			Assert.IsEmpty (desc.Properties.GetPropertyValue ("prop2", "en"));
			Assert.IsEmpty (desc.Properties.GetPropertyValue ("prop2", "en-US"));
			Assert.IsEmpty (desc.Properties.GetPropertyValue ("prop2", "en_US"));
			Assert.IsEmpty (desc.Properties.GetPropertyValue ("prop2", null));
			
			desc.Properties.SetPropertyValue ("prop2", "val4", null);
			Assert.AreEqual ("valCaEs", desc.Properties.GetPropertyValue ("prop2"));
			Assert.AreEqual ("valCa", desc.Properties.GetPropertyValue ("prop2", "ca"));
			Assert.AreEqual ("valCaEs", desc.Properties.GetPropertyValue ("prop2", "ca-ES"));
			Assert.AreEqual ("valCaEs", desc.Properties.GetPropertyValue ("prop2", "ca_ES"));
			Assert.AreEqual ("valCa", desc.Properties.GetPropertyValue ("prop2", "ca-AN"));
			Assert.AreEqual ("valCa", desc.Properties.GetPropertyValue ("prop2", "ca_AN"));
			Assert.AreEqual ("val4", desc.Properties.GetPropertyValue ("prop2", "en"));
			Assert.AreEqual ("val4", desc.Properties.GetPropertyValue ("prop2", "en-US"));
			Assert.AreEqual ("val4", desc.Properties.GetPropertyValue ("prop2", "en_US"));
			Assert.AreEqual ("val4", desc.Properties.GetPropertyValue ("prop2", null));
		}
		
		[Test]
		public void PropertiesFromAddin ()
		{
			Addin ad = AddinManager.Registry.GetAddin ("SimpleApp.Core");
			
			Assert.AreEqual ("Una aplicació simple", ad.Name);
			Assert.AreEqual ("A simple application", ad.Properties.GetPropertyValue ("Name","en-US"));
			Assert.AreEqual ("SimpleApp description", ad.Description.Description);
			Assert.AreEqual ("Lluis Sanchez", ad.Description.Author);
			Assert.AreEqual ("GPL", ad.Description.Copyright);
			Assert.AreEqual ("Val1", ad.Properties.GetPropertyValue ("Prop1","en-US"));
			Assert.AreEqual ("Val1Cat", ad.Properties.GetPropertyValue ("Prop1"));
			Assert.AreEqual ("Val2", ad.Properties.GetPropertyValue ("Prop2","en-US"));
			Assert.AreEqual ("Val2Cat", ad.Properties.GetPropertyValue ("Prop2"));
			
			oldc = System.Threading.Thread.CurrentThread.CurrentCulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("es-ES");
			
			Assert.AreEqual ("Una aplicación simple", ad.Name);
			Assert.AreEqual ("Descripción de SimpleApp", ad.Description.Description);
			
			System.Threading.Thread.CurrentThread.CurrentCulture = oldc;
		}
		
		AddinDescription DescFromResource (string res)
		{
			using (Stream s = GetType().Assembly.GetManifestResourceStream (res)) {
				return AddinDescription.Read (s, ".");
			}
		}
		
		XmlDocument XmlFromResource (string res)
		{
			using (Stream s = GetType().Assembly.GetManifestResourceStream (res)) {
				XmlDocument doc = new XmlDocument ();
				doc.Load (s);
				return doc;
			}
		}
		
		[Test]
		public void ReadCoreProperties ()
		{
			AddinDescription desc = DescFromResource ("TestManifest2.xml");
			
			Assert.AreEqual ("Core", desc.LocalId);
			Assert.AreEqual ("0.1.0", desc.Version);
			Assert.AreEqual ("0.0.1", desc.CompatVersion);
			Assert.AreEqual (false, desc.EnabledByDefault);
			Assert.AreEqual (AddinFlags.CantDisable | AddinFlags.CantUninstall | AddinFlags.Hidden, desc.Flags);
			Assert.AreEqual (true, desc.IsRoot);
			Assert.AreEqual ("SimpleApp", desc.Namespace);
		}
		
		[Test]
		public void WriteCorePropertiesAsElems ()
		{
			AddinDescription desc = DescFromResource ("TestManifest2.xml");
			XmlDocument doc1 = XmlFromResource ("TestManifest2.xml");
			
			XmlDocument doc2 = desc.SaveToXml ();
			Assert.AreEqual (Util.Infoset (doc1), Util.Infoset (doc2));
			
			desc.LocalId = "Core2";
			desc.Version = "0.2.0";
			desc.CompatVersion = "0.0.2";
			desc.EnabledByDefault = true;
			desc.Flags = AddinFlags.CantUninstall;
			desc.IsRoot = false;
			desc.Namespace = "SimpleApp2";
			
			doc1 = XmlFromResource ("TestManifest2-bis.xml");
			doc2 = desc.SaveToXml ();
			
			Assert.AreEqual (Util.Infoset (doc1), Util.Infoset (doc2));
		}
		
		[Test]
		public void WriteCorePropertiesAsProps ()
		{
			AddinDescription desc = DescFromResource ("TestManifest3.xml");
			XmlDocument doc1 = XmlFromResource ("TestManifest3.xml");
			XmlDocument doc2 = desc.SaveToXml ();
			Assert.AreEqual (Util.Infoset (doc1), Util.Infoset (doc2));
		}

		[Test]
		public void FileGlobbingTest ()
		{
			string pathToTestFolder = Path.Combine ("..", "..", "..", "ImportGlobFileExtension");
			AddinDescription desc = AddinDescription.Read (Path.Combine (pathToTestFolder, "ImportGlobFileExtension.addin.xml"));

			var allFiles = desc.AllFiles;
			Assert.IsNotNull (allFiles);
			CollectionAssert.IsNotEmpty (allFiles);
			Assert.AreEqual (5, allFiles.Count);
			CollectionAssert.AreEquivalent (new string [] {
				"file1.txt",
				Path.Combine ("dir1", "bar.bin"),
				Path.Combine ("dir1", "foo.bin"),
				Path.Combine ("dir2", "subdir", "subfile.txt"),
				Path.Combine ("dir2", "subdir", "subsubdir", "subsubfile.txt")
			}, allFiles);
		}
	}
}

