using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextEditorSWF.ExtensionModel;

namespace TextEditorSWF.Commands
{
	/// <summary>
	/// The exit command.
	/// </summary>
	[Command ("Exit", Id = "Exit")]
	class ExitCommand : ICommand
	{
		public void Run ()
		{
			Environment.Exit (0);
		}
	}
}
