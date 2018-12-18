//
// TestLocalization_Types.cs
//
// Author:
//       Marius Ungureanu <maungu@microsoft.com>
//
// Copyright (c) 2018 Microsoft Inc.
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
using Mono.Addins;
using System.IO;
using System.Threading;
using System.Globalization;
using SimpleApp;

namespace UnitTests
{
	[TestFixture ()]
	public class TestLocalizationTypes : TestBase
	{
		public override void Setup ()
		{
			base.Setup ();
		}

		[Test]
		public void TestCustomLocalizer ()
		{
			ExtensionContext ctx;
			ExtensionNode node;

			// Use a new extension context for every check, since strings are cached in
			// the nodes, and every extension has its own copy of the tree

			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
			ctx = AddinManager.CreateExtensionContext ();

			node = ctx.GetExtensionNode ("/SimpleApp/LocalizedTexts/Translated_One");
			Assert.IsNotNull (node, "t1.1");
			Assert.AreEqual ("One", node.ToString ());
			node = ctx.GetExtensionNode ("/SimpleApp/LocalizedTexts/Translated_Two");
			Assert.IsNotNull (node, "t1.2");
			Assert.AreEqual ("No translation", node.ToString ());

			Thread.CurrentThread.CurrentCulture = new CultureInfo ("de-DE");
			node = ctx.GetExtensionNode ("/SimpleApp/LocalizedTexts/Translated_One");
			Assert.IsNotNull (node, "t1.1");
			Assert.AreEqual ("Eins", node.ToString ());
			node = ctx.GetExtensionNode ("/SimpleApp/LocalizedTexts/Translated_Two");
			Assert.IsNotNull (node, "t1.2");
			Assert.AreEqual ("No translation", node.ToString ());

			Thread.CurrentThread.CurrentCulture = new CultureInfo ("jp-JP");
			Assert.IsNotNull (node, "t1.1");
			Assert.AreEqual ("Unknown locale", node.ToString ());
			node = ctx.GetExtensionNode ("/SimpleApp/LocalizedTexts/Translated_Two");
			Assert.IsNotNull (node, "t1.2");
			Assert.AreEqual ("No translation", node.ToString ());
		}
	}
}

