using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Addins;
using SnippetsAddin;

[assembly: ExtensionPoint ("/TextEditor/StockSnippets", ExtensionAttributeType = typeof (SnippetsAddin.SnippetAttribute))]

namespace SnippetsAddin
{
	[Extension]
	class StockSnippetProvider: ISnippetProvider
	{
		public string GetText (string shortcut)
		{
			foreach (ExtensionNode<SnippetAttribute> node in AddinManager.GetExtensionNodes ("/TextEditor/StockSnippets")) {
				if (node.Data.Shortcut == shortcut)
					return node.Data.Text;
			}
			return null;
		}
	}

	[AttributeUsage (AttributeTargets.Assembly, AllowMultiple=true)]
	public class SnippetAttribute : CustomExtensionAttribute
	{
		public SnippetAttribute ()
		{
		}

		public SnippetAttribute ([NodeAttribute ("Shortcut")] string shortcut, [NodeAttribute ("Text")] string text)
		{
			Shortcut = shortcut;
			Text = Text;
		}

		[NodeAttribute]
		public string Shortcut { get; set; }

		[NodeAttribute]
		public string Text { get; set; }
	}
}
