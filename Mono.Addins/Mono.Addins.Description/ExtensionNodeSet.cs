
using System;
using System.Collections;
using System.Xml;
using Mono.Addins.Serialization;
using System.Collections.Specialized;

namespace Mono.Addins.Description
{
	public class ExtensionNodeSet: ObjectDescription
	{
		string id;
		ExtensionNodeSetCollection nodeTypes;
		NodeSetIdCollection nodeSets;
		bool missingNodeSetId;
		
		internal ExtensionNodeSet (XmlElement element)
		{
			Element = element;
			id = element.GetAttribute (IdAttribute);
		}
		
		internal override void Verify (string location, StringCollection errors)
		{
			if (missingNodeSetId)
				errors.Add (location + "Missing id attribute in extension set reference");
			
			NodeTypes.Verify (location + "ExtensionNodeSet (" + Id + ")/", errors);
		}
		
		internal override void SaveXml (XmlElement parent)
		{
			SaveXml (parent, "ExtensionNodeSet");
		}
		
		internal virtual void SaveXml (XmlElement parent, string nodeName)
		{
			if (Element == null) {
				Element = parent.OwnerDocument.CreateElement (nodeName);
				parent.AppendChild (Element);
			}
			if (Id.Length > 0)
				Element.SetAttribute (IdAttribute, Id);
			if (nodeTypes != null)
				nodeTypes.SaveXml (Element);
			if (nodeSets != null) {
				foreach (string s in nodeSets) {
					if (Element.SelectSingleNode ("ExtensionNodeSet[@id='" + s + "']") == null) {
						XmlElement e = Element.OwnerDocument.CreateElement ("ExtensionNodeSet");
						e.SetAttribute ("id", s);
						Element.AppendChild (e);
					}
				}
				ArrayList list = new ArrayList ();
				foreach (XmlElement e in Element.SelectNodes ("ExtensionNodeSet")) {
					if (!nodeSets.Contains (e.GetAttribute ("id")))
						list.Add (e);
				}
				foreach (XmlElement e in list)
					Element.RemoveChild (e);
			}
		}
		
		public ExtensionNodeSet ()
		{
		}
		
		public string Id {
			get { return id != null ? id : string.Empty; }
			set { id = value; }
		}
			
		internal virtual string IdAttribute {
			get { return "id"; }
		}
		
		public ExtensionNodeSetCollection NodeTypes {
			get {
				if (nodeTypes == null) {
					if (Element != null)
						InitCollections ();
					else
						nodeTypes = new ExtensionNodeSetCollection ();
				}
				return nodeTypes;
			}
		}
		
		public NodeSetIdCollection NodeSets {
			get {
				if (nodeSets == null) {
					if (Element != null)
						InitCollections ();
					else
						nodeSets = new NodeSetIdCollection ();
				}
				return nodeSets;
			}
		}
		
		internal void Clear ()
		{
			Element = null;
			nodeSets = null;
			nodeTypes = null;
		}
		
		internal void SetExtensionsAddinId (string addinId)
		{
			foreach (ExtensionNodeType nt in NodeTypes) {
				nt.AddinId = addinId;
				nt.SetExtensionsAddinId (addinId);
			}
			NodeSets.SetExtensionsAddinId (addinId);
		}
		
		internal void MergeWith (string thisAddinId, ExtensionNodeSet other)
		{
			foreach (ExtensionNodeType nt in other.NodeTypes) {
				if (nt.AddinId != thisAddinId && !NodeTypes.Contains (nt))
					NodeTypes.Add (nt);
			}
			NodeSets.MergeWith (thisAddinId, other.NodeSets);
		}
		
		internal void UnmergeExternalData (string thisAddinId, Hashtable addinsToUnmerge)
		{
			// Removes extension types and extension sets coming from other add-ins.
			
			ArrayList todelete = new ArrayList ();
			foreach (ExtensionNodeType nt in NodeTypes) {
				if (nt.AddinId != thisAddinId && (addinsToUnmerge == null || addinsToUnmerge.Contains (nt.AddinId)))
					todelete.Add (nt);
			}
			foreach (ExtensionNodeType nt in todelete)
				NodeTypes.Remove (nt);
			
			NodeSets.UnmergeExternalData (thisAddinId, addinsToUnmerge);
		}
		
		void InitCollections ()
		{
			nodeTypes = new ExtensionNodeSetCollection ();
			nodeSets = new NodeSetIdCollection ();
			
			foreach (XmlNode n in Element.ChildNodes) {
				XmlElement nt = n as XmlElement;
				if (nt == null)
					continue;
				if (nt.LocalName == "ExtensionNode") {
					ExtensionNodeType etype = new ExtensionNodeType (nt);
					nodeTypes.Add (etype);
				}
				else if (nt.LocalName == "ExtensionNodeSet") {
					string id = nt.GetAttribute ("id");
					if (id.Length > 0)
						nodeSets.Add (id);
					else
						missingNodeSetId = true;
				}
			}
		}
		
		internal override void Write (BinaryXmlWriter writer)
		{
			writer.WriteValue ("Id", id);
			writer.WriteValue ("NodeTypes", NodeTypes);
			writer.WriteValue ("NodeSets", NodeSets.InternalList);
		}
		
		internal override void Read (BinaryXmlReader reader)
		{
			id = reader.ReadStringValue ("Id");
			nodeTypes = (ExtensionNodeSetCollection) reader.ReadValue ("NodeTypes", new ExtensionNodeSetCollection ());
			reader.ReadValue ("NodeSets", NodeSets.InternalList);
		}
	}
	
	public class NodeSetIdCollection: IEnumerable
	{
		// A list of string[2]. Item 0 is the node set id, item 1 is the addin that defines it.
		
		ArrayList list = new ArrayList ();
		
		public string this [int n] {
			get { return (string) list [n]; }
		}
		
		public int Count {
			get { return list.Count; }
		}
		
		public IEnumerator GetEnumerator ()
		{
			foreach (string[] ns in list)
				yield return ns [0];
		}
		
		public void Add (string nodeSetId)
		{
			if (!Contains (nodeSetId))
				list.Add (new string [] { nodeSetId, null });
		}
		
		public void Remove (string nodeSetId)
		{
			int i = IndexOf (nodeSetId);
			if (i != -1)
				list.RemoveAt (i);
		}
		
		public bool Contains (string nodeSetId)
		{
			return IndexOf (nodeSetId) != -1;
		}

		public int IndexOf (string nodeSetId)
		{
			for (int n=0; n<list.Count; n++)
				if (((string[])list [n])[0] == nodeSetId)
					return n;
			return -1;
		}
		
		internal void SetExtensionsAddinId (string id)
		{
			foreach (string[] ns in list)
				ns [1] = id;
		}
		
		internal ArrayList InternalList {
			get { return list; }
			set { list = value; }
		}
		
		internal void MergeWith (string thisAddinId, NodeSetIdCollection other)
		{
			foreach (string[] ns in other.list) {
				if (ns [1] != thisAddinId && !list.Contains (ns))
					list.Add (ns);
			}
		}
		
		internal void UnmergeExternalData (string thisAddinId, Hashtable addinsToUnmerge)
		{
			ArrayList newList = new ArrayList ();
			foreach (string[] ns in list) {
				if (ns[1] == thisAddinId || (addinsToUnmerge != null && !addinsToUnmerge.Contains (ns[1])))
					newList.Add (ns);
			}
			list = newList;
		}
	}
}
