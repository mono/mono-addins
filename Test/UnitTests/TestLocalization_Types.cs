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
			// Use a new extension context for every check, since strings are cached in
			// the nodes, and every extension has its own copy of the tree

			AssertNodeTexts ("en-US", "One", "No translation");
			AssertNodeTexts ("de-DE", "Eins", "No translation");
			AssertNodeTexts ("ja-JP", "Unknown locale", "No translation");
		}

		void AssertNodeTexts (string culture, string text1, string text2)
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo (culture);

			var ctx = AddinManager.CreateExtensionContext ();

			var node = ctx.GetExtensionNode<TranslatedStringExtensionNode> ("/SimpleApp/LocalizedTexts/Translated_One");
			Assert.IsNotNull (node, culture + " n1");
			Assert.AreEqual (text1, node.Text, culture + " n1");

			node = ctx.GetExtensionNode<TranslatedStringExtensionNode> ("/SimpleApp/LocalizedTexts/Translated_Two");
			Assert.IsNotNull (node, culture + " n2");
			Assert.AreEqual (text2, node.Text, culture + " n2");
		}
	}
}

