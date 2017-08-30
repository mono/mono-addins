// 
// AddinInfoView.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
//       Robert Nordan <rpvn@robpvn.net> (Ported to GTK#3)
// 
// Copyright (c) 2011 Novell, Inc (http://www.novell.com)
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
using System;
using System.IO;
using System.Collections.Generic;
using Mono.Addins.Setup;
using System.Text;
using Mono.Unix;
using System.Linq;
using UI = Gtk.Builder.ObjectAttribute;
using Gtk;

namespace Mono.Addins.GuiGtk3
{
	[System.ComponentModel.ToolboxItem(true)]
	class AddinInfoView : Gtk.Bin
	{
		//From UI files
		[UI] EventBox eboxButs;
		[UI] Label labelName;
		[UI] Label labelDesc;
		[UI] Button urlButton;
		[UI] HBox boxTitle;
		[UI] EventBox boxHeader;
		[UI] HBox headerBox;
		[UI] Button btnDisable;
		[UI] Button btnInstall;
		[UI] Button btnUninstall;
		[UI] Button btnUpdate;
		[UI] Label labelHeader;
		[UI] Image imageHeader;
		[UI] Label labelVersion;
		[UI] EventBox ebox;
		[UI] EventBox ebox2;
		[UI] Box vboxDesc;
		[UI] ScrolledWindow scrolledwindow;

		List<AddinRepositoryEntry> selectedEntry = new List<AddinRepositoryEntry> ();
		List<Addin> selectedAddin = new List<Addin> ();
		SetupService service;
		HeaderBox topHeaderBox;
		List<Gtk.Widget> previewImages = new List<Gtk.Widget> ();
		ImageContainer titleIcon;
		int titleWidth;
		string infoUrl;
		
		public event EventHandler InstallClicked;
		public event EventHandler UninstallClicked;
		public event EventHandler UpdateClicked;
		public event EventHandler EnableDisableClicked;
		
		public AddinInfoView ()
		{
			Builder builder = new Gtk.Builder (null, "Mono.Addins.GuiGtk3.interfaces.AddinInfoView.ui", null);
			builder.Autoconnect (this);
			Add ((Box) builder.GetObject ("AddinInfoView"));
			AllowInstall = true;
			titleWidth = labelName.SizeRequest ().Width;
			
			HeaderBox hb = new HeaderBox (1,1,1,1);
			hb.Show ();
			hb.Replace (this);
			
			hb = new HeaderBox (1,0,0,0);
			hb.SetPadding (6,6,6,6);
			hb.Show ();
			hb.GradientBackround = true;
			hb.Replace (eboxButs);
			
			hb = new HeaderBox (0,1,0,0);
			hb.SetPadding (6,6,6,6);
			hb.Show ();
			hb.GradientBackround = true;
			hb.Replace (boxHeader);
			topHeaderBox = hb;

			//Enable our buttons for clicking
			btnDisable.Clicked += OnBtnDisableClicked;
			btnInstall.Clicked += OnBtnInstallClicked;
			btnUninstall.Clicked += OnBtnUninstallClicked;
			btnUpdate.Clicked += OnBtnUpdateClicked;
			urlButton.Clicked += OnUrlButtonClicked;

			ShowAll ();
		}
		
		public void Init (SetupService service)
		{
			this.service = service;
		}
		
		public bool AllowInstall { get; set; }
		
		public List<AddinRepositoryEntry> SelectedEntries {
			get {
				return this.selectedEntry;
			}
		}

		public List<Addin> SelectedAddins {
			get {
				return this.selectedAddin;
			}
		}
		
