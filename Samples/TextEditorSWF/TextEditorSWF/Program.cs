using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Mono.Addins;

// This is the main add-in root
[assembly:AddinRoot ("Core", "1.0", Namespace="TextEditor")]

namespace TextEditorSWF
{
	public static class Program
	{
		[STAThread]
		static void Main ()
		{
			// Initialize the add-in engine
			AddinManager.Initialize (".");
			AddinManager.Registry.Update ();

			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);
			MainWindow = new TextEditor ();
			MainWindow.Initialize ();
			Application.Run (MainWindow);
		}

		/// <summary>
		/// The main window of the text editor
		/// </summary>
		public static TextEditor MainWindow { get; private set; }
	}
}
