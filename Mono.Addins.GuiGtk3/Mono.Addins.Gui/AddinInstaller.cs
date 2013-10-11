

using System;
using Mono.Addins.Setup;
using Mono.Unix;

namespace Mono.Addins.GuiGtk3
{
	public class AddinInstaller: IAddinInstaller
	{
		public void InstallAddins (AddinRegistry reg, string message, string[] addinIds)
		{
			Gtk.Builder builder = new Gtk.Builder (null, "Mono.Addins.GuiGtk3.interfaces.AddinInstallerDialog.ui", null);
			AddinInstallerDialog dlg = new AddinInstallerDialog (reg, message, addinIds, builder, builder.GetObject ("window1").Handle);
			try {
				if (dlg.Run () == (int) Gtk.ResponseType.Cancel)
					throw new InstallException (Catalog.GetString ("Installation cancelled"));
				else if (dlg.ErrMessage != null)
					throw new InstallException (dlg.ErrMessage);
			}
			finally {
				dlg.Destroy ();
			}
		}
	}
}
