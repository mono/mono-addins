//
// SearchEntry.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//   Gabriel Burt <gburt@novell.com>
//
// Copyright 2007-2010 Novell, Inc.
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
using Gtk;

namespace Mono.Addins.GuiGtk3
{
	[System.ComponentModel.ToolboxItem(true)]
	class SearchEntry : EventBox
	{
		HBox box = new HBox ();
		Gtk.Entry entry = new Gtk.Entry ();
		HoverImageButton iconFind;
		HoverImageButton iconClean;
		const int notifyDelay = 50;
		bool notifying;
		
		public SearchEntry ()
		{
			entry.HasFrame = false;
			box.PackStart (entry, true, true, 0);
			iconFind = new HoverImageButton (IconSize.Menu, Gtk.Stock.Find);
			box.PackStart (iconFind, false, false, 0);
			iconClean = new HoverImageButton (IconSize.Menu, Gtk.Stock.Clear);
			box.PackStart (iconClean, false, false, 0);
			box.BorderWidth = 1;
			
			HeaderBox hbox = new HeaderBox (1,1,1,1);
			hbox.Show ();
			hbox.Add (box);
			Add (hbox);
			
			ModifyBg (StateType.Normal, entry.Style.Base (StateType.Normal));
			iconClean.ModifyBg (StateType.Normal, entry.Style.Base (StateType.Normal));
			iconFind.ModifyBg (StateType.Normal, entry.Style.Base (StateType.Normal));
			
			iconClean.BorderWidth = 1;
			iconClean.CanFocus = false;
			iconFind.BorderWidth = 1;
			iconFind.CanFocus = false;
			
			iconClean.Clicked += delegate {
				entry.Text = string.Empty;
			};
			
			iconFind.Clicked += delegate {
				FireSearch ();
			};
			
			entry.Activated += delegate {
				FireSearch ();
			};
			
			ShowAll ();
			UpdateIcon ();
			
			entry.Changed += delegate {
				UpdateIcon ();
				FireSearch ();
			};
		}
		
		public event EventHandler TextChanged;
		
		public Gtk.Entry Entry {
			get {
				return this.entry;
			}
			set {
				entry = value;
			}
		}
		
		public string Text {
			get { return entry.Text; }
		}
		
		void UpdateIcon ()
		{
			if (entry.Text.Length > 0) {
				iconFind.Hide ();
				iconClean.Show ();
			}
			else {
				iconFind.Show ();
				iconClean.Hide ();
			}
		}
		
		void FireSearch ()
		{
			if (!notifying) {
				notifying = true;
				GLib.Timeout.Add (notifyDelay, delegate {
					notifying = false;
					if (TextChanged != null)
						TextChanged (this, EventArgs.Empty);
					return false;
				});
			}
		}
	}
}
