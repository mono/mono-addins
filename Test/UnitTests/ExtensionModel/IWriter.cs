
using System;
using Mono.Addins;

[assembly: ExtensionPoint ("/SimpleApp/Writers")]

namespace SimpleApp
{
	public interface IWriter
	{
		string Id { get; }
		string Title { get; }
		string Write ();
		string Test (string test);
	}
}
