using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextEditorSWF.ExtensionModel;

namespace TextEditorSWF.Commands
{
	/// <summary>
	/// The Save command.
	/// </summary>
	[Command ("Save", IconResource = "TextEditorSWF.Icons.save.png", Id = "Save")]
	class SaveCommand : ICommand
	{
		public void Run ()
		{
			Program.MainWindow.SaveFile ();
		}
	}
}
