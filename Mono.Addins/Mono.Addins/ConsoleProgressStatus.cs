
using System;

namespace Mono.Addins
{
	public class ConsoleProgressStatus: MarshalByRefObject, IProgressStatus
	{
		bool canceled;
		bool verbose;
		
		public ConsoleProgressStatus (bool verboseLog)
		{
			verbose = verboseLog;
		}
		
		public void SetMessage (string msg)
		{
		}
		
		public void SetProgress (double progress)
		{
		}
		
		public void Log (string msg)
		{
			Console.WriteLine (msg);
		}
		
		public void ReportWarning (string message)
		{
			Console.WriteLine ("WARNING: " + message);
		}
		
		public void ReportError (string message, Exception exception)
		{
			Console.Write ("ERROR: ");
			if (verbose) {
				if (message != null)
					Console.WriteLine (message);
				if (exception != null)
					Console.WriteLine (exception);
			} else {
				if (message != null && exception != null)
					Console.WriteLine (message + " (" + exception.Message + ")");
				else {
					if (message != null)
						Console.WriteLine (message);
					if (exception != null)
						Console.WriteLine (exception.Message);
				}
			}
		}
		
		public bool IsCanceled {
			get { return canceled; }
		}
		
		public bool VerboseLog {
			get { return verbose; }
		}
		
		public void Cancel ()
		{
			canceled = true;
		}
	}
}

