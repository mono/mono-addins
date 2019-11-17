// ProgressDialog.cs
//
// Author:
//   Lluis Sanchez Gual <lluis@novell.com>
//
// Copyright (c) 2007 Novell, Inc (http://www.novell.com)
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
//
//

using System;

namespace Mono.Addins.Gui
{
	internal partial class ProgressDialog : Gtk.Dialog, IProgressStatus
	{
		bool cancelled;
		bool hadError;
		
		readonly WeakReference<Gtk.Window> parent;

		public ProgressDialog (Gtk.Window parent)
		{
			this.Build();
			this.parent = new WeakReference<Gtk.Window> (parent);
			Services.PlaceDialog (this, parent);
		}

		public bool IsCanceled {
			get {
				return cancelled;
			}
		}

		public int LogLevel {
			get {
				return 1;
			}
		}

		public bool HadError {
			get {
				return hadError;
			}
		}
		
		public void SetMessage (string msg)
		{
			Gtk.Application.Invoke ((o, args) => {
				labelMessage.Text = msg;
			});
		}

		public void SetProgress (double progress)
		{
			Gtk.Application.Invoke ((o, args) => {
				progressbar.Fraction = progress;
			});
		}

		public void Log (string msg)
		{
			Gtk.Application.Invoke ((o, args) => {
				Gtk.TextIter it = textview.Buffer.EndIter;
				textview.Buffer.Insert (ref it, msg + "\n");
			});
		}

		public void ReportWarning (string message)
		{
			Log ("WARNING: " + message);
		}

		public void ReportError (string message, Exception exception)
		{
			Log ("Error: " + message);
			if (exception != null)
				Log (exception.ToString ());
			Gtk.Application.Invoke ((o, args) => {
				if (parent.TryGetTarget (out var parentWindow))
					Services.ShowError (exception, message, parentWindow, true);
			});
			hadError = true;
		}

		public void Cancel ()
		{
			Gtk.Application.Invoke ((o, args) => {
				cancelled = true;
				buttonCancel.Sensitive = false;
			});
		}

		protected virtual void OnButtonCancelClicked (object sender, System.EventArgs e)
		{
			Cancel ();
		}
	}
}
