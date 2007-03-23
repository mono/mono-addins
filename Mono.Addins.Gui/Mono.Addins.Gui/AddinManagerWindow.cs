
using System;

namespace Mono.Addins.Gui
{
	public class AddinManagerWindow
	{
		private AddinManagerWindow()
		{
		}
		
		public static Gtk.Window Show (Gtk.Window parent)
		{
			AddinManagerDialog dlg = new AddinManagerDialog (parent);
			dlg.Show ();
			return dlg;
		}
		
		public static void Run (Gtk.Window parent)
		{
			using (AddinManagerDialog dlg = new AddinManagerDialog (parent)) {
				dlg.Run ();
			}
		}
	}
}
