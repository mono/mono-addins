using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextEditorSWF.ExtensionModel;

namespace TextEditorSWF.Commands
{
	/// <summary>
	/// The cut command.
	/// </summary>
	[Command ("Cut", IconResource = "TextEditorSWF.Icons.cut.png", Id = "Cut")]
	class CutCommand : ICommand
	{
		public void Run ()
		{
			Program.MainWindow.Editor.Cut ();
		}
	}
}
