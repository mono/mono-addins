
using System;
using System.Collections;

namespace Mono.Addins
{
	public class ExtensionNodeList: IEnumerable
	{
		internal ArrayList list;
		
		internal static ExtensionNodeList Empty = new ExtensionNodeList (new ArrayList ());
		
		internal ExtensionNodeList (ArrayList list)
		{
			this.list = list;
		}
		
		public ExtensionNode this [int n] {
			get {
				if (list == null)
					throw new System.IndexOutOfRangeException ();
				else
					return (ExtensionNode) list [n];
			}
		}
		
		public ExtensionNode this [string id] {
			get {
				if (list == null)
					return null;
				else {
					for (int n = list.Count - 1; n >= 0; n--)
						if (((ExtensionNode) list [n]).Id == id)
							return (ExtensionNode) list [n];
					return null;
				}
			}
		}
		
		public IEnumerator GetEnumerator () 
		{
			return list.GetEnumerator ();
		}
		
		public int Count {
			get { return list == null ? 0 : list.Count; }
		}
		
		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}
	}
}
