using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mono.Addins;

namespace TextEditorSWF.ExtensionModel
{
	/// <summary>
	/// A menu or submenu extension node. It can contain command items, separators and other submenus.
	/// </summary>
	[ExtensionNode ("Menu")]
	[ExtensionNodeChild (typeof (InterfaceItemExtensionNode))]
	[ExtensionNodeChild (typeof (SeparatorExtensionNode))]
	[ExtensionNodeChild (typeof (MenuExtensionNode))]
	class MenuExtensionNode : ExtensionNode, IUserInterfaceItem
	{
		[NodeAttribute ("label")]
		public string Label { get; set; }

		public ToolStripItem CreateMenuItem ()
		{
			ToolStripMenuItem menu = new ToolStripMenuItem (Label);
			foreach (IUserInterfaceItem item in ChildNodes)
				menu.DropDownItems.Add (item.CreateMenuItem ());
			return menu;
		}

		public ToolStripItem CreateButton ()
		{
			ToolStripDropDownButton menu = new ToolStripDropDownButton (Label);
			foreach (IUserInterfaceItem item in ChildNodes)
				menu.DropDownItems.Add (item.CreateMenuItem ());
			return menu;
		}
	}
}
