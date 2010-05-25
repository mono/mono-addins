using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextEditorSWF.ExtensionModel;

namespace TextEditorSWF.Commands
{
	/// <summary>
	/// The Copy command
	/// </summary>
	[Command ("Copy", IconResource = "TextEditorSWF.Icons.copy.png", Id="Copy")]
	class CopyCommand : ICommand
	{
		public void Run ()
		{
			Program.MainWindow.Editor.Copy ();
		}
	}
}
