
using System;

namespace Mono.Addins
{
	public delegate void AddinErrorEventHandler (object sender, AddinErrorEventArgs args);
	
	public class AddinErrorEventArgs: AddinEventArgs
	{
		Exception exception;
		string message;
		
		public AddinErrorEventArgs (string message, string addinId, Exception exception): base (addinId)
		{
			this.message = message;
			this.exception = exception;
		}
		
		public Exception Exception {
			get { return exception; }
		}
		
		public string Message {
			get { return message; }
		}
	}
}