		public void ShowAddins (object[] data)
		{
			selectedEntry.Clear ();
			selectedAddin.Clear ();
			eboxButs.Visible = true;
			topHeaderBox.Hide ();
			urlButton.Hide ();
			
			if (titleIcon != null) {
				boxTitle.Remove (titleIcon);
				titleIcon.Destroy ();
				titleIcon = null;
			}
			
			foreach (var img in previewImages) {
				((Gtk.Container)img.Parent).Remove (img);
				img.Destroy ();
			}
			previewImages.Clear ();
			
			if (data.Length == 1) {
				headerBox.Show ();
				ShowAddin (data[0]);
			}
			else if (data.Length > 1) {
				headerBox.Hide ();
				StringBuilder sb = new StringBuilder ();
				sb.Append (Catalog.GetString ("Multiple selection:\n\n"));
				bool allowUpdate = AllowInstall;
				bool allowInstall = true;
				bool allowUninstall = AllowInstall;
				bool allowEnable = true;
				bool allowDisable = true;
				
				foreach (object o in data) {
					Addin installed;
					if (o is Addin) {
						Addin a = (Addin)o;
						installed = a;
						selectedAddin.Add (a);
						sb.Append (a.Name);
					}
					else {
						AddinRepositoryEntry entry = (AddinRepositoryEntry) o;
						selectedEntry.Add (entry);
						sb.Append (entry.Addin.Name);
						installed = AddinManager.Registry.GetAddin (Addin.GetIdName (entry.Addin.Id));
					}
					if (installed != null) {
						if (GetUpdate (installed) == null)
							allowUpdate = false;
						allowInstall = false;
						if (installed.Enabled)
							allowEnable = false;
						else
							allowDisable = false;
					} else
						allowEnable = allowDisable = allowUninstall = allowUpdate = false;
					
					sb.Append ('\n');
					labelDesc.Text = sb.ToString ();
					
					if (allowEnable) {
						btnDisable.Visible = true;
						btnDisable.Label = Catalog.GetString ("Enable");
					} else if (allowDisable) {
						btnDisable.Visible = true;
						btnDisable.Label = Catalog.GetString ("Disable");
					} else
						btnDisable.Visible = false;
					btnInstall.Visible = allowInstall;
					btnUninstall.Visible = allowUninstall;
					btnUpdate.Visible = allowUpdate;
				}
			}
			else {
				headerBox.Hide ();
				btnDisable.Visible = false;
				btnInstall.Visible = false;
				btnUninstall.Visible = false;
				btnUpdate.Visible = false;
				eboxButs.Visible = false;
				labelDesc.Text = Catalog.GetString ("No selection");
			}
		}
		
		
		void ShowAddin (object data)
		{
			AddinHeader sinfo = null;
			Addin installed = null;
			AddinHeader updateInfo = null;
			string repo = "";
			string downloadSize = null;
			
			topHeaderBox.Hide ();
			
			if (data is Addin) {
				installed = (Addin) data;
				sinfo = SetupService.GetAddinHeader (installed);
				var entry = GetUpdate (installed);
				if (entry != null) {
					updateInfo = entry.Addin;
					selectedEntry.Add (entry);
				}
				foreach (var prop in sinfo.Properties) {
					if (prop.Name.StartsWith ("PreviewImage"))
						previewImages.Add (new ImageContainer (installed, prop.Value));
				}
				string icon32 = sinfo.Properties.GetPropertyValue ("Icon32");
				if (icon32.Length > 0)
					titleIcon = new ImageContainer (installed, icon32);
			}
			else if (data is AddinRepositoryEntry) {
				AddinRepositoryEntry entry = (AddinRepositoryEntry) data;
				sinfo = entry.Addin;
				installed = AddinManager.Registry.GetAddin (Addin.GetIdName (sinfo.Id));
				if (installed != null && Addin.CompareVersions (installed.Version, sinfo.Version) > 0)
					updateInfo = sinfo;
				selectedEntry.Add (entry);
				string rname = !string.IsNullOrEmpty (entry.RepositoryName) ? entry.RepositoryName : entry.RepositoryUrl;
				repo = "<span font='11'><b>" + Catalog.GetString ("Available in repository:") + "</b>\n" + GLib.Markup.EscapeText (rname) + "\n\n<span>";
				foreach (var prop in sinfo.Properties) {
					if (prop.Name.StartsWith ("PreviewImage"))
						previewImages.Add (new ImageContainer (entry, prop.Value));
				}
				string icon32 = sinfo.Properties.GetPropertyValue ("Icon32");
				if (icon32.Length > 0)
					titleIcon = new ImageContainer (entry, icon32);
				int size;
				if (int.TryParse (sinfo.Properties.GetPropertyValue ("DownloadSize"), out size)) {
					float fs = ((float)size) / 1048576f;
					downloadSize = fs.ToString ("0.00 MB");
				}
			} else
				selectedEntry.Clear ();
			
			if (installed != null)
				selectedAddin.Add (installed);
			
			string missingDepsTxt = null;
			
			if (sinfo == null) {
				btnDisable.Visible = false;
				btnUninstall.Visible = false;
				btnUpdate.Visible = false;
			} else {
				string version;
				string newVersion = null;
				if (installed != null) {
					btnInstall.Visible = false;
					btnUpdate.Visible = updateInfo != null && AllowInstall;
					btnDisable.Visible = true;
					btnDisable.Label = installed.Enabled ? Catalog.GetString ("Disable") : Catalog.GetString ("Enable");
					btnDisable.Visible = installed.Description.CanDisable;
					btnUninstall.Visible = installed.Description.CanUninstall;
					version = installed.Version;
					var missingDeps = Services.GetMissingDependencies (installed);
					if (updateInfo != null) {
						newVersion = updateInfo.Version;
						labelHeader.Markup = "<b><span color='black'>" + Catalog.GetString ("Update available") + "</span></b>";
//						topHeaderBox.BackgroundColor = new Gdk.Color (0, 132, 208);
						imageHeader.Pixbuf = Gdk.Pixbuf.LoadFromResource ("update-16.png");
						topHeaderBox.BackgroundColor = new Gdk.Color (255, 176, 0);
						topHeaderBox.Show ();
					}
					else if (missingDeps.Any ()) {
						labelHeader.Markup = "<b><span color='black'>" + Catalog.GetString ("This extension package can't be loaded due to missing dependencies") + "</span></b>";
						topHeaderBox.BackgroundColor = new Gdk.Color (255, 176, 0);
						imageHeader.SetFromStock (Gtk.Stock.DialogWarning, Gtk.IconSize.Menu);
						topHeaderBox.Show ();
						missingDepsTxt = "";
						foreach (var mdep in missingDeps) {
							if (mdep.Found != null)
								missingDepsTxt += "\n" + string.Format (Catalog.GetString ("Required: {0} v{1}, found v{2}"), mdep.Addin, mdep.Required, mdep.Found);
							else
								missingDepsTxt += "\n" + string.Format (Catalog.GetString ("Missing: {0} v{1}"), mdep.Addin, mdep.Required);
						}
					}
				} else {
					btnInstall.Visible = AllowInstall;
					btnUpdate.Visible = false;
					btnDisable.Visible = false;
					btnUninstall.Visible = false;
					version = sinfo.Version;
				}
				labelName.Markup = "<b><big>" + GLib.Markup.EscapeText(sinfo.Name) + "</big></b>";
				
				string ver;
				if (newVersion != null) {
					ver =  "<span font='11'><b>" + Catalog.GetString ("Installed version") + ":</b> " + version + "<span>\n";
					ver += "<span font='11'><b>" + Catalog.GetString ("Repository version") + ":</b> " + newVersion + "<span>";
				}
				else
					ver = "<span font='11'><b>" + Catalog.GetString ("Version") + " " + version + "</b><span>";
				
				if (downloadSize != null)
					ver += "\n<span font='11'><b>" + Catalog.GetString ("Download size") + ":</b> " + downloadSize + "<span>";
				if (missingDepsTxt != null)
					ver += "\n\n" + GLib.Markup.EscapeText (Catalog.GetString ("The following dependencies required by this extension package are not available:")) + missingDepsTxt;
				labelVersion.Markup = ver;
				
				string desc = GLib.Markup.EscapeText (sinfo.Description);
				labelDesc.Markup = repo + GLib.Markup.EscapeText (desc);
				
				foreach (var img in previewImages)
					vboxDesc.PackStart (img, false, false, 0);
				
				urlButton.Visible = !string.IsNullOrEmpty (sinfo.Url);
				infoUrl = sinfo.Url;
				
				if (titleIcon != null) {
					boxTitle.PackEnd (titleIcon, false, false, 0);
					labelName.WidthRequest = titleWidth - 32;
					labelVersion.WidthRequest = titleWidth - 32;
				} else {
					labelName.WidthRequest = titleWidth;
					labelVersion.WidthRequest = titleWidth;
				}
				
				if (IsRealized)
					SetComponentsBg ();
			}
		}
		
