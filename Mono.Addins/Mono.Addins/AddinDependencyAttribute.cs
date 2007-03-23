
using System;

namespace Mono.Addins
{
	[AttributeUsage (AttributeTargets.Assembly, AllowMultiple=true)]
	public class AddinDependencyAttribute: Attribute
	{
		string id;
		string version;
		
		public AddinDependencyAttribute (string id, string version)
		{
			this.id = id;
			this.version = version;
		}
		
		public string Id {
			get { return id; }
		}
		
		public string Version {
			get { return version; }
		}
		
	}
}
