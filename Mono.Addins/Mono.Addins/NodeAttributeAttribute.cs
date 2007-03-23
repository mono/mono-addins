
using System;

namespace Mono.Addins
{
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Field, AllowMultiple=true)]
	public class NodeAttributeAttribute: Attribute
	{
		string name;
		bool required;
		Type type;
		string description;
		
		public NodeAttributeAttribute ()
		{
		}
		
		public NodeAttributeAttribute (string name)
			:this (name, false)
		{
		}
		
		public NodeAttributeAttribute (string name, bool required)
		{
			this.name = name;
			this.required = required;
		}
		
		public NodeAttributeAttribute (string name, Type type)
			: this (name, type, false)
		{
		}
		
		public NodeAttributeAttribute (string name, Type type, bool required)
		{
			this.name = name;
			this.type = type;
			this.required = required;
		}
		
		public string Name {
			get { return name != null ? name : string.Empty; }
			set { name = value; }
		}
		
		public bool Required {
			get { return required; }
			set { required = value; }
		}
		
		public Type Type {
			get { return type; }
			set { type = value; }
		}
		
		public string Description {
			get { return description != null ? description : string.Empty; }
			set { description = value; }
		}
	}
}
