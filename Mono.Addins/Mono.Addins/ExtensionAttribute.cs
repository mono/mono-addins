
using System;

namespace Mono.Addins
{
	[AttributeUsage (AttributeTargets.Class, AllowMultiple=true)]
	public class ExtensionAttribute: Attribute
	{
		string path;
		string nodeName;
		string id;
		string insertBefore;
		string insertAfter;
		
		public ExtensionAttribute ()
		{
		}
		
		public ExtensionAttribute (string path)
		{
			this.path = path;
		}
		
		public string Path {
			get { return path != null ? path : string.Empty; }
			set { path = value; }
		}
		
		public string NodeName {
			get { return nodeName != null && nodeName.Length > 0 ? nodeName : "Type"; }
			set { nodeName = value; }
		}
		
		public string Id {
			get { return id != null ? id : string.Empty; }
			set { id = value; }
		}
		
		public string InsertBefore {
			get { return insertBefore != null ? insertBefore : string.Empty; }
			set { insertBefore = value; }
		}
		
		public string InsertAfter {
			get { return insertAfter != null ? insertAfter : string.Empty; }
			set { insertAfter = value; }
		}
	}
}
