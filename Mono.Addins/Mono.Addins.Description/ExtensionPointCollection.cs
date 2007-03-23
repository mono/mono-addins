
using System;
using System.Collections;

namespace Mono.Addins.Description
{
	public class ExtensionPointCollection: ObjectDescriptionCollection
	{
		public ExtensionPoint this [int n] {
			get { return (ExtensionPoint) List [n]; }
		}
		
		public ExtensionPoint this [string path] {
			get {
				for (int n=0; n<List.Count; n++)
					if (((ExtensionPoint) List [n]).Path == path)
						return (ExtensionPoint) List [n];
				return null;
			}
		}
	}
}
