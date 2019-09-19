//
// AddinManagerWindow.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using Mono.Addins.Setup;

namespace Mono.Addins.Gui
{
	public class AddinManagerWindow
	{
		private static bool mAllowInstall = true;
		
		public static bool AllowInstall
		{
			get { return mAllowInstall; }
			set { mAllowInstall = value; }
		}
		
		private AddinManagerWindow()
		{
		}
		
		private static void InitDialog (AddinManagerDialog dlg)
		{
			dlg.AllowInstall = AllowInstall;
		}

		public static Gtk.Dialog Create (Gtk.Window parent = null, SetupService service = null)
		{
			if (service == null) {
				service = new SetupService ();
			}
			var dlg = new AddinManagerDialog (parent, service);
			InitDialog (dlg);
			return dlg;
		}

		public static Gtk.Window Show (Gtk.Window parent)
		{
			return Show (parent, new SetupService ());
		}
		
		public static Gtk.Window Show (Gtk.Window parent, SetupService service)
		{
			var dlg = Create (parent, service);
			if (parent == null) {
				dlg.SetPosition (Gtk.WindowPosition.Center);
			}
			dlg.Show ();
			return dlg;
		}

		public static void Run (Gtk.Window parent)
		{
			Run (parent, new SetupService ());
		}

		public static void Run (Gtk.Window parent, SetupService service)
		{
			var dlg = (AddinManagerDialog) Create (parent, service);
			try {
				InitDialog (dlg);
				dlg.Run ();
			} finally {
				dlg.Destroy ();
			}
		}

		public static int RunToInstallFile (Gtk.Window parent, Setup.SetupService service, string file)
		{
			var dlg = new InstallDialog (parent, service);
			try {
				dlg.InitForInstall (new [] { file });
				return dlg.Run ();
			} finally {
				dlg.Destroy ();
			}
		}
	}
}