		public AddinRepositoryEntry GetUpdate (Addin a)
		{
			AddinRepositoryEntry[] updates = service.Repositories.GetAvailableAddinUpdates (Addin.GetIdName (a.Id));
			AddinRepositoryEntry best = null;
			string bestVersion = a.Version;
			foreach (AddinRepositoryEntry e in updates) {
				if (Addin.CompareVersions (bestVersion, e.Addin.Version) > 0) {
					best = e;
					bestVersion = e.Addin.Version;
				}
			}
			return best;
		}
		
		protected virtual void OnBtnInstallClicked (object sender, System.EventArgs e)
		{
			if (InstallClicked != null)
				InstallClicked (this, e);
		}
		
		protected virtual void OnBtnDisableClicked (object sender, System.EventArgs e)
		{
			if (EnableDisableClicked != null)
				EnableDisableClicked (this, e);
		}
		
		protected virtual void OnBtnUpdateClicked (object sender, System.EventArgs e)
		{
			if (UpdateClicked != null)
				UpdateClicked (this, e);
		}
		
		protected virtual void OnBtnUninstallClicked (object sender, System.EventArgs e)
		{
			if (UninstallClicked != null)
				UninstallClicked (this, e);
		}
		
		protected override void OnRealized ()
		{
			base.OnRealized ();
			HslColor gcol = ebox.Style.Background (Gtk.StateType.Normal);
			gcol.L -= 0.03;
			ebox.ModifyBg (Gtk.StateType.Normal, gcol);
			ebox2.ModifyBg (Gtk.StateType.Normal, gcol);
			scrolledwindow.ModifyBg (Gtk.StateType.Normal, gcol);
			SetComponentsBg ();
		}
		
