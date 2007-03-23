
using System;
using System.Xml;
using System.Collections.Specialized;
using Mono.Addins.Serialization;

namespace Mono.Addins.Description
{
	public class Extension: ObjectDescription, IComparable
	{
		string path;
		ExtensionNodeDescriptionCollection nodes;
		
		public Extension ()
		{
		}
		
		public Extension (string path)
		{
			this.path = path;
		}
		
		internal override void Verify (string location, StringCollection errors)
		{
			VerifyNotEmpty (location + "Extension", errors, path, "path");
			ExtensionNodes.Verify (location + "Extension (" + path + ")/", errors);
			
			foreach (ExtensionNodeDescription cnode in ExtensionNodes)
				VerifyNode (location, cnode, errors);
		}
		
		void VerifyNode (string location, ExtensionNodeDescription node, StringCollection errors)
		{
			string id = node.GetAttribute ("id");
			if (id.Length > 0)
				id = "(" + id + ")";
			if (node.NodeName == "Condition" && node.GetAttribute ("id").Length == 0) {
				errors.Add (location + node.NodeName + id + ": Missing 'id' attribute in Condition element.");
			}
			if (node.NodeName == "ComplexCondition") {
				if (node.ChildNodes.Count > 0) {
					VerifyConditionNode (location, node.ChildNodes[0], errors);
					for (int n=1; n<node.ChildNodes.Count; n++)
						VerifyNode (location + node.NodeName + id + "/", node.ChildNodes[n], errors);
				}
				else
					errors.Add (location + "ComplexCondition: Missing child condition in ComplexCondition element.");
			}
			foreach (ExtensionNodeDescription cnode in node.ChildNodes)
				VerifyNode (location + node.NodeName + id + "/", cnode, errors);
		}
		
		void VerifyConditionNode (string location, ExtensionNodeDescription node, StringCollection errors)
		{
			string nodeName = node.NodeName;
			if (nodeName != "Or" && nodeName != "And" && nodeName != "Condition") {
				errors.Add (location + "ComplexCondition: Invalid condition element: " + nodeName);
				return;
			}
			foreach (ExtensionNodeDescription cnode in node.ChildNodes)
				VerifyConditionNode (location, cnode, errors);
		}
		
		public Extension (XmlElement element)
		{
			Element = element;
			path = element.GetAttribute ("path");
		}
		
		public string Path {
			get { return path; }
			set { path = value; }
		}
		
		internal override void SaveXml (XmlElement parent)
		{
			if (Element == null) {
				Element = parent.OwnerDocument.CreateElement ("Extension");
				parent.AppendChild (Element);
			}
			Element.SetAttribute ("path", path);
			if (nodes != null)
				nodes.SaveXml (Element);
		}
		
		public ExtensionNodeDescriptionCollection ExtensionNodes {
			get {
				if (nodes == null) {
					nodes = new ExtensionNodeDescriptionCollection ();
					if (Element != null) {
						foreach (XmlNode node in Element.ChildNodes) {
							XmlElement e = node as XmlElement;
							if (e != null)
								nodes.Add (new ExtensionNodeDescription (e));
						}
					}
				}
				return nodes;
			}
		}
		
		int IComparable.CompareTo (object obj)
		{
			Extension other = (Extension) obj;
			return Path.CompareTo (other.Path);
		}
		
		internal override void Write (BinaryXmlWriter writer)
		{
			writer.WriteValue ("path", path);
			writer.WriteValue ("Nodes", ExtensionNodes);
		}
		
		internal override void Read (BinaryXmlReader reader)
		{
			path = reader.ReadStringValue ("path");
			nodes = (ExtensionNodeDescriptionCollection) reader.ReadValue ("Nodes", new ExtensionNodeDescriptionCollection ());
		}
	}
}
