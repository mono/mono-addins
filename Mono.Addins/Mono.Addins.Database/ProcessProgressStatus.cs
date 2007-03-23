
using System;
using System.IO;

namespace Mono.Addins.Database
{
	internal class ProcessProgressStatus: MarshalByRefObject, IProgressStatus
	{
		bool canceled;
		bool verbose;
		
		public ProcessProgressStatus (bool verboseLog)
		{
			verbose = verboseLog;
		}
		
		public void SetMessage (string msg)
		{
			Console.WriteLine ("process-ps-msg:" + Encode (msg));
		}
		
		public void SetProgress (double progress)
		{
			Console.WriteLine ("process-ps-progress:" + progress.ToString ());
		}
		
		public void Log (string msg)
		{
			Console.WriteLine ("process-ps-log:" + Encode (msg));
		}
		
		public void ReportWarning (string message)
		{
			Console.WriteLine ("process-ps-warning:" + Encode (message));
		}
		
		public void ReportError (string message, Exception exception)
		{
			if (message == null) message = string.Empty;
			string et;
			if (verbose)
				et = exception != null ? exception.ToString () : string.Empty;
			else
				et = exception != null ? exception.Message : string.Empty;
			
			Console.WriteLine ("process-ps-exception:" + Encode (et));
			Console.WriteLine ("process-ps-error:" + Encode (message));
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
			Console.WriteLine ("process-ps-cancel:");
		}
		
		static string Encode (string msg)
		{
			msg = msg.Replace ("&", "&a");
			return msg.Replace ("\n", "&n");
		}
		
		static string Decode (string msg)
		{
			msg = msg.Replace ("&n", "\n");
			return msg.Replace ("&a", "&");
		}
		
		public static void MonitorProcessStatus (IProgressStatus monitor, TextReader reader)
		{
			string line;
			string exceptionText = null;
			while ((line = reader.ReadLine ()) != null) {
				int i = line.IndexOf (':');
				if (i != -1) {
					string tag = line.Substring (0, i);
					string txt = line.Substring (i+1);
					bool wasTag = true;
					
					switch (tag) {
						case "process-ps-msg":
							monitor.SetMessage (Decode (txt));
							break;
						case "process-ps-progress":
							monitor.SetProgress (double.Parse (txt));
							break;
						case "process-ps-log":
							monitor.Log (Decode (txt));
							break;
						case "process-ps-warning":
							monitor.ReportWarning (Decode (txt));
							break;
						case "process-ps-exception":
							exceptionText = Decode (txt);
							if (exceptionText == string.Empty)
								exceptionText = null;
							break;
						case "process-ps-error":
							string err = Decode (txt);
							if (err == string.Empty) err = null;
							monitor.ReportError (err, exceptionText != null ? new Exception (exceptionText) : null);
							break;
						case "process-ps-cancel":
							monitor.Cancel ();
							break;
						default:
							wasTag = false;
							break;
					}
					if (wasTag)
						continue;
				}
				Console.WriteLine (line);
			}
		}
	}
}
