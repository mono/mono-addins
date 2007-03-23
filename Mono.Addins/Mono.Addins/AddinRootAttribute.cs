
using System;

namespace Mono.Addins
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public class AddinRootAttribute: AddinAttribute
	{
		public AddinRootAttribute ()
		{
		}
		
		public AddinRootAttribute (string id): base (id)
		{
		}
		
		public AddinRootAttribute (string id, string version): base (id, version)
		{
		}
		
	}
}