		void SetComponentsBg ()
		{
			HslColor gcol = ebox.Style.Background (Gtk.StateType.Normal);
			//gcol.L -= 0.03;
			if (titleIcon != null)
				titleIcon.ModifyBg (Gtk.StateType.Normal, gcol);
			foreach (var i in previewImages)
				i.ModifyBg (Gtk.StateType.Normal, gcol);
		}
		
		protected virtual void OnUrlButtonClicked (object sender, System.EventArgs e)
		{
			System.Diagnostics.Process.Start (infoUrl);
		}
	}
	
	class ImageContainer: Gtk.EventBox
	{
		AddinRepositoryEntry aentry;
		IAsyncResult aresult;
		Gtk.Image image;
		bool destroyed;
		
		ImageContainer ()
		{
			image = new Gtk.Image ();
			Add (image);
			image.SetAlignment (0.5f, 0f);
			Show ();
		}
		
		public ImageContainer (AddinRepositoryEntry aentry, string fileName): this ()
		{
			this.aentry = aentry;
			aresult = aentry.BeginDownloadSupportFile (fileName, ImageDownloaded, null);
		}
		
		public ImageContainer (Addin addin, string fileName): this ()
		{
			string path = System.IO.Path.Combine (addin.Description.BasePath, fileName);
			LoadImage (File.OpenRead (path));
		}
		
		void ImageDownloaded (object state)
		{
			Gtk.Application.Invoke (delegate {
				if (destroyed)
					return;
				try {
					LoadImage (aentry.EndDownloadSupportFile (aresult));
				} catch {
					// ignore
				}
			});
		}
		
		void LoadImage (Stream s)
		{
			using (s) {
				Gdk.PixbufLoader loader = new Gdk.PixbufLoader (s);
				Gdk.Pixbuf pix = image.Pixbuf = loader.Pixbuf;
				loader.Dispose ();
				if (pix.Width > 250) {
					Gdk.Pixbuf spix = pix.ScaleSimple (250, (250 * pix.Height) / pix.Width, Gdk.InterpType.Hyper);
					pix.Dispose ();
					pix = spix;
				}
				image.Pixbuf = pix;
				image.Show ();
			}
		}
		
		protected override void OnDestroyed ()
		{
			destroyed = true;
			base.OnDestroyed ();
		}
	}
}

