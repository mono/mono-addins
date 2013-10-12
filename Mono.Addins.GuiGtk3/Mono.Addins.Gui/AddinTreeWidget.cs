//
// AddinTreeWidget.cs
//
// Author:
//   Lluis Sanchez Gual
//   Robert Nordan <rpvn@robpvn.net> (Ported to GTK#3)
// 
// Copyright (c) 2007 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using Gtk;
using Gdk;
using Mono.Addins;
using Mono.Addins.Setup;
using Mono.Unix;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Mono.Addins.GuiGtk3
{
	public class AddinTreeWidget
	{
		protected Gtk.TreeView treeView;
		protected Gtk.TreeStore treeStore;
		bool allowSelection;
		ArrayList selected = new ArrayList ();
		Hashtable addinData = new Hashtable ();
		TreeViewColumn versionColumn;
		string filter;
		Dictionary<string,Gdk.Pixbuf> cachedIcons = new Dictionary<string, Gdk.Pixbuf> ();
		bool disposed;
		
		Gdk.Pixbuf iconInstalled;
		Gdk.Pixbuf updateOverlay;
		Gdk.Pixbuf installedOverlay;
		
		public event EventHandler SelectionChanged;
		
		const int ColAddin = 0;
		const int ColData = 1;
		const int ColName = 2;
		const int ColVersion = 3;
		const int ColAllowSelection = 4;
		const int ColSelected = 5;
		const int ColImage = 6;
		const int ColShowImage = 7;
		
		public AddinTreeWidget (Gtk.TreeView treeView)
		{
			iconInstalled = Gdk.Pixbuf.LoadFromResource ("plugin-32.png");
			updateOverlay = Gdk.Pixbuf.LoadFromResource ("software-update-available-overlay.png");
			installedOverlay = Gdk.Pixbuf.LoadFromResource ("installed-overlay.png");
			
			this.treeView = treeView;
			ArrayList list = new ArrayList ();
			AddStoreTypes (list);
			Type[] types = (Type[]) list.ToArray (typeof(Type));
			treeStore = new Gtk.TreeStore (types);
			treeView.Model = treeStore;
			CreateColumns ();
			ShowCategories = true;
			
			treeView.Destroyed += HandleTreeViewDestroyed;
		}

		void HandleTreeViewDestroyed (object sender, EventArgs e)
		{
			disposed = true;
			foreach (var px in cachedIcons.Values)
				if (px != null) px.Dispose ();
		}
		
		internal void SetFilter (string text)
		{
			this.filter = text;
		}
		
		internal void ShowEmptyMessage ()
		{
			treeStore.AppendValues (null, null, Catalog.GetString ("No add-ins found"), "", false, false, null, false);
		}
		
		protected virtual void AddStoreTypes (ArrayList list)
		{
			list.Add (typeof(object));
			list.Add (typeof(object));
			list.Add (typeof(string));
			list.Add (typeof(string));
			list.Add (typeof(bool));
			list.Add (typeof(bool));
			list.Add (typeof (Pixbuf));
			list.Add (typeof(bool));
		}
		
		protected virtual void CreateColumns ()
		{
			TreeViewColumn col = new TreeViewColumn ();
			col.Title = Catalog.GetString ("Add-in");
			
			CellRendererToggle crtog = new CellRendererToggle ();
			crtog.Activatable = true;
			crtog.Toggled += new ToggledHandler (OnAddinToggled);
			col.PackStart (crtog, false);
			
			CellRendererPixbuf pr = new CellRendererPixbuf ();
			col.PackStart (pr, false);
			col.AddAttribute (pr, "pixbuf", ColImage);
			col.AddAttribute (pr, "visible", ColShowImage);
			
			CellRendererText crt = new CellRendererText ();
			crt.Ellipsize = Pango.EllipsizeMode.End;
			col.PackStart (crt, true);
			
			col.AddAttribute (crt, "markup", ColName);
			col.AddAttribute (crtog, "visible", ColAllowSelection);
			col.AddAttribute (crtog, "active", ColSelected);
			col.Expand = true;
			treeView.AppendColumn (col);
			
			col = new TreeViewColumn ();
			col.Title = Catalog.GetString ("Version");
			col.PackStart (crt, true);
			col.AddAttribute (crt, "markup", ColVersion);
			versionColumn = col;
			treeView.AppendColumn (col);
		}
		
		public bool AllowSelection {
			get { return allowSelection; }
			set { allowSelection = value; }
		}
		
		public bool VersionVisible {
			get {
				return versionColumn.Visible;
			}
			set {
				versionColumn.Visible = value;
				treeView.HeadersVisible = value;
			}
		}
		
		public bool ShowCategories { get; set; }
		
		void OnAddinToggled (object o, ToggledArgs args)
		{
			TreeIter it;
			if (treeStore.GetIter (out it, new TreePath (args.Path))) {
				bool sel = !(bool) treeStore.GetValue (it, 5);
				treeStore.SetValue (it, 5, sel);
				AddinHeader info = (AddinHeader) treeStore.GetValue (it, 0);
				if (sel)
					selected.Add (info);
				else
					selected.Remove (info);

				OnSelectionChanged (EventArgs.Empty);
			}
		}
		
		protected virtual void OnSelectionChanged (EventArgs e)
		{
			if (SelectionChanged != null)
				SelectionChanged (this, e);
		}
		
		public void Clear ()
		{
			addinData.Clear ();
			selected.Clear ();
			treeStore.Clear ();
		}
		
		public TreeIter AddAddin (AddinHeader info, object dataItem, bool enabled)
		{
			return AddAddin (info, dataItem, enabled, true);
		}
		
		public TreeIter AddAddin (AddinHeader info, object dataItem, bool enabled, bool userDir)
		{
			return AddAddin (info, dataItem, enabled ? AddinStatus.Installed : AddinStatus.Disabled | AddinStatus.Installed);
		}
		
		public TreeIter AddAddin (AddinHeader info, object dataItem, AddinStatus status)
		{
			addinData [info] = dataItem;
			TreeIter iter;
			if (ShowCategories) {
				TreeIter piter = TreeIter.Zero;
				if (info.Category == "") {
					string otherCat = Catalog.GetString ("Other");
					piter = FindCategory (otherCat);
				} else {
					piter = FindCategory (info.Category);
				}
				iter = treeStore.AppendNode (piter);
			} else {
				iter = treeStore.AppendNode ();
			}
			UpdateRow (iter, info, dataItem, status);
			return iter;
		}
		
		protected virtual void UpdateRow (TreeIter iter, AddinHeader info, object dataItem, AddinStatus status)
		{
			bool sel = selected.Contains (info);
			
			treeStore.SetValue (iter, ColAddin, info);
			treeStore.SetValue (iter, ColData, dataItem);
			
			string name = EscapeWithFilterMarker (info.Name);
			if (!string.IsNullOrEmpty (info.Description)) {
				string desc = info.Description;
				int i = desc.IndexOf ('\n');
				if (i != -1)
					desc = desc.Substring (0, i);
				name += "\n<small><span foreground=\"darkgrey\">" + EscapeWithFilterMarker (desc) + "</span></small>";
			}
			
			if (status != AddinStatus.Disabled) {
				treeStore.SetValue (iter, ColName, name);
				treeStore.SetValue (iter, ColVersion, info.Version);
				treeStore.SetValue (iter, ColAllowSelection, allowSelection);
			}
			else {
				treeStore.SetValue (iter, ColName, "<span foreground=\"grey\">" + name + "</span>");
				treeStore.SetValue (iter, ColVersion, "<span foreground=\"grey\">" + info.Version + "</span>");
				treeStore.SetValue (iter, ColAllowSelection, false);
			}
			
			treeStore.SetValue (iter, ColShowImage, true);
			treeStore.SetValue (iter, ColSelected, sel);
			SetRowIcon (iter, info, dataItem, status);
		}
		
		void SetRowIcon (TreeIter it, AddinHeader info, object dataItem, AddinStatus status)
		{
			string customIcom = info.Properties.GetPropertyValue ("Icon32");
			string iconId = info.Id + " " + info.Version + " " + customIcom;
			Gdk.Pixbuf customPix;
			
			if (customIcom.Length == 0) {
				customPix = null;
				iconId = "__";
			}
			else if (!cachedIcons.TryGetValue (iconId, out customPix)) {
				
				if (dataItem is Addin) {
					string file = Path.Combine (((Addin)dataItem).Description.BasePath, customIcom);
					if (File.Exists (file)) {
						try {
							customPix = new Gdk.Pixbuf (file);
						} catch (Exception ex) {
							Console.WriteLine (ex);
						}
					}
					cachedIcons [iconId] = customPix;
				}
				else if (dataItem is AddinRepositoryEntry) {
					AddinRepositoryEntry arep = (AddinRepositoryEntry) dataItem;
					string tmpId = iconId;
					arep.BeginDownloadSupportFile (customIcom, delegate (IAsyncResult res) {
						Gtk.Application.Invoke (delegate {
							LoadRemoteIcon (it, tmpId, arep, res, info, dataItem, status);
						});
					}, null);
					iconId = "__";
				}
			}
			
			StoreIcon (it, iconId, customPix, status);
		}
		
		Gdk.Pixbuf GetCachedIcon (string id, string effect, Func<Gdk.Pixbuf> pixbufGenerator)
		{
			Gdk.Pixbuf pix;
			if (!cachedIcons.TryGetValue (id + "_" + effect, out pix))
				cachedIcons [id + "_" + effect] = pix = pixbufGenerator ();
			return pix;
		}
		
		internal bool ShowInstalledMarkers = false;
		
		void StoreIcon (TreeIter it, string iconId, Gdk.Pixbuf customPix, AddinStatus status)
		{
			if (customPix == null)
				customPix = iconInstalled;
			
			if ((status & AddinStatus.Installed) == 0) {
				treeStore.SetValue (it, ColImage, customPix);
				return;
			} else if (ShowInstalledMarkers && (status & AddinStatus.HasUpdate) == 0) {
				customPix = GetCachedIcon (iconId, "InstalledOverlay", delegate { return Services.AddIconOverlay (customPix, installedOverlay); });
				iconId = iconId + "_Installed";
			}
			
			if ((status & AddinStatus.Disabled) != 0) {
				customPix = GetCachedIcon (iconId, "Desaturate", delegate { return Services.DesaturateIcon (customPix); });
				iconId = iconId + "_Desaturate";
			}
			if ((status & AddinStatus.HasUpdate) != 0)
				customPix = GetCachedIcon (iconId, "UpdateOverlay", delegate { return Services.AddIconOverlay (customPix, updateOverlay); });

			treeStore.SetValue (it, ColImage, customPix);
		}

		
		void LoadRemoteIcon (TreeIter it, string iconId, AddinRepositoryEntry arep, IAsyncResult res, AddinHeader info, object dataItem, AddinStatus status)
		{
			if (!disposed && treeStore.IterIsValid (it)) {
				Gdk.Pixbuf customPix = null;
				try {
					Gdk.PixbufLoader loader = new Gdk.PixbufLoader (arep.EndDownloadSupportFile (res));
					customPix = loader.Pixbuf;
				} catch (Exception ex) {
					Console.WriteLine (ex);
				}
				cachedIcons [iconId] = customPix;
				StoreIcon (it, iconId, customPix, status);
			}
		}
		
		string EscapeWithFilterMarker (string txt)
		{
			if (string.IsNullOrEmpty (filter))
				return GLib.Markup.EscapeText (txt);
			
			StringBuilder sb = new StringBuilder ();
			int last = 0;
			int i = txt.IndexOf (filter, StringComparison.CurrentCultureIgnoreCase);
			while (i != -1) {
				sb.Append (GLib.Markup.EscapeText (txt.Substring (last, i - last)));
				sb.Append ("<span color='blue'>").Append (txt.Substring (i, filter.Length)).Append ("</span>");
				last = i + filter.Length;
				i = txt.IndexOf (filter, last, StringComparison.CurrentCultureIgnoreCase);
			}
			if (last < txt.Length)
				sb.Append (GLib.Markup.EscapeText (txt.Substring (last, txt.Length - last)));
			return sb.ToString ();
		}
		
		public object GetAddinData (AddinHeader info)
		{
			return addinData [info];
		}
		
		public AddinHeader[] GetSelectedAddins ()
		{
			return (AddinHeader[]) selected.ToArray (typeof(AddinHeader));
		}
		
		TreeIter FindCategory (string namePath)
		{
			TreeIter iter = TreeIter.Zero;
			string[] paths = namePath.Split ('/');
			foreach (string name in paths) {
				TreeIter child;
				if (!FindCategory (iter, name, out child)) {
					if (iter.Equals (TreeIter.Zero))
						iter = treeStore.AppendValues (null, null, name, "", false, false, null, false);
					else
						iter = treeStore.AppendValues (iter, null, null, name, "", false, false, null, false);
				}
				else
					iter = child;
			}
			return iter;
		}
		
		bool FindCategory (TreeIter piter, string name, out TreeIter child)
		{
			if (piter.Equals (TreeIter.Zero)) {
				if (!treeStore.GetIterFirst (out child))
					return false;
			}
			else if (!treeStore.IterChildren (out child, piter))
				return false;

			do {
				if (((string) treeStore.GetValue (child, ColName)) == name) {
					return true;
				}
			} while (treeStore.IterNext (ref child));

			return false;
		}
		
		public AddinHeader ActiveAddin {
			get {
				AddinHeader[] sel = ActiveAddins;
				if (sel.Length > 0)
					return sel[0];
				else
					return null;
			}
		}
		
		public AddinHeader[] ActiveAddins {
			get {
				List<AddinHeader> list = new List<AddinHeader> ();
				foreach (TreePath p in treeView.Selection.GetSelectedRows ()) {
					TreeIter iter;
					treeStore.GetIter (out iter, p);
					AddinHeader ah = (AddinHeader) treeStore.GetValue (iter, 0);
					if (ah != null)
						list.Add (ah);
				}
				return list.ToArray ();
			}
		}
		
		public object ActiveAddinData {
			get {
				AddinHeader ai = ActiveAddin;
				return ai != null ? GetAddinData (ai) : null;
			}
		}
		
		public object[] ActiveAddinsData {
			get {
				List<object> res = new List<object> ();
				foreach (AddinHeader ai in ActiveAddins) {
					res.Add (GetAddinData (ai));
				}
				return res.ToArray ();
			}
		}
		
		public object[] AddinsData {
			get {
				object[] data = new object [addinData.Count];
				addinData.Values.CopyTo (data, 0);
				return data;
			}
		}
		
		public object SaveStatus ()
		{
			TreeIter iter;
			ArrayList list = new ArrayList ();
			
			// Save the current selection
			list.Add (treeView.Selection.GetSelectedRows ());
			
			if (!treeStore.GetIterFirst (out iter))
				return null;
			
			// Save the expand state
			do {
				SaveStatus (list, iter);
			} while (treeStore.IterNext (ref iter));
			
			return list;
		}
		
		void SaveStatus (ArrayList list, TreeIter iter)
		{
			Gtk.TreePath path = treeStore.GetPath (iter);
			if (treeView.GetRowExpanded (path))
				list.Add (path);
			if (treeStore.IterChildren (out iter, iter)) {
				do {
					SaveStatus (list, iter);
				} while (treeStore.IterNext (ref iter));
			}
		}
		
		public void RestoreStatus (object ob)
		{
			if (ob == null)
				return;
				
			// The first element is the selection
			ArrayList list = (ArrayList) ob;
			TreePath[] selpaths = (TreePath[]) list [0];
			list.RemoveAt (0);
			
			foreach (TreePath path in list)
				treeView.ExpandRow (path, false);
			
			foreach (TreePath p in selpaths)
				treeView.Selection.SelectPath (p);
		}
		
		public void SelectAll ()
		{
			TreeIter iter;
			
			if (!treeStore.GetIterFirst (out iter))
				return;
			do {
				SelectAll (iter);
			} while (treeStore.IterNext (ref iter));
			OnSelectionChanged (EventArgs.Empty);
		}
		
		void SelectAll (TreeIter iter)
		{
			AddinHeader info = (AddinHeader) treeStore.GetValue (iter, ColAddin);
				
			if (info != null) {
				treeStore.SetValue (iter, ColSelected, true);
				if (!selected.Contains (info))
					selected.Add (info);
				treeView.ExpandToPath (treeStore.GetPath (iter));
			} else {
				if (treeStore.IterChildren (out iter, iter)) {
					do {
						SelectAll (iter);
					} while (treeStore.IterNext (ref iter));
				}
			}
		}
		
		public void UnselectAll ()
		{
			TreeIter iter;
			if (!treeStore.GetIterFirst (out iter))
				return;
			do {
				UnselectAll (iter);
			} while (treeStore.IterNext (ref iter));
			OnSelectionChanged (EventArgs.Empty);
		}
		
		void UnselectAll (TreeIter iter)
		{
			AddinHeader info = (AddinHeader) treeStore.GetValue (iter, ColAddin);
			if (info != null) {
				treeStore.SetValue (iter, ColSelected, false);
				selected.Remove (info);
			} else {
				if (treeStore.IterChildren (out iter, iter)) {
					do {
						UnselectAll (iter);
					} while (treeStore.IterNext (ref iter));
				}
			}
		}
	}
	
	[Flags]
	public enum AddinStatus
	{
		NotInstalled = 0,
		Installed = 1,
		Disabled = 2,
		HasUpdate = 4
	}
}
