
using System;

namespace Mono.Addins
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public class AddinAttribute: Attribute
	{
		string id;
		string version;
		string ns;
		string category;
		
		public AddinAttribute ()
		{
		}
		
		public AddinAttribute (string id)
		{
			this.id = id;
		}
		
		public AddinAttribute (string id, string version)
		{
			this.id = id;
			this.version = version;
		}
		
		public string Id {
			get { return id != null ? id : string.Empty; }
			set { id = value; }
		}
		
		public string Version {
			get { return version != null ? version : string.Empty; }
			set { version = value; }
		}
		
		public string Namespace {
			get { return ns != null ? ns : string.Empty; }
			set { ns = value; }
		}
		
		public string Category {
			get { return category != null ? category : string.Empty; }
			set { category = value; }
		}
	}
}
