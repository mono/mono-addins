
using System;
using System.Collections.Specialized;
using System.Xml;
using Mono.Addins.Serialization;

namespace Mono.Addins.Description
{
	public class ObjectDescription: IBinaryXmlElement
	{
		internal XmlElement Element;
		
		internal ObjectDescription (XmlElement elem)
		{
			Element = elem;
		}
		
		internal ObjectDescription ()
		{
		}
		
		void IBinaryXmlElement.Write (BinaryXmlWriter writer)
		{
			Write (writer);
		}
		
		void IBinaryXmlElement.Read (BinaryXmlReader reader)
		{
			Read (reader);
		}
		
		internal virtual void Write (BinaryXmlWriter writer)
		{
		}
		
		internal virtual void Read (BinaryXmlReader reader)
		{
		}
		
		internal virtual void SaveXml (XmlElement parent)
		{
		}
		
		internal void CreateElement (XmlElement parent, string nodeName)
		{
			if (Element == null) {
				Element = parent.OwnerDocument.CreateElement (nodeName); 
				parent.AppendChild (Element);
			}
		}
		
		internal string ReadXmlDescription ()
		{
			XmlElement de = Element ["Description"];
			if (de != null)
				return de.InnerText;
			else
				return null;
		}
		
		internal void SaveXmlDescription (string desc)
		{
			XmlElement de = Element ["Description"];
			if (desc != null && desc.Length > 0) {
				if (de == null) {
					de = Element.OwnerDocument.CreateElement ("Description");
					Element.AppendChild (de);
				}
				de.InnerText = desc;
			} else {
				if (de != null)
					Element.RemoveChild (de);
			}
		}
		
		internal virtual void Verify (string location, StringCollection errors)
		{
		}
		
		internal void VerifyNotEmpty (string location, StringCollection errors, string attr, string val)
		{
			if (val == null || val.Length == 0)
				errors.Add (location + ": attribute '" + attr + "' can't be empty.");
		}
	}
}
