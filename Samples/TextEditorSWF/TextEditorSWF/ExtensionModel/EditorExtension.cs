using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mono.Addins;

namespace TextEditorSWF.ExtensionModel
{
	/// <summary>
	/// Add-ins can create subclasses of EditorExtension to intercept some
	/// text editor events, such as saving a file, loading, etc.
	/// </summary>
	[TypeExtensionPoint]
	public class EditorExtension
	{
		/// <summary>
		/// Called when the text editor is initialized
		/// </summary>
		public virtual void Initialize ()
		{
		}

		/// <summary>
		/// Called when a file is loaded
		/// </summary>
		public virtual void OnLoadFile (string file)
		{
		}

		/// <summary>
		/// Called when the current file is saved
		/// </summary>
		public virtual void OnSaveFile (string file)
		{
		}

		/// <summary>
		/// Called when a new file is created
		/// </summary>
		public virtual void OnCreateFile ()
		{
		}
	}
}
