using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Addins;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TextEditorSWF.ExtensionModel
{
	/// <summary>
	/// Extension node for command nodes
	/// </summary>
	class CommandExtensionNode : TypeExtensionNode<CommandAttribute>
	{
		Bitmap icon;

		/// <summary>
		/// Icon for the command (cached)
		/// </summary>
		public Bitmap Icon
		{
			get
			{
				if (icon == null && (Data.IconResource != null || Data.IconFile != null)) {
					Stream s;
					if (Data.IconResource != null)
						s = Addin.GetResource (Data.IconResource);
					else
						s = File.OpenRead (Addin.GetFilePath (Data.IconFile));
					using (s)
						icon = new Bitmap (s);
				}
				return icon;
			}
		}

		/// <summary>
		/// Returns a menu item for this command
		/// </summary>
		public ToolStripItem CreateMenuItem ()
		{
			ICommand cmd = (ICommand) CreateInstance (typeof (ICommand));
			return new ToolStripMenuItem (Data.Label, Icon, delegate {
				cmd.Run ();
			});
		}

		/// <summary>
		/// Returns a toolbar item for this command
		/// </summary>
		public ToolStripItem CreateButton ()
		{
			ICommand cmd = (ICommand) CreateInstance (typeof (ICommand));
			return new ToolStripButton (null, Icon, delegate {
				cmd.Run ();
			});
		}
	}
}
