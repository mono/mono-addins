
using System;

namespace Mono.Addins.Description
{
	public class NodeTypeAttributeCollection: ObjectDescriptionCollection
	{
		public NodeTypeAttribute this [int n] {
			get { return (NodeTypeAttribute) List [n]; }
		}
	}
}
