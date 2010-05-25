using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Addins;

namespace TextEditorSWF.ExtensionModel
{
	/// <summary>
	/// A user interface command.
	/// </summary>
	[TypeExtensionPoint (NodeType=typeof(CommandExtensionNode), ExtensionAttributeType=typeof(CommandAttribute))]
	public interface ICommand
	{
		/// <summary>
		/// Executes the command
		/// </summary>
		void Run ();
	}

	/// <summary>
	/// Attribute which can be used to declare new commands
	/// </summary>
	public class CommandAttribute : CustomExtensionAttribute
	{
		public CommandAttribute ()
		{
		}

		public CommandAttribute ([NodeAttribute ("Label")] string label)
		{
			Label = label;
		}

		/// <summary>
		/// Resource that holds the command icon
		/// </summary>
		[NodeAttribute]
		public string IconResource { get; set; }

		/// <summary>
		/// File that holds the command icon
		/// </summary>
		[NodeAttribute]
		public string IconFile { get; set; }

		/// <summary>
		/// Label of the command
		/// </summary>
		[NodeAttribute]
		public string Label { get; set; }
	}
}
