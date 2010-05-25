using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextEditorSWF.ExtensionModel;
using TextEditorSWF;
using System.Windows.Forms;

namespace DateAddin
{
	[Command ("Insert Date")]
	class InsertDateCommand: ICommand
	{
		public void Run ()
		{
			Program.MainWindow.Editor.SelectedText = DateTime.Now.ToShortDateString ();
		}
	}
}
