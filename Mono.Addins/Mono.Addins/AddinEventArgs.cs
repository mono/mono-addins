
using System;

namespace Mono.Addins
{
	public delegate void AddinEventHandler (object sender, AddinEventArgs args);
	
	public class AddinEventArgs: EventArgs
	{
		string addinId;
		
		public AddinEventArgs (string addinId)
		{
			this.addinId = addinId;
		}
		
		public string AddinId {
			get { return addinId; }
		}
	}
}
