using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Addins;

namespace SnippetsAddin
{
	/// <summary>
	/// Extension point for snippet providers.
	/// </summary>
	[TypeExtensionPoint]
	public interface ISnippetProvider
	{
		string GetText (string shortcut);
	}
}
