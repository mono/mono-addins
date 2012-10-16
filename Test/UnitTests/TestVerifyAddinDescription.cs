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
using System.Linq;
using NUnit.Framework;
using Mono.Addins;
using Mono.Addins.Description;
using System.IO;

namespace UnitTests
{
	public abstract class TestVerifyAddinDescription
	{
		protected AddinDescription desc;
		
		[Test]
		public void Header ()
		{
			Assert.AreEqual ("SimpleApp.Core,0.1.0", desc.AddinId);
			Assert.AreEqual ("Core", desc.LocalId);
			Assert.AreEqual ("0.1.0", desc.Version);
			Assert.AreEqual ("LSG", desc.Author);
			Assert.AreEqual ("SomeCategory", desc.Category);
			Assert.AreEqual ("0.0.1", desc.CompatVersion);
			Assert.AreEqual ("GPL", desc.Copyright);
			Assert.AreEqual ("Long description", desc.Description);
			Assert.AreEqual (false, desc.EnabledByDefault);
			Assert.AreEqual (AddinFlags.CantDisable | AddinFlags.CantUninstall | AddinFlags.Hidden, desc.Flags);
			Assert.AreEqual (true, desc.IsRoot);
			Assert.AreEqual ("A simple application", desc.Name);
			Assert.AreEqual ("SimpleApp", desc.Namespace);
			Assert.AreEqual ("http://somewhere.com", desc.Url);
		}
		
		[Test]
		public void Properties ()
		{
			Assert.AreEqual (3, desc.Properties.Count ());
			Assert.AreEqual ("TestProp1", desc.Properties.GetPropertyValue ("TestProperty1"));
			Assert.AreEqual ("TestProp2", desc.Properties.GetPropertyValue ("TestProperty2", "ll1"));
			Assert.AreEqual ("TestProp3", desc.Properties.GetPropertyValue ("TestProperty2", "ll2"));
		}
		
		[Test]
		public void Flags ()
		{
			Assert.IsFalse (desc.CanDisable);
			Assert.IsFalse (desc.CanUninstall);
			Assert.IsTrue (desc.IsHidden);
		}
		
		[Test]
		public void Files ()
		{
			Assert.AreEqual (6, desc.AllFiles.Count);
			Assert.IsTrue (desc.AllFiles.Contains ("UnitTests1.dll"));
			Assert.IsTrue (desc.AllFiles.Contains ("UnitTests2.dll"));
			Assert.IsTrue (desc.AllFiles.Contains ("UnitTestsModule.dll"));
			Assert.IsTrue (desc.AllFiles.Contains ("File1"));
			Assert.IsTrue (desc.AllFiles.Contains ("File2"));
			Assert.IsTrue (desc.AllFiles.Contains ("FileModule"));
			
			Assert.AreEqual (2, desc.MainModule.DataFiles.Count);
			Assert.IsTrue (desc.AllFiles.Contains ("File1"));
			Assert.IsTrue (desc.AllFiles.Contains ("File2"));
			
			Assert.AreEqual (2, desc.MainModule.Assemblies.Count);
			Assert.IsTrue (desc.AllFiles.Contains ("UnitTests1.dll"));
			Assert.IsTrue (desc.AllFiles.Contains ("UnitTests2.dll"));
		}
		
		[Test]
		public void Dependencies ()
		{
			Assert.AreEqual (4, desc.MainModule.Dependencies.Count);
			Assert.IsInstanceOfType (typeof(AddinDependency), desc.MainModule.Dependencies[0]);
			Assert.IsInstanceOfType (typeof(AddinDependency), desc.MainModule.Dependencies[1]);
			Assert.IsInstanceOfType (typeof(AddinDependency), desc.MainModule.Dependencies[2]);
			Assert.IsInstanceOfType (typeof(AddinDependency), desc.MainModule.Dependencies[3]);
			Assert.AreEqual ("SimpleApp.Dep1,1.0", ((AddinDependency)desc.MainModule.Dependencies[0]).FullAddinId);
			Assert.AreEqual ("SimpleApp.Dep2,2.0", ((AddinDependency)desc.MainModule.Dependencies[1]).FullAddinId);
			Assert.AreEqual ("SimpleApp.Other.Dep3,3.0", ((AddinDependency)desc.MainModule.Dependencies[2]).FullAddinId);
			Assert.AreEqual ("Other.Dep4,4.0", ((AddinDependency)desc.MainModule.Dependencies[3]).FullAddinId);
		}
		
		[Test]
		public void Conditions ()
		{
			Assert.AreEqual (2, desc.ConditionTypes.Count);
			Assert.AreEqual ("TestCondition1", desc.ConditionTypes[0].Id);
			Assert.AreEqual ("SimpleApp.TestCondition", desc.ConditionTypes[0].TypeName);
			Assert.AreEqual ("Test condition description", desc.ConditionTypes[0].Description);
			Assert.AreEqual ("TestCondition2", desc.ConditionTypes[1].Id);
			Assert.AreEqual ("SimpleApp.TestCondition2", desc.ConditionTypes[1].TypeName);
			Assert.AreEqual ("Test condition description 2", desc.ConditionTypes[1].Description);
		}
		
