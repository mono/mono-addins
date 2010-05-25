using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TextEditorSWF.ExtensionModel;
using Mono.Addins;

namespace TextEditorSWF
{
	/// <summary>
	/// Manages commands, menus and toolbars
	/// </summary>
	static class CommandManager
	{
		/// <summary>
		/// Returns the list of items for the main menu
		/// </summary>
		public static IEnumerable<ToolStripItem> GetMainMenuItems ()
		{
			foreach (IUserInterfaceItem item in AddinManager.GetExtensionNodes ("/TextEditor/MainMenu"))
				yield return item.CreateMenuItem ();
		}

		/// <summary>
		/// Returns the list of items for the main toolbar
		/// </summary>
		public static IEnumerable<ToolStripItem> GetToolbarItems ()
		{
			foreach (IUserInterfaceItem item in AddinManager.GetExtensionNodes ("/TextEditor/Toolbar"))
				yield return item.CreateButton ();
		}

		/// <summary>
		/// Returns the extension node for the provided command identifier.
		/// </summary>
		internal static CommandExtensionNode GetCommand (string id)
		{
			foreach (CommandExtensionNode cmd in AddinManager.GetExtensionNodes (typeof (ICommand))) {
				if (cmd.Id == id)
					return cmd;
			}
			throw new InvalidOperationException ("Unknown command: " + id);
		}
	}
}
