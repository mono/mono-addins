using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextEditorSWF.ExtensionModel;

namespace TextEditorSWF.Commands
{
	/// <summary>
	/// The Open command.
	/// </summary>
	[Command ("Open", IconResource = "TextEditorSWF.Icons.open.png", Id = "Open")]
	class OpenCommand : ICommand
	{
		public void Run ()
		{
			Program.MainWindow.OpenFile ();
		}
	}
}
