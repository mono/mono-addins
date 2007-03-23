
using System;

namespace Mono.Addins
{
	[AttributeUsage (AttributeTargets.Assembly, AllowMultiple=true)]
	public class ExtensionPointAttribute: Attribute
	{
		string path;
		Type nodeType;
		string nodeName;
		string desc;
		string name;
		Type objectType;
		
		public ExtensionPointAttribute ()
		{
		}
		
		public ExtensionPointAttribute (string path)
		{
			this.path = path;
		}
		
		public ExtensionPointAttribute (string path, Type nodeType)
		{
			this.path = path;
			this.nodeType = nodeType;
		}
		
		public ExtensionPointAttribute (string path, string nodeName, Type nodeType)
		{
			this.path = path;
			this.nodeType = nodeType;
			this.nodeName = nodeName;
		}
		
		public string Path {
			get { return path != null ? path : string.Empty; }
			set { path = value; }
		}
		
		public string Description {
			get { return desc != null ? desc : string.Empty; }
			set { desc = value; }
		}
		
		public Type NodeType {
			get { return nodeType != null ? nodeType : typeof(TypeExtensionNode); }
			set { nodeType = value; }
		}
		
		public Type ObjectType {
			get { return objectType; }
			set { objectType = value; }
		}
		
		public string NodeName {
			get { return nodeName != null && nodeName.Length > 0 ? nodeName : "Type"; }
			set { nodeName = value; }
		}
		
		public string Name {
			get { return name != null ? name : string.Empty; }
			set { name = value; }
		}
	}
}
