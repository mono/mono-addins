using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextEditorSWF.ExtensionModel;

namespace TextEditorSWF.Commands
{
	/// <summary>
	/// The New command.
	/// </summary>
	[Command ("New", IconResource = "TextEditorSWF.Icons.new.png", Id = "New")]
	class NewCommand : ICommand
	{
		public void Run ()
		{
			Program.MainWindow.NewFile ();
		}
	}
}
