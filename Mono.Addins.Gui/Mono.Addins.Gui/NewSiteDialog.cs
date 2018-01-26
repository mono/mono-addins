//
// NewSiteDialog.cs
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
using Gtk;
using Mono.Addins.Setup;
using Mono.Unix;

namespace Mono.Addins.Gui
{
	partial class NewSiteDialog : Dialog
	{
		ComboBox typeComboBox;
		public NewSiteDialog (Gtk.Window parent)
		{
			Build ();
			var hbox = new HBox ();
			hbox.Spacing = 6;
			hbox.PackStart (new Label(), false, false, 10);
			var label = new Label ();
			label.Text = Catalog.GetString ("Type:");
			hbox.PackStart (label, false, false, 1);
			typeComboBox = new ComboBox (new string [] {
				Catalog.GetString ("Add-in Repository"),//AddinRepositoryType.MonoAddins
				Catalog.GetString ("Visual Studio Marketplace")//AddinRepositoryType.VisualStudioMarketplace
			});
			typeComboBox.Active = 0;
			hbox.PackStart (typeComboBox, true, true, 1);
			hbox.ShowAll ();
			vbox89.Add (hbox);
			var w6 = (Box.BoxChild)vbox89 [hbox];
			w6.Position = 3;

			TransientFor = parent;
			Services.PlaceDialog (this, parent);
			pathEntry.Sensitive = false;
			CheckValues ();
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			Destroy ();
		}
		
		public string Url {
			get {
				if (btnOnlineRep.Active)
					return urlText.Text;
				else if (pathEntry.Text.Length > 0)
					return "file://" + pathEntry.Text;
				else
					return string.Empty;
			}
		}

		public AddinRepositoryType AddinRepositoryType {
			get {
				return (AddinRepositoryType)typeComboBox.Active;
			}
		}
		
		void CheckValues ()
		{
			btnOk.Sensitive = (Url != "");
		}
		
		public new bool Run ()
		{
			ShowAll ();
			return ((ResponseType) base.Run ()) == ResponseType.Ok;
		}
		
		protected void OnClose (object sender, EventArgs args)
		{
			Destroy ();
		}
		
		protected void OnOptionClicked (object sender, EventArgs e)
		{
			if (btnOnlineRep.Active) {
				urlText.Sensitive = true;
				typeComboBox.Sensitive = true;
				pathEntry.Sensitive = false;
			} else {
				urlText.Sensitive = false;
				typeComboBox.Sensitive = false;
				pathEntry.Sensitive = true;
			}
			CheckValues ();
		}

		protected virtual void OnButtonBrowseClicked(object sender, System.EventArgs e)
		{
			FileChooserDialog dlg = new FileChooserDialog ("Select Folder", this, FileChooserAction.SelectFolder);
			try {
				dlg.AddButton (Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
				dlg.AddButton (Gtk.Stock.Open, Gtk.ResponseType.Ok);
				
				dlg.SetFilename (Environment.GetFolderPath (Environment.SpecialFolder.Personal));
				if (dlg.Run () == (int) ResponseType.Ok) {
					pathEntry.Text = dlg.Filename;
				}
			} finally {
				dlg.Destroy ();
			}
		}

		protected virtual void OnPathEntryChanged(object sender, System.EventArgs e)
		{
			CheckValues ();
		}

		protected virtual void OnUrlTextChanged (object sender, System.EventArgs e)
		{
			CheckValues ();
		}
	}
}
