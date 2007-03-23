
using System;

namespace Mono.Addins
{
	[AttributeUsage (AttributeTargets.Class)]
	public class ExtensionNodeAttribute: Attribute
	{
		string nodeName;
		string description;
		
		public ExtensionNodeAttribute ()
		{
		}
		
		public ExtensionNodeAttribute (string nodeName)
		{
			this.nodeName = nodeName;
		}
		
		public ExtensionNodeAttribute (string nodeName, string description)
		{
			this.nodeName = nodeName;
			this.description = description;
		}
		
		public string NodeName {
			get { return nodeName != null ? nodeName : string.Empty; }
			set { nodeName = value; }
		}
		
		public string Description {
			get { return description != null ? description : string.Empty; }
			set { description = value; }
		}
	}
}
