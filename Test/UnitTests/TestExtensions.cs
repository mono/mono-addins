
using System;
using System.IO;
using NUnit.Framework;
using Mono.Addins;
using SimpleApp;

namespace UnitTests
{
	[TestFixture()]
	public class TestExtensions: TestBase
	{
		[Test()]
		public void TestGetExtensionNodes ()
		{
			ExtensionNodeList writers = AddinManager.GetExtensionNodes ("/SimpleApp/Writers");
			Assert.AreEqual (4, writers.Count, "Node count");
			
			foreach (ExtensionNode n in writers) {
				TypeExtensionNode tn = n as TypeExtensionNode;
				Assert.IsNotNull (tn, "Not TypeExtensionNode " + n.GetType ());
				Assert.IsNotNull (tn.CreateInstance (typeof(IWriter)));
			}
			
			Assert.IsTrue (AddinManager.IsAddinLoaded ("SimpleApp.HelloWorldExtension"), "HelloWorldExtension loaded");
		}
		
		[Test()]
		public void TestGetExtensionNode ()
		{
			ExtensionNode writer = AddinManager.GetExtensionNode ("/SimpleApp/Writers/HelloWorldExtension.HelloWorldWriter");
			Assert.IsNotNull (writer, "t1");
			Assert.IsTrue (writer is TypeExtensionNode, "t2");
			
			Assert.AreEqual (0, writer.ChildNodes.Count, "t3");
			Assert.IsNotNull (writer.Addin, "t3");
			Assert.AreEqual ("SimpleApp.HelloWorldExtension", writer.Addin.Id, "t4");
			Assert.IsTrue (writer.HasId, "t5");
			Assert.AreEqual ("HelloWorldExtension.HelloWorldWriter", writer.Id, "t6");
			Assert.AreEqual ("/SimpleApp/Writers/HelloWorldExtension.HelloWorldWriter", writer.Path, "t7");
			
			TypeExtensionNode tn = (TypeExtensionNode) writer;
			object ob = tn.CreateInstance () as IWriter;
			Assert.IsNotNull (ob, "t8");
			ob = tn.CreateInstance (typeof(IWriter)) as IWriter;
			Assert.IsNotNull (ob, "t9");
			
			try {
				ob = tn.CreateInstance (typeof(string)) as IWriter;
			} catch (InvalidOperationException) {
			}
		}
		
		[Test()]
		public void TestGetExtensionNodeChildren ()
		{
			ExtensionNode writer = AddinManager.GetExtensionNode ("/SimpleApp/Writers/SomeFile");
			Assert.IsNotNull (writer, "t1");
			Assert.IsTrue (writer is TypeExtensionNode, "t2");
			
			Assert.AreEqual (3, writer.ChildNodes.Count, "t3");
			
			TypeExtensionNode ch = writer.ChildNodes[0] as TypeExtensionNode;
			Assert.IsNotNull (ch, "t4 child 0");
			Assert.AreEqual ("root", ch.CreateInstance (), "t4 child val 0");
			
			ch = writer.ChildNodes[1] as TypeExtensionNode;
			Assert.IsNotNull (ch, "t4 child 1");
			Assert.AreEqual ("some/path1", ch.CreateInstance (), "t4 child val 1");
			
			ch = writer.ChildNodes[2] as TypeExtensionNode;
			Assert.IsNotNull (ch, "t4 child 2");
			Assert.AreEqual ("some/path2", ch.CreateInstance (), "t4 child val 2");
		}
		
		[Test()]
		public void TestGetUnknownExtensionNode ()
		{
			ExtensionNode node = AddinManager.GetExtensionNode ("/SimpleApp/Writers/Unknown");
			Assert.IsNull (node, "t1");
			
			node = AddinManager.GetExtensionNode ("/SimpleApp/Writers/SomeFile/Unknown");
			Assert.IsNull (node, "t2");
		}
		
		[Test()]
		public void TestGetExtensionPointRootNode ()
		{
			ExtensionNode node = AddinManager.GetExtensionNode ("/SimpleApp/Writers");
			Assert.IsNotNull (node, "root node is null");
			Assert.AreEqual (4, node.ChildNodes.Count, "Node count");
		}
		
		[Test()]
		public void TestGetUnknownOiitNode ()
		{
			ExtensionNode node = AddinManager.GetExtensionNode ("/Unknown");
			Assert.IsNull (node, "t1");
		}
		
		[Test()]
		public void TestGetNodeNotExtension ()
		{
			ExtensionNode node = AddinManager.GetExtensionNode ("/SimpleApp");
			Assert.IsNull (node, "node not null");
		}
		
