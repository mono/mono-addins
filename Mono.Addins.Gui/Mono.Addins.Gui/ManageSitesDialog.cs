//
// ManageSitesDialog.cs
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
using Mono.Unix;
using System.Threading;

using Mono.Addins.Setup;


namespace Mono.Addins.Gui
{
	partial class ManageSitesDialog : Dialog
	{
		ListStore treeStore;
		SetupService service;
		
		public ManageSitesDialog (Gtk.Window parent, SetupService service)
		{
			Build ();
			TransientFor = parent;
			Services.PlaceDialog (this, parent);
			this.service = service;
			treeStore = new Gtk.ListStore (typeof (string), typeof (string), typeof(bool));
			repoTree.Model = treeStore;
			repoTree.HeadersVisible = false;
			var crt = new Gtk.CellRendererToggle ();
			crt.Toggled += HandleRepoToggled;
			repoTree.AppendColumn ("", crt, "active", 2);
			repoTree.AppendColumn ("", new Gtk.CellRendererText (), "markup", 1);
			repoTree.Selection.Changed += new EventHandler(OnSelect);
			
			AddinRepository[] reps = service.Repositories.GetRepositories ();
			foreach (AddinRepository rep in reps)
				AppendRepository (rep);

			btnRemove.Sensitive = false;
		}

		public override void Dispose ()
		{
			base.Dispose ();
			Destroy ();
		}
		
		void AppendRepository (AddinRepository rep)
		{
			string txt = GLib.Markup.EscapeText (rep.Title) + "\n<span color='darkgray'>" + GLib.Markup.EscapeText (rep.Url) + "</span>";
			treeStore.AppendValues (rep.Url, txt, rep.Enabled);
		}
		
		protected void OnAdd (object sender, EventArgs e)
		{
			NewSiteDialog dlg = new NewSiteDialog (this);
			try {
				if (dlg.Run ()) {
					string url = dlg.Url;
					if (!url.StartsWith ("http://") && !url.StartsWith ("https://") && !url.StartsWith ("file://")) {
						url = "http://" + url;
					}
					
					try {
						new Uri (url);
					} catch {
						Services.ShowError (null, "Invalid url: " + url, null, true);
					}
					
					if (!service.Repositories.ContainsRepository (url)) {
						ProgressDialog pdlg = new ProgressDialog (this);
						pdlg.Show ();
						pdlg.SetMessage (AddinManager.CurrentLocalizer.GetString ("Registering repository"));
						
						bool done = false;
						AddinRepository rr = null;
						Exception error = null;
						
						ThreadPool.QueueUserWorkItem (delegate {
							try {
								rr = service.Repositories.RegisterRepository (pdlg, url, true, "MonoAddins");
							} catch (System.Exception ex) {
								error = ex;
							} finally {
								done = true;
							}
						});
						
						while (!done) {
							if (Gtk.Application.EventsPending ())
								Gtk.Application.RunIteration ();
							else
								Thread.Sleep (100);
						}

						pdlg.Destroy ();
						
						if (pdlg.HadError) {
							if (rr != null)
								service.Repositories.RemoveRepository (rr.Url);
							return;
						}
						
						if (error != null) {
							Services.ShowError (error, "The repository could not be registered", null, true);
							return;
						}
						
						AppendRepository (rr);
					}
				}
			} finally {
				dlg.Destroy ();
			}
		}
		
		protected void OnRemove (object sender, EventArgs e)
		{
			Gtk.TreeModel foo;
			Gtk.TreeIter iter;
			if (!repoTree.Selection.GetSelected (out foo, out iter))
				return;
				
			string rep = (string) treeStore.GetValue (iter, 0);
			service.Repositories.RemoveRepository (rep);
			
			treeStore.Remove (ref iter);
		}

		void HandleRepoToggled (object o, ToggledArgs args)
		{
			Gtk.TreeIter iter;
			if (!treeStore.GetIterFromString (out iter, args.Path))
				return;
			
			bool newVal = !(bool) treeStore.GetValue (iter, 2);
			string rep = (string) treeStore.GetValue (iter, 0);
			service.Repositories.SetRepositoryEnabled (rep, newVal);
			
			treeStore.SetValue (iter, 2, newVal);
		}
		
		protected void OnSelect(object sender, EventArgs e)
		{
			btnRemove.Sensitive = repoTree.Selection.CountSelectedRows() > 0;
		}
	}
}
