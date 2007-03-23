
using System;
using System.Collections;

namespace Mono.Addins.Description
{
	public class ExtensionCollection: ObjectDescriptionCollection
	{
		public Extension this [int n] {
			get { return (Extension) List [n]; }
		}
	}
}
