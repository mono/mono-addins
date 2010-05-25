using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mono.Addins;
using TextEditorSWF.ExtensionModel;
using TextEditorSWF;

[assembly: Addin ("SnippetsAddin","1.0", Namespace="TextEditor")]
[assembly: AddinDependency ("Core", "1.0")]

namespace SnippetsAddin
{
	[Extension]
	public class SnippetsExtension: EditorExtension
	{
		public override void Initialize ()
		{
			Program.MainWindow.Editor.KeyPress += new KeyPressEventHandler (EditorKeyPress);
		}

		void EditorKeyPress (object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar != '\t')
				return;
			RichTextBox editor = Program.MainWindow.Editor;
			int p = editor.SelectionStart - 1;
			string txt = editor.Text;
			while (p >= 0 && char.IsLetterOrDigit (txt[p]))
				p--;
			p++;
			string word = txt.Substring (p, editor.SelectionStart - p);

			foreach (ISnippetProvider provider in AddinManager.GetExtensionObjects <ISnippetProvider>()) {
				string fullText = provider.GetText (word);
				if (fullText != null) {
					int nextp;
					int cursorPos = fullText.IndexOf ("<|>");
					if (cursorPos != -1) {
						fullText = fullText.Remove (cursorPos, 3);
						nextp = p + cursorPos;
					}
					else
						nextp = p + fullText.Length;

					editor.Text = txt.Substring (0, p) + fullText + txt.Substring (editor.SelectionStart);
					editor.SelectionStart = nextp;
					e.Handled = true;
					return;
				}
			}
		}
	}
}
