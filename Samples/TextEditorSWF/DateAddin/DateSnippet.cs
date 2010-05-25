using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnippetsAddin;
using Mono.Addins;

[assembly: Addin]
[assembly: AddinDependency ("TextEditor.Core", "1.0")]
[assembly: AddinDependency ("TextEditor.SnippetsAddin", "1.0")]

namespace DateAddin
{
	[Extension]
	public class DateSnippet: ISnippetProvider
	{
		public string GetText (string shortcut)
		{
			if (shortcut == "date")
				return DateTime.Now.ToShortDateString ();
			else
				return null;
		}
	}
}
