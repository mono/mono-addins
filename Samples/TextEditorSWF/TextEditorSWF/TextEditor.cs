using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TextEditorSWF.ExtensionModel;
using Mono.Addins;
using System.IO;

namespace TextEditorSWF
{
	public partial class TextEditor : Form
	{
		string currentFile;

		public TextEditor ()
		{
			InitializeComponent ();

			// Create the main menu and the toolbar

			foreach (ToolStripItem item in CommandManager.GetMainMenuItems ())
				menuStrip.Items.Add (item);
			foreach (ToolStripItem item in CommandManager.GetToolbarItems ())
				toolStrip.Items.Add (item);
		}

		internal void Initialize ()
		{
			// Initialize the editor extensions. Must be done after setting Program.MainWindow since
			// extensions may use it
			foreach (EditorExtension ext in AddinManager.GetExtensionObjects<EditorExtension> ())
				ext.Initialize ();
		}

		/// <summary>
		/// The editor box
		/// </summary>
		public RichTextBox Editor
		{
			get { return richTextBox; }
		}

		/// <summary>
		/// Saves the file to disk, asking for location if the file is unsaved
		/// </summary>
		public void SaveFile ()
		{
			if (currentFile == null) {
				SaveFileDialog dlg = new SaveFileDialog ();
				if (dlg.ShowDialog (this) != DialogResult.OK)
					return;
				currentFile = dlg.FileName;
				dlg.Dispose ();
			}
			SaveFile (currentFile);
		}

		/// <summary>
		/// Saves the file to the specified location
		/// </summary>
		public void SaveFile (string file)
		{
			File.WriteAllText (file, richTextBox.Text);

			// Notify editor extensions
			foreach (EditorExtension ext in AddinManager.GetExtensionObjects<EditorExtension> ())
				ext.OnSaveFile (file);
		}

		/// <summary>
		/// Create a new file
		/// </summary>
		public void NewFile ()
		{
			richTextBox.Text = "";
			currentFile = null;

			// Notify editor extensions
			foreach (EditorExtension ext in AddinManager.GetExtensionObjects<EditorExtension> ())
				ext.OnCreateFile ();
		}

		/// <summary>
		/// Open a new file. Will ask for the file name in a dialog.
		/// </summary>
		public void OpenFile ()
		{
			OpenFileDialog dlg = new OpenFileDialog ();
			if (dlg.ShowDialog () == DialogResult.OK)
				OpenFile (dlg.FileName);
			dlg.Dispose ();
		}

		/// <summary>
		/// Open the specified file in the text editor
		/// </summary>
		public void OpenFile (string file)
		{
			richTextBox.Text = File.ReadAllText (file);
			currentFile = file;

			// Notify editor extensions
			foreach (EditorExtension ext in AddinManager.GetExtensionObjects<EditorExtension> ())
				ext.OnLoadFile (file);
		}
	}
}
