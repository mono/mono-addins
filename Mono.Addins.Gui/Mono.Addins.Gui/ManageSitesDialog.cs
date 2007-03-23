
using System;
using Gtk;
using Mono.Unix;

using Mono.Addins.Setup;


namespace Mono.Addins.Gui
{
	partial class ManageSitesDialog : Dialog
	{
		ListStore treeStore;
		SetupService service;
		
		public ManageSitesDialog (SetupService service)
		{
			Build ();
			this.service = service;
			treeStore = new Gtk.ListStore (typeof (string), typeof (string));
			repoTree.Model = treeStore;
			repoTree.HeadersVisible = true;
			repoTree.AppendColumn (Catalog.GetString ("Name"), new Gtk.CellRendererText (), "text", 1);
			repoTree.AppendColumn (Catalog.GetString ("Url"), new Gtk.CellRendererText (), "text", 0);
			repoTree.Selection.Changed += new EventHandler(OnSelect);
			
			AddinRepository[] reps = service.Repositories.GetRepositories ();
			foreach (AddinRepository rep in reps) {
				treeStore.AppendValues (rep.Url, rep.Title);
			}

			btnRemove.Sensitive = false;
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			Destroy ();
		}
		
		protected void OnAdd (object sender, EventArgs e)
		{
			using (NewSiteDialog dlg = new NewSiteDialog ()) {
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
						IProgressStatus m = new ConsoleProgressStatus (false);
						AddinRepository rr = service.Repositories.RegisterRepository (m, url);
						if (rr == null) {
							Services.ShowError (null, "The repository could not be registered", null, true);
							return;
						}
						treeStore.AppendValues (rr.Url, rr.Title);
					}
				}
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

		protected void OnSelect(object sender, EventArgs e)
		{
			btnRemove.Sensitive = repoTree.Selection.CountSelectedRows() > 0;
		}
	}
}
