
using System;
using System.Xml;

namespace Mono.Addins
{
	[ExtensionNode ("Type")]
	public class TypeExtensionNode: ExtensionNode
	{
		string typeName;
		object cachedInstance;
		
		internal protected override void Read (NodeElement elem)
		{
			base.Read (elem);
			typeName = elem.GetAttribute ("type");
			if (typeName.Length == 0)
				typeName = elem.GetAttribute ("class");
			if (typeName.Length == 0)
				typeName = elem.GetAttribute ("id");
		}
		
		public object GetInstance (Type expectedType)
		{
			object ob = GetInstance ();
			if (!expectedType.IsInstanceOfType (ob))
				throw new InvalidOperationException (string.Format ("Expected subclass of type '{0}'. Found '{1}'.", expectedType, ob.GetType ()));
			return ob;
		}
		
		public object GetInstance ()
		{
			if (cachedInstance == null)
				cachedInstance = CreateInstance ();
			return cachedInstance;
		}
		
		public object CreateInstance (Type expectedType)
		{
			object ob = CreateInstance ();
			if (!expectedType.IsInstanceOfType (ob))
				throw new InvalidOperationException (string.Format ("Expected subclass of type '{0}'. Found '{1}'.", expectedType, ob.GetType ()));
			return ob;
		}
		
		public virtual object CreateInstance ()
		{
			if (typeName.Length == 0)
				throw new InvalidOperationException ("Type name not specified.");

			Type t = Addin.GetType (typeName, true);
			return Activator.CreateInstance (t);
		}
	}
}
