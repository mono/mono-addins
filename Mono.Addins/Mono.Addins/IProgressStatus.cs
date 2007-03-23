
using System;

namespace Mono.Addins
{
	public interface IProgressStatus
	{
		void SetMessage (string msg);
		void SetProgress (double progress);
		
		void Log (string msg);
		bool VerboseLog { get; }
		
		void ReportWarning (string message);
		void ReportError (string message, Exception exception);
		
		bool IsCanceled { get; }
		void Cancel ();
	}
}
