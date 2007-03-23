
using System;
using System.Collections;

namespace Mono.Addins.Serialization
{
	internal class BinaryXmlTypeMap
	{
		Hashtable types = new Hashtable ();
		Hashtable names = new Hashtable ();
		
		public BinaryXmlTypeMap ()
		{
		}
		
		public BinaryXmlTypeMap (params Type[] types)
		{
			foreach (Type t in types)
				RegisterType (t);
		}
		
		public void RegisterType (Type type)
		{
			RegisterType (type, type.Name);
		}
		
		public void RegisterType (Type type, string name)
		{
			names [type] = name;
			types [name] =  type;
		}
		
		public string GetTypeName (object ob)
		{
			string s = (string) names [ob.GetType ()];
			if (s == null)
				throw new InvalidOperationException ("Type not registered: " + ob.GetType ());
			return s;
		}
		
		public IBinaryXmlElement CreateObject (string typeName)
		{
			Type t = (Type) types [typeName];
			if (t == null)
				return null;
			return (IBinaryXmlElement) Activator.CreateInstance (t,true);
		}
	}
}
