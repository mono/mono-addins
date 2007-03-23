
using System;
using System.IO;

namespace Mono.Addins.Setup.ProgressMonitoring
{
	internal class ProgressStatusMonitor: MarshalByRefObject, IProgressMonitor
	{
		IProgressStatus status;
		LogTextWriter logger;
		ProgressTracker tracker = new ProgressTracker ();
		
		public ProgressStatusMonitor (IProgressStatus status)
		{
			this.status = status;
			logger = new LogTextWriter ();
			logger.TextWritten += new LogTextEventHandler (WriteLog);
		}
		
		public static IProgressMonitor GetProgressMonitor (IProgressStatus status)
		{
			if (status == null)
				return new NullProgressMonitor ();
			else
				return new ProgressStatusMonitor (status);
		}
		
		public void BeginTask (string name, int totalWork)
		{
			tracker.BeginTask (name, totalWork);
			status.SetMessage (tracker.CurrentTask);
			status.SetProgress (tracker.GlobalWork);
		}
		
		public void BeginStepTask (string name, int totalWork, int stepSize)
		{
			tracker.BeginStepTask (name, totalWork, stepSize);
			status.SetMessage (tracker.CurrentTask);
			status.SetProgress (tracker.GlobalWork);
		}
		
		public void Step (int work)
		{
			tracker.Step (work);
			status.SetProgress (tracker.GlobalWork);
		}
		
		public void EndTask ()
		{
			tracker.EndTask ();
			status.SetMessage (tracker.CurrentTask);
			status.SetProgress (tracker.GlobalWork);
		}
		
		void WriteLog (string text)
		{
			status.Log (text);
		}
		
		public TextWriter Log {
			get { return logger; }
		}
		
		public void ReportWarning (string message)
		{
			status.ReportWarning (message);
		}
		
		public void ReportError (string message, Exception ex)
		{
			status.ReportError (message, ex);
		}
		
		public bool IsCancelRequested { 
			get { return status.IsCanceled; }
		}
		
		public void Cancel ()
		{
			status.Cancel ();
		}
		
		public bool VerboseLog {
			get { return status.VerboseLog; }
		}
		
		public void Dispose ()
		{
		}
	}
}
