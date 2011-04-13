// 
// InstallDialog.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2011 Novell, Inc (http://www.novell.com)
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
using Mono.Addins.Setup;
using Mono.Addins.Description;
using System.Text;
using Mono.Unix;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace Mono.Addins.Gui
{
	internal partial class InstallDialog : Gtk.Dialog
	{
		string[] filesToInstall;
		AddinRepositoryEntry[] addinsToInstall;
		PackageCollection packagesToInstall;
		SetupService service;
		Gtk.ResponseType response = Gtk.ResponseType.None;
		IEnumerable<string> uninstallIds;
		InstallMonitor installMonitor;
		bool installing;
		const int MaxHeight = 350;
		
		public InstallDialog (Gtk.Window parent, SetupService service)
		{
			this.Build ();
			this.service = service;
			TransientFor = parent;
			WindowPosition = Gtk.WindowPosition.CenterOnParent;
			Services.PlaceDialog (this, parent);
			boxProgress.Visible = false;
			Resizable = false;
		}
		
		public void InitForInstall (AddinRepositoryEntry[] addinsToInstall)
		{
			this.addinsToInstall = addinsToInstall;
			FillSummaryPage ();
			Services.PlaceDialog (this, TransientFor);
		}
		
		public void InitForInstall (string[] filesToInstall)
		{
			this.filesToInstall = filesToInstall;
			FillSummaryPage ();
			Services.PlaceDialog (this, TransientFor);
		}
		
		public void InitForUninstall (Addin[] info)
		{
			this.uninstallIds = info.Select (a => a.Id);
			buttonOk.Label = Catalog.GetString ("Uninstall");
			
			HashSet<Addin> sinfos = new HashSet<Addin> ();
			
			StringBuilder sb = new StringBuilder ();
			sb.Append ("<b>").Append (Catalog.GetString ("The following packages will be uninstalled:")).Append ("</b>\n\n");
			foreach (var a in info) {
				sb.Append (a.Name + "\n\n");
				sinfos.UnionWith (service.GetDependentAddins (a.Id, true));
			}
			
			if (sinfos.Count > 0) {
				sb.Append ("<b>").Append (Catalog.GetString ("There are other add-ins that depend on the previous ones which will also be uninstalled:")).Append ("</b>\n\n");
				foreach (Addin si in sinfos)
					sb.Append (si.Description.Name + "\n");
			}
			
			ShowMessage (sb.ToString ());
			Services.PlaceDialog (this, TransientFor);
		}
		
		void FillSummaryPage ()
		{
			PackageCollection packs = new PackageCollection ();
			
			if (filesToInstall != null) {
				foreach (string file in filesToInstall) {
					packs.Add (Package.FromFile (file));
				}
			}
			else {
				foreach (AddinRepositoryEntry arep in addinsToInstall) {
					packs.Add (Package.FromRepository (arep));
				}
			}
			
			packagesToInstall = new PackageCollection (packs);
			
			PackageCollection toUninstall;
			DependencyCollection unresolved;
			bool res;
			
			InstallMonitor m = new InstallMonitor ();
			res = service.ResolveDependencies (m, packs, out toUninstall, out unresolved);
			
			StringBuilder sb = new StringBuilder ();
			if (!res) {
				sb.Append ("<b><span foreground=\"red\">").Append (Catalog.GetString ("The selected add-ins can't be installed because there are dependency conflicts.")).Append ("</span></b>\n");
				foreach (string s in m.Errors) {
					sb.Append ("<b><span foreground=\"red\">" + s + "</span></b>\n");
				}
				sb.Append ("\n");
			}
			
			if (m.Warnings.Count != 0) {
				foreach (string w in m.Warnings) {
					sb.Append ("<b><span foreground=\"red\">" + w + "</span></b>\n");
				}
				sb.Append ("\n");
			}
			
			sb.Append ("<b>").Append (Catalog.GetString ("The following packages will be installed:")).Append ("</b>\n\n");
			foreach (Package p in packs) {
				sb.Append (p.Name);
				if (!p.SharedInstall)
					sb.Append (Catalog.GetString (" (in user directory)"));
				sb.Append ("\n");
			}
			sb.Append ("\n");
			
			if (toUninstall.Count > 0) {
				sb.Append ("<b>").Append (Catalog.GetString ("The following packages need to be uninstalled:")).Append ("</b>\n\n");
				foreach (Package p in toUninstall) {
					sb.Append (p.Name + "\n");
				}
				sb.Append ("\n");
			}
			
			if (unresolved.Count > 0) {
				sb.Append ("<b>").Append (Catalog.GetString ("The following dependencies could not be resolved:")).Append ("</b>\n\n");
				foreach (Dependency p in unresolved) {
					sb.Append (p.Name + "\n");
				}
				sb.Append ("\n");
			}
			buttonOk.Sensitive = res;
			ShowMessage (sb.ToString ());
		}
		
		void ShowMessage (string txt)
		{
			labelInfo.Markup = txt.TrimEnd ('\n','\t',' ');
			if (labelInfo.SizeRequest ().Height > MaxHeight) {
				scrolledwindow1.VscrollbarPolicy = Gtk.PolicyType.Automatic;
				scrolledwindow1.HeightRequest = MaxHeight;
			}
			else {
				scrolledwindow1.HeightRequest = labelInfo.SizeRequest ().Height;
			}
		}
		
		protected virtual void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			if (response != Gtk.ResponseType.None) {
				Respond (response);
				return;
			}
			Install ();
		}
		
		protected virtual void OnButtonCancelClicked (object sender, System.EventArgs e)
		{
			if (installing) {
				if (Services.AskQuestion (Catalog.GetString ("Are you sure you want to cancel the installation?")))
					installMonitor.Cancel ();
			} else
				Respond (Gtk.ResponseType.Cancel);
		}
		
		void Install ()
		{
			insSeparator.Visible = true;
			boxProgress.Visible = true;
			buttonOk.Sensitive = false;
			
			string txt;
			string errmessage;
			string warnmessage;
			
			ThreadStart oper;
				
			if (uninstallIds == null) {
				installMonitor = new InstallMonitor (globalProgressLabel, mainProgressBar, Catalog.GetString ("Installing Add-ins"));
				oper = new ThreadStart (RunInstall);
				errmessage = Catalog.GetString ("The installation failed!");
				warnmessage = Catalog.GetString ("The installation has completed with warnings.");
			} else {
				installMonitor = new InstallMonitor (globalProgressLabel, mainProgressBar, Catalog.GetString ("Uninstalling Add-ins"));
				oper = new ThreadStart (RunUninstall);
				errmessage = Catalog.GetString ("The uninstallation failed!");
				warnmessage = Catalog.GetString ("The uninstallation has completed with warnings.");
			}
			
			installing = true;
			oper ();
			installing = false;
			
			buttonCancel.Visible = false;
			buttonOk.Label = Gtk.Stock.Close;
			buttonOk.UseStock = true;
			
			if (installMonitor.Success && installMonitor.Warnings.Count == 0) {
				Respond (Gtk.ResponseType.Ok);
				return;
			} else if (installMonitor.Success) {
				txt = "<b>" + warnmessage + "</b>\n\n";
				foreach (string s in installMonitor.Warnings)
					txt += GLib.Markup.EscapeText (s) + "\n";
				response = Gtk.ResponseType.Ok;
				buttonOk.Sensitive = true;
			} else {
				buttonCancel.Label = Gtk.Stock.Close;
				buttonCancel.UseStock = true;
				txt = "<span foreground=\"red\"><b>" + errmessage + "</b></span>\n\n";
				foreach (string s in installMonitor.Errors)
					txt += GLib.Markup.EscapeText (s) + "\n";
				response = Gtk.ResponseType.Cancel;
				buttonOk.Sensitive = true;
			}
			
			ShowMessage (txt);
		}
		
		void RunInstall ()
		{
			try {
				if (filesToInstall != null)
					service.Install (installMonitor, filesToInstall);
				else
					service.Install (installMonitor, packagesToInstall);
			} catch (Exception ex) {
				installMonitor.Errors.Add (ex.Message);
			} finally {
				installMonitor.Dispose ();
			}
		}
		
		void RunUninstall ()
		{
			try {
				service.Uninstall (installMonitor, uninstallIds);
			} catch (Exception ex) {
				installMonitor.Errors.Add (ex.Message);
			} finally {
				installMonitor.Dispose ();
			}
		}
	}
}

