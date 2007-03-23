
using System;
using System.Collections;

namespace Mono.Addins.Description
{
	public class ExtensionNodeDescriptionCollection: ObjectDescriptionCollection
	{
		public ExtensionNodeDescription this [int n] {
			get { return (ExtensionNodeDescription) List [n]; }
		}
	}
}
