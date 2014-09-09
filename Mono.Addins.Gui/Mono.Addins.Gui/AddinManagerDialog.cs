//
// AddinManagerDialog.cs
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
using Mono.Addins;
using Mono.Unix;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Mono.Addins.Gui
{
	partial class AddinManagerDialog : Dialog, IDisposable
	{
		AddinTreeWidget tree;
		AddinTreeWidget galleryTree;
		AddinTreeWidget updatesTree;
		
		SetupService service = new SetupService ();
		ListStore repoStore;
		int lastRepoActive;
		SearchEntry filterEntry;
		Label installedTabLabel;
		Label updatesTabLabel;
		Label galleryTabLabel;

		static bool firstLoad = true;
		
		const string AllRepoMarker = "__ALL";
		const string ManageRepoMarker = "__MANAGE";
		
		internal bool AllowInstall
		{
			set {
				addininfoInstalled.AllowInstall = value;
				addininfoGallery.AllowInstall = value;
				addininfoUpdates.AllowInstall = value;
			}
		}
		
		public AddinManagerDialog (Window parent)
		{
			Build ();
			TransientFor = parent;
			HasSeparator = false;
			Services.PlaceDialog (this, parent);
			Show ();
			
			addininfoInstalled.Init (service);
			addininfoGallery.Init (service);
			
			addinTree.Selection.Mode = SelectionMode.Multiple;
			tree = new AddinTreeWidget (addinTree);
			addinTree.Selection.Changed += OnSelectionChanged;
			tree.VersionVisible = false;
			
			galleryTreeView.Selection.Mode = SelectionMode.Multiple;
			galleryTree = new AddinTreeWidget (galleryTreeView);
			galleryTree.VersionVisible = false;
			galleryTree.ShowInstalledMarkers = true;
			galleryTreeView.Selection.Changed += OnGallerySelectionChanged;
			
			updatesTreeView.Selection.Mode = SelectionMode.Multiple;
			updatesTree = new AddinTreeWidget (updatesTreeView);
			updatesTree.VersionVisible = false;
			updatesTree.ShowCategories = false;
			updatesTree.ShowInstalledMarkers = true;
			updatesTreeView.Selection.Changed += OnGallerySelectionChanged;
			
			repoStore = new ListStore (typeof(string), typeof(string));
			repoCombo.Model = repoStore;
			CellRendererText crt = new CellRendererText ();
			repoCombo.PackStart (crt, true);
			repoCombo.AddAttribute (crt, "text", 0);
			repoCombo.RowSeparatorFunc = delegate(TreeModel model, TreeIter iter) {
				string val = (string) model.GetValue (iter, 0);
				return val == "---";
			};
			
			// Make sure the tree has the focus when switching tabs
			
			vboxUpdates.FocusChain = new Widget [] { scrolledUpdates, eboxRepoUpdates };
			vboxGallery.FocusChain = new Widget [] { scrolledGallery, eboxRepo };
				
			// Improve the look of the headers
			
			HBox tab = new HBox (false, 3);
			tab.PackStart (new Image (Gdk.Pixbuf.LoadFromResource ("plugin-22.png")), false, false, 0);
			installedTabLabel = new Label (Catalog.GetString ("Installed"));
			tab.PackStart (installedTabLabel, true, true, 0);
			tab.BorderWidth = 3;
			tab.ShowAll ();
			notebook.SetTabLabel (notebook.GetNthPage (0), tab);
			
			tab = new HBox (false, 3);
			tab.PackStart (new Image (Gdk.Pixbuf.LoadFromResource ("plugin-update-22.png")), false, false, 0);
			updatesTabLabel = new Label (Catalog.GetString ("Updates"));
			tab.PackStart (updatesTabLabel, true, true, 0);
			tab.BorderWidth = 3;
			tab.ShowAll ();
			notebook.SetTabLabel (notebook.GetNthPage (1), tab);
			
			tab = new HBox (false, 3);
			tab.PackStart (new Image (Gdk.Pixbuf.LoadFromResource ("system-software-update_22.png")), false, false, 0);
			galleryTabLabel = new Label (Catalog.GetString ("Gallery"));
			tab.PackStart (galleryTabLabel, true, true, 0);
			tab.BorderWidth = 3;
			tab.ShowAll ();
			notebook.SetTabLabel (notebook.GetNthPage (2), tab);
			
			// Gradient header for the updates and gallery tabs
			
			HeaderBox hb = new HeaderBox (1, 0, 1, 1);
			hb.SetPadding (6,6,6,6);
			hb.GradientBackround = true;
			hb.Show ();
			hb.Replace (eboxRepo);
			
			hb = new HeaderBox (1, 0, 1, 1);
			hb.SetPadding (6,6,6,6);
			hb.GradientBackround = true;
			hb.Show ();
			hb.Replace (eboxRepoUpdates);
			
			InsertFilterEntry ();
			
			FillRepos ();
			repoCombo.Active = 0;
			
			LoadAll ();
		}
		
		void InsertFilterEntry ()
		{
			filterEntry = new SearchEntry ();
			filterEntry.Entry.SetSizeRequest (200, filterEntry.Entry.SizeRequest ().Height);
			filterEntry.Parent = notebook;
			filterEntry.Show ();
			notebook.SizeAllocated += delegate {
				RepositionFilter ();
			};
			filterEntry.TextChanged += delegate {
				tree.SetFilter (filterEntry.Text);
				galleryTree.SetFilter (filterEntry.Text);
				updatesTree.SetFilter (filterEntry.Text);
				LoadAll ();
				addinTree.ExpandAll ();
				galleryTreeView.ExpandAll ();
			};
			RepositionFilter ();
		}
		
		void RepositionFilter ()
		{
			int w = filterEntry.SizeRequest ().Width;
			int h = filterEntry.SizeRequest ().Height;
			var alloc = notebook.Allocation;
			filterEntry.Allocation = new Gdk.Rectangle (alloc.Left + alloc.Width - 1 - w, alloc.Y, w, h);
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			Destroy ();
		}
		
		internal void OnSelectionChanged (object sender, EventArgs args)
		{
			UpdateAddinInfo ();
		}
		
		internal void OnManageRepos (object sender, EventArgs e)
		{
			ManageSitesDialog dlg = new ManageSitesDialog (this, service);
			try {
				dlg.Run ();
			} finally {
				dlg.Destroy ();
			}
		}
		
		void LoadAll ()
		{
			LoadInstalled ();
			LoadGallery ();
			LoadUpdates ();
			UpdateAddinInfo ();
		}
		
		void UpdateAddinInfo ()
		{
			addininfoInstalled.ShowAddins (tree.ActiveAddinsData);
			addininfoGallery.ShowAddins (galleryTree.ActiveAddinsData);
			addininfoUpdates.ShowAddins (updatesTree.ActiveAddinsData);
		}
		
		void LoadInstalled ()
		{
			object s = tree.SaveStatus ();
			
			int count = 0;
			tree.Clear ();
			foreach (Addin ainfo in AddinManager.Registry.GetModules (AddinSearchFlags.IncludeAddins | AddinSearchFlags.LatestVersionsOnly)) {
				if (Services.InApplicationNamespace (service, ainfo.Id) && !ainfo.Description.IsHidden) {
					AddinHeader ah = SetupService.GetAddinHeader (ainfo);
					if (IsFiltered (ah))
						continue;
					AddinStatus st = AddinStatus.Installed;
					if (!ainfo.Enabled || Services.GetMissingDependencies (ainfo).Any())
						st |= AddinStatus.Disabled;
					if (addininfoInstalled.GetUpdate (ainfo) != null)
						st |= AddinStatus.HasUpdate;
					tree.AddAddin (ah, ainfo, st);
					count++;
				}
			}
			
			if (count > 0)
				tree.RestoreStatus (s);
			else
				tree.ShowEmptyMessage ();
			
			UpdateAddinInfo ();
			
			installedTabLabel.Text = Catalog.GetString ("Installed");
			
			if (filterEntry.Text.Length != 0 && count > 0)
				installedTabLabel.Text += " (" + count + ")";
		}
		
		void FillRepos ()
		{
			int i = repoCombo.Active;
			repoStore.Clear ();
			
			repoStore.AppendValues (Catalog.GetString ("All repositories"), AllRepoMarker);
			
			foreach (AddinRepository rep in service.Repositories.GetRepositories ()) {
				if (rep.Enabled)
					repoStore.AppendValues (rep.Title, rep.Url);
			}
			repoStore.AppendValues ("---", "");
			repoStore.AppendValues (Catalog.GetString ("Manage Repositories..."), ManageRepoMarker);
			repoCombo.Active = i;
		}
		
		string GetRepoSelection ()
		{
			Gtk.TreeIter iter;
			if (!repoCombo.GetActiveIter (out iter))
				return null;
			return (string) repoStore.GetValue (iter, 1);
		}
		
		void LoadGallery ()
		{
			object s = galleryTree.SaveStatus ();
			
			galleryTree.Clear ();
			
			string rep = GetRepoSelection ();
			
			AddinRepositoryEntry[] reps;
			if (rep == AllRepoMarker)
				reps = service.Repositories.GetAvailableAddins (RepositorySearchFlags.LatestVersionsOnly);
			else
				reps = service.Repositories.GetAvailableAddins (rep, RepositorySearchFlags.LatestVersionsOnly);
			
			int count = 0;
			
			foreach (AddinRepositoryEntry arep in reps)
			{
				if (!Services.InApplicationNamespace (service, arep.Addin.Id))
					continue;
				
				if (IsFiltered (arep.Addin))
					continue;
				
				AddinStatus status = AddinStatus.NotInstalled;
				
				// Find whatever version is installed
				Addin sinfo = AddinManager.Registry.GetAddin (Addin.GetIdName (arep.Addin.Id));
				
				if (sinfo != null) {
					status |= AddinStatus.Installed;
					if (!sinfo.Enabled || Services.GetMissingDependencies (sinfo).Any())
						status |= AddinStatus.Disabled;
					if (Addin.CompareVersions (sinfo.Version, arep.Addin.Version) > 0)
						status |= AddinStatus.HasUpdate;
				}
				galleryTree.AddAddin (arep.Addin, arep, status);
				count++;
			}
			
			if (count > 0)
				galleryTree.RestoreStatus (s);
			else
				galleryTree.ShowEmptyMessage ();
			
			galleryTabLabel.Text = Catalog.GetString ("Gallery");
			
			if (filterEntry.Text.Length != 0 && count > 0)
				galleryTabLabel.Text += " (" + count + ")";
		}
		
		void LoadUpdates ()
		{
			object s = updatesTree.SaveStatus ();
			
			updatesTree.Clear ();
			
			AddinRepositoryEntry[] reps;
			reps = service.Repositories.GetAvailableAddins (RepositorySearchFlags.LatestVersionsOnly);
			
			int count = 0;
			
			foreach (AddinRepositoryEntry arep in reps)
			{
				if (!Services.InApplicationNamespace (service, arep.Addin.Id))
					continue;
				
				// Find whatever version is installed
				Addin sinfo = AddinManager.Registry.GetAddin (Addin.GetIdName (arep.Addin.Id));
				if (sinfo == null || !sinfo.Enabled || Addin.CompareVersions (sinfo.Version, arep.Addin.Version) <= 0)
					continue;
				
				if (IsFiltered (arep.Addin))
					continue;
				
				AddinStatus status = AddinStatus.Installed;
				if (!sinfo.Enabled || Services.GetMissingDependencies (sinfo).Any())
					status |= AddinStatus.Disabled;
				
				updatesTree.AddAddin (arep.Addin, arep, status | AddinStatus.HasUpdate);
				count++;
			}
			
			labelUpdates.Text = string.Format (Catalog.GetPluralString ("{0} update available", "{0} updates available", count), count);
			updatesTabLabel.Text = Catalog.GetString ("Updates");
			if (count > 0)
				updatesTabLabel.Text += " (" + count + ")";
			
			buttonUpdateAll.Visible = count > 0;
			
			if (count > 0)
				updatesTree.RestoreStatus (s);
			else
				updatesTree.ShowEmptyMessage ();
		}
		
		bool IsFiltered (AddinHeader ah)
		{
			if (filterEntry.Text.Length == 0)
				return false;
			if (ah.Name.IndexOf (filterEntry.Text, StringComparison.CurrentCultureIgnoreCase) != -1)
				return false;
			if (ah.Description.IndexOf (filterEntry.Text, StringComparison.CurrentCultureIgnoreCase) != -1)
				return false;
			if (ah.Id.IndexOf (filterEntry.Text, StringComparison.CurrentCultureIgnoreCase) != -1)
				return false;
			return true;
		}
		
		void ManageSites ()
		{
			ManageSitesDialog dlg = new ManageSitesDialog (this, service);
			try {
				dlg.Run ();
				repoCombo.Active = lastRepoActive;
				FillRepos ();
			} finally {
				dlg.Destroy ();
			}
		}
		
		protected virtual void OnRepoComboChanged (object sender, System.EventArgs e)
		{
			if (GetRepoSelection () == ManageRepoMarker)
				ManageSites ();
			else
				LoadGallery ();
			lastRepoActive = repoCombo.Active;
		}
		
		protected virtual void OnGallerySelectionChanged (object sender, System.EventArgs e)
		{
			UpdateAddinInfo ();
		}

		void UpdateRepositories ()
		{
			ProgressDialog pdlg = new ProgressDialog (this);
			if (!firstLoad)
				pdlg.Show ();
			pdlg.SetMessage (AddinManager.CurrentLocalizer.GetString ("Updating repository"));
			bool updateDone = buttonRefresh.Sensitive = false;

			Thread t = new Thread (delegate () {
				try {
					service.Repositories.UpdateAllRepositories (pdlg);
				} finally {
					updateDone = true;
				}
			});
			t.Start ();
			while (!updateDone) {
				while (Gtk.Application.EventsPending ())
					Gtk.Application.RunIteration ();
				Thread.Sleep (50);
			}
			buttonRefresh.Sensitive = true;
			pdlg.Destroy ();
		}

		protected virtual void OnButtonRefreshClicked (object sender, System.EventArgs e)
		{
			UpdateRepositories ();
			LoadGallery ();
			LoadUpdates ();
		}
		
		protected virtual void OnInstallClicked (object sender, System.EventArgs e)
		{
			InstallDialog dlg = new InstallDialog (this, service);
			try {
				List<AddinRepositoryEntry> selectedEntry = ((AddinInfoView)sender).SelectedEntries;
				dlg.InitForInstall (selectedEntry.ToArray ());
				if (dlg.Run () == (int) Gtk.ResponseType.Ok)
					LoadAll ();
			} finally {
				dlg.Destroy ();
			}
		}
		
		protected virtual void OnUninstallClicked (object sender, System.EventArgs e)
		{
			List<Addin> selectedAddin = ((AddinInfoView)sender).SelectedAddins;
			InstallDialog dlg = new InstallDialog (this, service);
			try {
				dlg.InitForUninstall (selectedAddin.ToArray ());
				if (dlg.Run () == (int) Gtk.ResponseType.Ok) {
					LoadAll ();
				}
			} finally {
				dlg.Destroy ();
			}
		}
		
		protected virtual void OnUpdateClicked (object sender, System.EventArgs e)
		{
			List<AddinRepositoryEntry> selectedEntry = ((AddinInfoView)sender).SelectedEntries;
			InstallDialog dlg = new InstallDialog (this, service);
			try {
				dlg.InitForInstall (selectedEntry.ToArray ());
				if (dlg.Run () == (int) Gtk.ResponseType.Ok)
					LoadAll ();
			} finally {
				dlg.Destroy ();
			}
		}
		
		protected virtual void OnEnableDisableClicked (object sender, System.EventArgs e)
		{
			try {
				foreach (Addin a in ((AddinInfoView)sender).SelectedAddins) {
					a.Enabled = !a.Enabled;
				}
				LoadAll ();
			}
			catch (Exception ex) {
				Services.ShowError (ex, null, this, true);
			}
		}
		
		protected virtual void OnUpdateAll (object sender, System.EventArgs e)
		{
			object[] data = updatesTree.AddinsData;
			AddinRepositoryEntry[] entries = new AddinRepositoryEntry [data.Length];
			Array.Copy (data, entries, data.Length);
			InstallDialog dlg = new InstallDialog (this, service);
			try {
				dlg.InitForInstall (entries);
				if (dlg.Run () == (int) Gtk.ResponseType.Ok)
					LoadAll ();
			} finally {
				dlg.Destroy ();
			}
		}
		
		static string lastFolder;
		
		protected virtual void OnButtonInstallFromFileClicked (object sender, System.EventArgs e)
		{
			string[] files;
			Gtk.FileChooserDialog dlg = new Gtk.FileChooserDialog (Catalog.GetString ("Install Add-in Package"), this, FileChooserAction.Open);
			try {
				if (lastFolder != null)
					dlg.SetCurrentFolder (lastFolder);
				else
					dlg.SetCurrentFolder (Environment.GetFolderPath (Environment.SpecialFolder.Personal));
				dlg.SelectMultiple = true;
				
				Gtk.FileFilter f = new Gtk.FileFilter ();
				f.AddPattern ("*.mpack");
				f.Name = Catalog.GetString ("Add-in packages");
				dlg.AddFilter (f);
				
				f = new Gtk.FileFilter ();
				f.AddPattern ("*");
				f.Name = Catalog.GetString ("All files");
				dlg.AddFilter (f);
				
				dlg.AddButton (Gtk.Stock.Cancel, ResponseType.Cancel);
				dlg.AddButton (Gtk.Stock.Open, ResponseType.Ok);
				if (dlg.Run () != (int) Gtk.ResponseType.Ok)
					return;
				files = dlg.Filenames;
				lastFolder = dlg.CurrentFolder;
			} finally {
				dlg.Destroy ();
			}
			
			InstallDialog idlg = new InstallDialog (this, service);
			try {
				idlg.InitForInstall (files);
				if (idlg.Run () == (int) Gtk.ResponseType.Ok)
					LoadAll ();
			} finally {
				idlg.Destroy ();
			}
		}

		protected void OnNotebookSwitchPage (object o, SwitchPageArgs args)
		{
			if (args.PageNum == 2 && firstLoad) {
				UpdateRepositories ();
				firstLoad = false;
			}
		}
	}
}