		[Test()]
		public void TestTypeExtension ()
		{
			ISampleExtender[] objects = (ISampleExtender[]) AddinManager.GetExtensionObjects (typeof(ISampleExtender));
			Assert.AreEqual (2, objects.Length, "Node count");
			if (objects[0].Text.StartsWith ("F")) {
				Assert.AreEqual ("FileSampleExtender", objects[0].Text, "t1");
				Assert.AreEqual ("HelloSampleExtender", objects[1].Text, "t2");
			}
			else {
				Assert.AreEqual ("HelloSampleExtender", objects[0].Text, "t1.1");
				Assert.AreEqual ("FileSampleExtender", objects[1].Text, "t2.1");
			}
		}
		
		[Test()]
		public void TestExtensionWithChildren ()
		{
			ExtensionNodeList nodes = AddinManager.GetExtensionNodes ("/SimpleApp/NodeWithChildren");
			Assert.AreEqual (2, nodes.Count, "Node count");
			ExtensionNode n1 = nodes [0];
			ExtensionNode n2 = nodes [1];
			Assert.AreEqual ("node1", n1.Id, "t1");
			Assert.AreEqual (3, n1.ChildNodes.Count, "n1 node count");
			Assert.AreEqual ("child1", n1.ChildNodes[0].Id, "t1.1");
			Assert.AreEqual ("child1.1", n1.ChildNodes[1].Id, "t1.2");
			Assert.AreEqual ("child2", n1.ChildNodes[2].Id, "t1.3");
			
			Assert.AreEqual ("node2", n2.Id, "t2");
		}
		
		[Test()]
		public void TestExtensionWithAttribute ()
		{
			ExtensionNodeList nodes = AddinManager.GetExtensionNodes ("/SimpleApp/NodesWithAttribute");
			Assert.AreEqual (2, nodes.Count, "Node count");
			NodeWithAttribute n1 = nodes [0] as NodeWithAttribute;
			NodeWithAttribute n2 = nodes [1] as NodeWithAttribute;
			Assert.IsNotNull (n1);
			Assert.IsNotNull (n2);
			Assert.AreEqual ("test1", n1.Name, "t1");
			Assert.AreEqual (true, n1.Value, "t2");
			Assert.AreEqual ("test2", n2.Name, "t1");
			Assert.AreEqual (true, n2.Value, "t2");
		}
		
		[Test()]
		public void TestTypeExtensionWithAttribute ()
		{
			ExtensionNodeList nodes = AddinManager.GetExtensionNodes (typeof(IWriterWithMetadata));
			Assert.AreEqual (2, nodes.Count, "Node count");
			TypeExtensionNode<WriterWithMetadataAttribute> n1 = nodes [0] as TypeExtensionNode<WriterWithMetadataAttribute>;
			TypeExtensionNode<WriterWithMetadataAttribute> n2 = nodes [1] as TypeExtensionNode<WriterWithMetadataAttribute>;
			Assert.IsNotNull (n1);
			Assert.IsNotNull (n2);
			Assert.AreEqual ("meta1", n1.Data.Name, "t1");
			Assert.AreEqual (1, n1.Data.Version, "t2");
			IWriterWithMetadata md = (IWriterWithMetadata) n1.CreateInstance ();
			Assert.AreEqual ("mt1", md.Write ());
			Assert.AreEqual ("meta2", n2.Data.Name, "t3");
			Assert.AreEqual (2, n2.Data.Version, "t4");
			md = (IWriterWithMetadata) n2.CreateInstance ();
			Assert.AreEqual ("mt2", md.Write ());
		}
		
		[Test()]
		public void TestDataExtensionWithAttribute ()
		{
			ExtensionNodeList nodes = AddinManager.GetExtensionNodes ("/SimpleApp/DataExtensionWithAttribute");
			Assert.AreEqual (2, nodes.Count, "Node count");
			ExtensionNode<SimpleExtensionAttribute> n1 = nodes [0] as ExtensionNode<SimpleExtensionAttribute>;
			ExtensionNode<SimpleExtensionAttribute> n2 = nodes [1] as ExtensionNode<SimpleExtensionAttribute>;
			Assert.IsNotNull (n1);
			Assert.IsNotNull (n2);
			Assert.AreEqual ("test3", n1.Data.Name, "t1");
			Assert.AreEqual (true, n1.Data.Value, "t2");
			Assert.AreEqual ("test4", n2.Data.Name, "t1");
			Assert.AreEqual (false, n2.Data.Value, "t2");
		}
		
		[Test()]
		public void TestAttrExtensionWithManyNodes ()
		{
			ExtensionNodeList nodes = AddinManager.GetExtensionNodes ("/SimpleApp/AttrExtensionWithManyNodes");
			Assert.AreEqual (2, nodes.Count, "Node count");
			Assert.IsTrue (nodes [0] is AttrExtensionWithManyNodesExtensionNode);
			Assert.IsNotNull (nodes [1] is AttrExtensionWithManyNodesExtensionNodeExtra);
		}
		
		[Test()]
		public void TestMultiAssemblyAddin ()
		{
			ExtensionNodeList nodes = AddinManager.GetExtensionNodes ("/SimpleApp/MultiAssemblyTestExtensionPoint");
			Assert.AreEqual (6, nodes.Count, "Node count");
		}
	}
}
