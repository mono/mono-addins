//
// ObjectDescriptionCollection.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;

namespace Mono.Addins.Description
{
	public class ObjectDescriptionCollection: CollectionBase
	{
		public void Add (ObjectDescription ep)
		{
			List.Add (ep);
		}
		
		public void Remove (ObjectDescription ep)
		{
			List.Remove (ep);
		}
		
		public bool Contains (ObjectDescription ob)
		{
			return List.Contains (ob);
		}
		
		protected override void OnRemove (int index, object value)
		{
			ObjectDescription ep = (ObjectDescription) value;
			if (ep.Element != null) {
				ep.Element.ParentNode.RemoveChild (ep.Element);
				ep.Element = null;
			}
		}
		
		internal void SaveXml (XmlElement parent)
		{
			foreach (ObjectDescription ob in this)
				ob.SaveXml (parent);
		}
		
		internal void Verify (string location, StringCollection errors)
		{
			int n=0;
			foreach (ObjectDescription ob in this) {
				ob.Verify (location + "[" + n + "]/", errors);
				n++;
			}
		}
	}
}
