
using System;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;

namespace Mono.Addins.Description
{
	public class ObjectDescriptionCollection: CollectionBase
	{
		public void Add (ObjectDescription ep)
		{
			List.Add (ep);
		}
		
		public void Remove (ObjectDescription ep)
		{
			List.Remove (ep);
		}
		
		public bool Contains (ObjectDescription ob)
		{
			return List.Contains (ob);
		}
		
		protected override void OnRemove (int index, object value)
		{
			ObjectDescription ep = (ObjectDescription) value;
			if (ep.Element != null) {
				ep.Element.ParentNode.RemoveChild (ep.Element);
				ep.Element = null;
			}
		}
		
		internal void SaveXml (XmlElement parent)
		{
			foreach (ObjectDescription ob in this)
				ob.SaveXml (parent);
		}
		
		internal void Verify (string location, StringCollection errors)
		{
			int n=0;
			foreach (ObjectDescription ob in this) {
				ob.Verify (location + "[" + n + "]/", errors);
				n++;
			}
		}
	}
}
