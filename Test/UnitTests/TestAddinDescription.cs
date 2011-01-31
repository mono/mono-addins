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

namespace UnitTests
{
	public class TestAddinDescription
	{
		CultureInfo oldc;
		
		[SetUp]
		public void Setup ()
		{
			oldc = System.Threading.Thread.CurrentThread.CurrentCulture;
			CultureInfo ci = CultureInfo.GetCultureInfo("ca-ES");
			System.Threading.Thread.CurrentThread.CurrentCulture = ci;
		}
		
		[TearDown]
		public void Teardown ()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = oldc;
		}
		
		[Test]
		public void PropertyLocalization ()
		{
			AddinDescription desc = new AddinDescription ();
			
			desc.Properties.SetPropertyValue ("prop1", null, "val1");
			Assert.Equals ("val1", desc.Properties.GetPropertyValue ("prop1"));
			Assert.Equals ("val1", desc.Properties.GetPropertyValue ("prop1", "en"));
			Assert.Equals ("val1", desc.Properties.GetPropertyValue ("prop1", "en-US"));
			Assert.Equals ("val1", desc.Properties.GetPropertyValue ("prop1", "en_US"));
			Assert.Equals ("val1", desc.Properties.GetPropertyValue ("prop1", null));
			
			desc.Properties.SetPropertyValue ("prop2", "ca", "val2");
			Assert.Equals ("val2", desc.Properties.GetPropertyValue ("prop2"));
			Assert.Equals ("val2", desc.Properties.GetPropertyValue ("prop2", "ca"));
			Assert.Equals ("val2", desc.Properties.GetPropertyValue ("prop2", "ca-ES"));
			Assert.Equals ("val2", desc.Properties.GetPropertyValue ("prop2", "ca_ES"));
			Assert.Equals ("val2", desc.Properties.GetPropertyValue ("prop2", "ca-AN"));
			Assert.Equals ("val2", desc.Properties.GetPropertyValue ("prop2", "ca_AN"));
			Assert.IsEmpty (desc.Properties.GetPropertyValue ("prop2", "en"));
			Assert.IsEmpty (desc.Properties.GetPropertyValue ("prop2", "en-US"));
			Assert.IsEmpty (desc.Properties.GetPropertyValue ("prop2", "en_US"));
			Assert.IsEmpty (desc.Properties.GetPropertyValue ("prop2", null));
			
			desc.Properties.SetPropertyValue ("prop2", "ca_ES", "val3");
			Assert.Equals ("val2", desc.Properties.GetPropertyValue ("prop2"));
			Assert.Equals ("val2", desc.Properties.GetPropertyValue ("prop2", "ca"));
			Assert.Equals ("val3", desc.Properties.GetPropertyValue ("prop2", "ca-ES"));
			Assert.Equals ("val3", desc.Properties.GetPropertyValue ("prop2", "ca_ES"));
			Assert.Equals ("val2", desc.Properties.GetPropertyValue ("prop2", "ca-AN"));
			Assert.Equals ("val2", desc.Properties.GetPropertyValue ("prop2", "ca_AN"));
			Assert.IsEmpty (desc.Properties.GetPropertyValue ("prop2", "en"));
			Assert.IsEmpty (desc.Properties.GetPropertyValue ("prop2", "en-US"));
			Assert.IsEmpty (desc.Properties.GetPropertyValue ("prop2", "en_US"));
			Assert.IsEmpty (desc.Properties.GetPropertyValue ("prop2", null));
			
			desc.Properties.SetPropertyValue ("prop2", null, "val4");
			Assert.Equals ("val2", desc.Properties.GetPropertyValue ("prop2"));
			Assert.Equals ("val2", desc.Properties.GetPropertyValue ("prop2", "ca"));
			Assert.Equals ("val3", desc.Properties.GetPropertyValue ("prop2", "ca-ES"));
			Assert.Equals ("val3", desc.Properties.GetPropertyValue ("prop2", "ca_ES"));
			Assert.Equals ("val2", desc.Properties.GetPropertyValue ("prop2", "ca-AN"));
			Assert.Equals ("val2", desc.Properties.GetPropertyValue ("prop2", "ca_AN"));
			Assert.Equals ("val4", desc.Properties.GetPropertyValue ("prop2", "en"));
			Assert.Equals ("val4", desc.Properties.GetPropertyValue ("prop2", "en-US"));
			Assert.Equals ("val4", desc.Properties.GetPropertyValue ("prop2", "en_US"));
			Assert.Equals ("val4", desc.Properties.GetPropertyValue ("prop2", null));
		}
		
		[Test]
		public void PropertiesFromAddin ()
		{
			Addin ad = AddinManager.Registry.GetAddin ("SimpleApp.Core");
			
			Assert.Equals ("Una aplicació simple", ad.Name);
			Assert.Equals ("A simple application", ad.Properties.GetPropertyValue ("Name","en-US"));
			Assert.Equals ("SimpleApp description", ad.Description);
			Assert.Equals ("Lluis Sanchez", ad.Description.Author);
			Assert.Equals ("GPL", ad.Description.Copyright);
			Assert.Equals ("Val1", ad.Properties.GetPropertyValue ("Prop1","en-US"));
			Assert.Equals ("Val1Cat", ad.Properties.GetPropertyValue ("Prop1"));
			Assert.Equals ("Val1", ad.Properties.GetPropertyValue ("Prop2","en-US"));
			Assert.Equals ("Val2", ad.Properties.GetPropertyValue ("Prop2"));
		}
	}
}

