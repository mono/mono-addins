// 
// AddinInstallDialog.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2011 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using Mono.Unix;
using Gtk;
using Mono.Addins.Setup;
using Mono.Addins.Description;
namespace Mono.Addins.GuiGtk3
{
	class InstallMonitor: IProgressStatus, IDisposable
	{
		Label progressLabel;
		ProgressBar progressBar;
		StringCollection errors = new StringCollection ();
		StringCollection warnings = new StringCollection ();
		bool canceled;
		bool done;
		string mainOperation;
		
		public InstallMonitor (Label progressLabel, ProgressBar progressBar, string mainOperation)
		{
			this.progressLabel = progressLabel;
			this.progressBar = progressBar;
			this.mainOperation = mainOperation;
		}
		
		public InstallMonitor ()
		{
		}
		
		public void SetMessage (string msg)
		{
			if (progressLabel != null)
				progressLabel.Markup = "<b>" + GLib.Markup.EscapeText (mainOperation) + "</b>\n" + GLib.Markup.EscapeText (msg);
			RunPendingEvents ();
		}
		
		public void SetProgress (double progress)
		{
			if (progressBar != null)
				progressBar.Fraction = progress;
			RunPendingEvents ();
		}
		
		public void Log (string msg)
		{
			Console.WriteLine (msg);
		}
		
		public void ReportWarning (string message)
		{
			warnings.Add (message);
		}
		
		public void ReportError (string message, Exception exception)
		{
			errors.Add (message);
		}
		
		public bool IsCanceled {
			get { return canceled; }
		}
		
		public StringCollection Errors {
			get { return errors; }
		}
		
		public StringCollection Warnings {
			get { return warnings; }
		}
		
		public void Cancel ()
		{
			canceled = true;
		}
		
		public int LogLevel {
			get { return 1; }
		}
		
		public void Dispose ()
		{
			done = true;
		}
		
		public void WaitForCompleted ()
		{
			while (!done) {
				RunPendingEvents ();
				Thread.Sleep (50);
			}
		}
		
		public bool Success {
			get { return errors.Count == 0; }
		}
		
		void RunPendingEvents ()
		{
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
		}
	}
}
