
using System;

namespace Mono.Addins
{
	[AttributeUsage (AttributeTargets.Class, AllowMultiple=true)]
	public class ExtensionNodeChildAttribute: Attribute
	{
		string nodeName;
		Type extensionNodeType;
		
		public ExtensionNodeChildAttribute (string nodeName)
			: this (typeof(TypeExtensionNode), nodeName)
		{
		}
		
		public ExtensionNodeChildAttribute (Type extensionNodeType)
			: this (extensionNodeType, null)
		{
		}
		
		public ExtensionNodeChildAttribute (Type extensionNodeType, string nodeName)
		{
			this.extensionNodeType = extensionNodeType;
			this.nodeName = nodeName;
		}
		
		public string NodeName {
			get { return nodeName != null ? nodeName : string.Empty; }
			set { nodeName = value; }
		}
		
		public Type ExtensionNodeType {
			get { return extensionNodeType; }
			set { extensionNodeType = value; }
		}
	}
}
