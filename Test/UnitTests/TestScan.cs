//
// TestScan.cs
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
using Mono.Addins;
using NUnit.Framework;

namespace UnitTests
{
	public class TestScan
	{
		[OneTimeSetUp]
		public virtual void Setup ()
		{
		}

		[OneTimeTearDown]
		public virtual void Teardown ()
		{
		}

		[Test]
		[TestCase (",0.0.1", TestName = "Lower version")]
		[TestCase (",0.1.0", TestName = "Exact version")]
		[TestCase ("", TestName = "No version")]
		public void ResolveDuplicateAddin (string version)
		{
			var dir = Util.GetSampleDirectory ("ScanTest");
			var registry = new AddinRegistry (Path.Combine (dir, "Config"), Path.Combine (dir, "App"), Path.Combine (dir, "Addins"));
			registry.Rebuild (new ConsoleProgressStatus (true));

			// Query using a lower version number
			var addin = registry.GetAddin ("SimpleApp.Ext" + version);
			Assert.IsNotNull (addin);
			Assert.AreEqual ("AddinsDir", addin.Properties.GetPropertyValue ("Origin"));

			// Now try deleting the add-in

			var addinPath = Path.Combine (dir, "Addins", "SimpleAddin.addin.xml");
			File.Delete (addinPath);

			registry.Update (new ConsoleProgressStatus (true));
			addin = registry.GetAddin ("SimpleApp.Ext" + version);
			Assert.IsNotNull (addin);
			Assert.AreEqual ("AppDir", addin.Properties.GetPropertyValue ("Origin"));
		}

		[Test]
		[TestCase (",0.0.1", TestName = "Lower version check")]
		[TestCase (",0.1.0", TestName = "Exact version check")]
		[TestCase ("", TestName = "No version check")]
		public void ResolveInstalledDuplicateAddin (string version)
		{
			var dir = Util.GetSampleDirectory ("ScanTest");
			var registry = new AddinRegistry (Path.Combine (dir, "Config"), Path.Combine (dir, "App"), Path.Combine (dir, "Addins"));

			// Hide the user addin. We'll simulate that the add-in is added
			var addinPath = Path.Combine (dir, "Addins", "SimpleAddin.addin.xml");
			File.Move (addinPath, addinPath + ".x");

			registry.Rebuild (new ConsoleProgressStatus (true));

			// Query using a lower version number
			var addin = registry.GetAddin ("SimpleApp.Ext" + version);
			Assert.IsNotNull (addin);
			Assert.AreEqual ("AppDir", addin.Properties.GetPropertyValue ("Origin"));

			// Now simulate that the add-in is added

			File.Move (addinPath + ".x", addinPath);
			registry.Update (new ConsoleProgressStatus (true));
			addin = registry.GetAddin ("SimpleApp.Ext" + version);
			Assert.IsNotNull (addin);
			Assert.AreEqual ("AddinsDir", addin.Properties.GetPropertyValue ("Origin"));
		}
	}
}