		[Test]
		public void ExtensionPoints ()
		{
			Assert.AreEqual (2, desc.ExtensionPoints.Count);
			ExtensionPoint ep1 = desc.ExtensionPoints [0];
			Assert.AreEqual ("/SimpleApp/TestEP1", ep1.Path);
			Assert.AreEqual ("TestEP1", ep1.Name);
			Assert.AreEqual ("Test EP1.", ep1.Description);
			Assert.AreEqual (0, ep1.NodeSet.NodeTypes.Count);
			Assert.AreEqual (1, ep1.NodeSet.NodeSets.Count);
			Assert.AreEqual ("TestSet", ep1.NodeSet.NodeSets[0]);
			Assert.AreEqual (1, ep1.Conditions.Count);
			Assert.AreEqual ("TestCondition1", ep1.Conditions[0].Id);
			
			ExtensionPoint ep2 = desc.ExtensionPoints [1];
			Assert.AreEqual ("/SimpleApp/TestEP2", ep2.Path);
			Assert.AreEqual ("TestEP2", ep2.Name);
			Assert.AreEqual ("Test EP2.", ep2.Description);
			
			Assert.AreEqual (1, ep2.NodeSet.NodeTypes.Count);
			ExtensionNodeType nt = ep2.NodeSet.NodeTypes[0];
			Assert.AreEqual ("Node", nt.NodeName);
			Assert.AreEqual ("Node description", nt.Description);
			Assert.AreEqual (0, nt.NodeSets.Count);
			Assert.AreEqual (1, nt.NodeTypes.Count);
			nt = nt.NodeTypes [0];
			Assert.AreEqual ("Child", nt.NodeName);
			Assert.AreEqual ("SomeNodeType", nt.TypeName);
			Assert.AreEqual ("SomeObjectType", nt.ObjectTypeName);
			Assert.AreEqual ("SomeCustomAttrType", nt.ExtensionAttributeTypeName);
			Assert.AreEqual ("Child description", nt.Description);
		}
			
		[Test]
		public void Extensions ()
		{
			Assert.AreEqual (2, desc.MainModule.Extensions.Count);
			Extension ex = desc.MainModule.Extensions [0];
			Assert.AreEqual ("/SimpleApp/TestEP2", ex.Path);
			Assert.AreEqual (1, ex.ExtensionNodes.Count);
			var nod = ex.ExtensionNodes[0];
			Assert.AreEqual ("node1", nod.Id);
			Assert.AreEqual ("test", nod.GetAttribute ("type"));
			Assert.AreEqual (2, nod.ChildNodes.Count);
			Assert.AreEqual ("child1", nod.ChildNodes [0].Id);
			Assert.AreEqual ("test1", nod.ChildNodes [0].GetAttribute ("type"));
			Assert.AreEqual ("child2", nod.ChildNodes [1].Id);
			Assert.AreEqual ("test2", nod.ChildNodes [1].GetAttribute ("type"));
			
			ex = desc.MainModule.Extensions [1];
			Assert.AreEqual ("/SimpleApp/TestEP1", ex.Path);
			Assert.AreEqual (0, ex.ExtensionNodes.Count);
		}
		
		[Test]
		public void Modules ()
		{
			Assert.AreEqual (3, desc.AllModules.Count);
			Assert.AreEqual (2, desc.OptionalModules.Count);
		}
		
		[Test]
		public void ModuleFiles ()
		{
			ModuleDescription mod = desc.OptionalModules [0];
			Assert.AreEqual (2, mod.AllFiles.Count);
			Assert.IsTrue (mod.AllFiles.Contains ("UnitTestsModule.dll"));
			Assert.IsTrue (mod.AllFiles.Contains ("FileModule"));
			
			Assert.AreEqual (1, mod.DataFiles.Count);
			Assert.IsTrue (mod.DataFiles.Contains ("FileModule"));
			
			Assert.AreEqual (1, mod.Assemblies.Count);
			Assert.IsTrue (mod.Assemblies.Contains ("UnitTestsModule.dll"));
		}
		
		[Test]
		public void ModuleDependencies ()
		{
			ModuleDescription mod = desc.OptionalModules [0];
			Assert.AreEqual (1, mod.Dependencies.Count);
			Assert.IsInstanceOfType (typeof(AddinDependency), mod.Dependencies[0]);
			Assert.AreEqual ("SimpleApp.Dep1,1.0", ((AddinDependency)mod.Dependencies[0]).FullAddinId);
		}
		
		[Test]
		public void ModuleExtensions ()
		{
			Assert.AreEqual (2, desc.MainModule.Extensions.Count);
			Extension ex = desc.MainModule.Extensions [0];
			Assert.AreEqual ("/SimpleApp/TestEP2", ex.Path);
			Assert.AreEqual (1, ex.ExtensionNodes.Count);
			var nod = ex.ExtensionNodes[0];
			Assert.AreEqual ("node1", nod.Id);
			Assert.AreEqual ("test", nod.GetAttribute ("type"));
			Assert.AreEqual (2, nod.ChildNodes.Count);
			Assert.AreEqual ("child1", nod.ChildNodes [0].Id);
			Assert.AreEqual ("test1", nod.ChildNodes [0].GetAttribute ("type"));
			Assert.AreEqual ("child2", nod.ChildNodes [1].Id);
			Assert.AreEqual ("test2", nod.ChildNodes [1].GetAttribute ("type"));
		}
	}
}

