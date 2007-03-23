
using System;
using System.Xml;
using System.Collections.Specialized;
using Mono.Addins.Serialization;

namespace Mono.Addins.Description
{
	public class Condition: ObjectDescription
	{
		string id;
		string description;
		string addinId;
		
		public string Id {
			get { return id; }
			set { id = value; }
		}
		
		public string Description {
			get { return description; }
			set { description = value; }
		}
		
		internal string AddinId {
			get { return addinId; }
			set { addinId = value; }
		}
		
		internal override void Verify (string location, StringCollection errors)
		{
			VerifyNotEmpty (location + "Condition", errors, Id, "id");
		}
		
		internal Condition (XmlElement elem): base (elem)
		{
			id = elem.GetAttribute ("id");
			description = ReadXmlDescription ();
		}
		
		internal override void SaveXml (XmlElement parent)
		{
			CreateElement (parent, "Condition");
			Element.SetAttribute ("id", id);
			SaveXmlDescription (description);
		}
		
		internal override void Write (BinaryXmlWriter writer)
		{
			writer.WriteValue ("id", id);
			writer.WriteValue ("description", description);
			writer.WriteValue ("addinId", addinId);
		}
		
		internal override void Read (BinaryXmlReader reader)
		{
			id = reader.ReadStringValue ("id");
			description = reader.ReadStringValue ("description");
			addinId = reader.ReadStringValue ("addinId");
		}
	}
}
