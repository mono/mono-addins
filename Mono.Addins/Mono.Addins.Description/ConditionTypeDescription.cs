
using System;
using System.Xml;
using System.Collections.Specialized;
using Mono.Addins.Serialization;

namespace Mono.Addins.Description
{
	public class ConditionTypeDescription: ObjectDescription
	{
		string id;
		string typeName;
		string addinId;
		string description;
		
		public ConditionTypeDescription ()
		{
		}
		
		internal ConditionTypeDescription (XmlElement elem): base (elem)
		{
			id = elem.GetAttribute ("id");
			typeName = elem.GetAttribute ("type");
			description = ReadXmlDescription ();
		}
		
		internal override void Verify (string location, StringCollection errors)
		{
			VerifyNotEmpty (location + "ConditionType", errors, Id, "id");
			VerifyNotEmpty (location + "ConditionType (" + Id + ")", errors, TypeName, "type");
		}
		
		public string Id {
			get { return id != null ? id : string.Empty; }
			set { id = value; }
		}
		
		public string TypeName {
			get { return typeName != null ? typeName : string.Empty; }
			set { typeName = value; }
		}
		
		public string Description {
			get { return description != null ? description : string.Empty; }
			set { description = value; }
		}
		
		internal string AddinId {
			get { return addinId; }
			set { addinId = value; }
		}
		
		internal override void SaveXml (XmlElement parent)
		{
			CreateElement (parent, "ConditionType");
			Element.SetAttribute ("id", id);
			Element.SetAttribute ("type", typeName);
			SaveXmlDescription (description);
		}
		
		internal override void Write (BinaryXmlWriter writer)
		{
			writer.WriteValue ("Id", Id);
			writer.WriteValue ("TypeName", TypeName);
			writer.WriteValue ("Description", Description);
			writer.WriteValue ("AddinId", AddinId);
		}
		
		internal override void Read (BinaryXmlReader reader)
		{
			Id = reader.ReadStringValue ("Id");
			TypeName = reader.ReadStringValue ("TypeName");
			Description = reader.ReadStringValue ("Description");
			AddinId = reader.ReadStringValue ("AddinId");
		}
	}
}
