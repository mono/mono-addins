
using System;

namespace Mono.Addins
{
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple=true)]
	public class TypeExtensionPointAttribute: Attribute
	{
		string path;
		string nodeName;
		Type nodeType;
		string desc;
		string name;
		
		public TypeExtensionPointAttribute ()
		{
		}
		
		public TypeExtensionPointAttribute (string path)
		{
			this.path = path;
		}
		
		public string Path {
			get { return path != null ? path : string.Empty; }
			set { path = value; }
		}
		
		public string Description {
			get { return desc != null ? desc : string.Empty; }
			set { desc = value; }
		}
		
		public string NodeName {
			get { return nodeName != null && nodeName.Length > 0 ? nodeName : "Type"; }
			set { nodeName = value; }
		}
		
		public string Name {
			get { return name != null ? name : string.Empty; }
			set { name = value; }
		}

		public Type NodeType {
			get { return nodeType != null ? nodeType : typeof(TypeExtensionNode); }
			set { nodeType = value; }
		}
}
}
