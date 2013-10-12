//
// AddinManagerWindow.cs
//
// Author:
//   Lluis Sanchez Gual
//   Robert Nordan <rpvn@robpvn.net> (Ported to GTK#3)
// 
// Copyright (c) 2011 Novell, Inc (http://www.novell.com)
// Copyright (c) 2013 Robert Nordan
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

namespace Mono.Addins.GuiGtk3
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

		public static Gtk.Window Show (Gtk.Window parent)
		{
			
			Gtk.Builder builder = new Gtk.Builder (null, "Mono.Addins.GuiGtk3.interfaces.AddinManagerDialog.ui", null);
			AddinManagerDialog dlg = new AddinManagerDialog (builder, builder.GetObject ("AddinManagerDialog").Handle);
			InitDialog (dlg);
			parent.Add (dlg);
			dlg.Show ();
			return dlg;
		}
		
		public static void Run (Gtk.Window parent)
		{
			Gtk.Builder builder = new Gtk.Builder (null, "Mono.Addins.GuiGtk3.interfaces.AddinManagerDialog.ui", null);
			AddinManagerDialog dlg = new AddinManagerDialog (builder, builder.GetObject ("AddinManagerDialog").Handle);
			try {
				InitDialog (dlg);
				dlg.Run ();
			} finally {
				dlg.Destroy ();
			}
		}
	}
}
