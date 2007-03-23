
using System;

namespace Mono.Addins
{
	public interface NodeElement
	{
		string NodeName { get; }
		string GetAttribute (string key);
		NodeAttribute[] Attributes { get; }
	}
	
	public class NodeAttribute
	{
		internal string name;
		internal string value;
		
		internal NodeAttribute ()
		{
		}
		
		public string Name {
			get { return name; }
		}
		
		public string Value {
			get { return value; }
		}
	}
}
