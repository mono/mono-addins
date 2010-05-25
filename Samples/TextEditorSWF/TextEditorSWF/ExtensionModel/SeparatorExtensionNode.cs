using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mono.Addins;

namespace TextEditorSWF.ExtensionModel
{
	/// <summary>
	/// A menu or toolbar separator
	/// </summary>
	[ExtensionNode ("Separator")]
	class SeparatorExtensionNode : ExtensionNode, IUserInterfaceItem
	{
		public ToolStripItem CreateMenuItem ()
		{
			return new ToolStripSeparator ();
		}

		public ToolStripItem CreateButton ()
		{
			return new ToolStripSeparator ();
		}
	}
}
