
using System;
using System.Collections;

namespace Mono.Addins.Description
{
	public class ExtensionNodeSetCollection: ObjectDescriptionCollection
	{
		public ExtensionNodeSet this [int n] {
			get { return (ExtensionNodeSet) List [n]; }
		}
		
		public ExtensionNodeSet this [string id] {
			get {
				for (int n=0; n<List.Count; n++)
					if (((ExtensionNodeSet) List [n]).Id == id)
						return (ExtensionNodeSet) List [n];
				return null;
			}
		}
	}
}
