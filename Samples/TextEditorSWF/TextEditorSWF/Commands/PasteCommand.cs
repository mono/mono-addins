using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextEditorSWF.ExtensionModel;

namespace TextEditorSWF.Commands
{
	/// <summary>
	/// The Paste command.
	/// </summary>
	[Command ("Paste", IconResource = "TextEditorSWF.Icons.paste.png", Id = "Paste")]
	class PasteCommand : ICommand
	{
		public void Run ()
		{
			Program.MainWindow.Editor.Paste ();
		}
	}
}
