// ProgressDialog.cs
//
// Author:
//   Lluis Sanchez Gual <lluis@novell.com>
//   Robert Nordan <rpvn@robpvn.net> (Ported to GTK#3)
// 
// Copyright (c) 2007 Novell, Inc (http://www.novell.com)
// Copyright (c) 2013 Robert Nordan
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
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Mono.Addins.GuiGtk3
{
	internal class ProgressDialog : Gtk.Dialog, IProgressStatus
	{
		//From UI file
		[UI] Label labelMessage;
		[UI] ProgressBar progressbar;
		[UI] TextView textview;
		[UI] Button buttonCancel;

		bool cancelled;
		bool hadError;
		
		public ProgressDialog (Builder builder, IntPtr handle): base (handle)
		{
			builder.Autoconnect (this);
//			Services.PlaceDialog (this, parent);
			ShowAll ();
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
			Gtk.Application.Invoke (delegate {
				labelMessage.Text = msg;
			});
		}

		public void SetProgress (double progress)
		{
			Gtk.Application.Invoke (delegate {
				progressbar.Fraction = progress;
			});
		}

		public void Log (string msg)
		{
			Gtk.Application.Invoke (delegate {
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
			Gtk.Application.Invoke (delegate {
				Services.ShowError (exception, message, null, true);
			});
			hadError = true;
		}

		public void Cancel ()
		{
			Gtk.Application.Invoke (delegate {
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
