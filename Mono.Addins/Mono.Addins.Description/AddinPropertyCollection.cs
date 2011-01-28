// 
// AddinPropertyCollection.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2011 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;

namespace Mono.Addins.Description
{
	public interface AddinPropertyCollection: IEnumerable<AddinProperty>
	{
		string GetPropertyValue (string name);
		string GetPropertyValue (string name, string locale);
		void SetPropertyValue (string name, string value);
		void SetPropertyValue (string name, string value, string locale);
		void RemoveProperty (string name);
		void RemoveProperty (string name, string locale);
	}
	
	class AddinPropertyCollectionImpl: List<AddinProperty>, AddinPropertyCollection
	{
		public string GetPropertyValue (string name)
		{
			return GetPropertyValue (name, null);
		}
		
		public string GetPropertyValue (string name, string locale)
		{
			AddinProperty best = null;
			foreach (var p in this) {
				if (p.Name == name) {
					if (best == null)
						best = p;
					else if (string.IsNullOrEmpty (p.Locale))
						best = p;
					if (p.Locale == locale)
						return p.Value;
				}
			}
			if (best != null)
				return best.Value;
			else
				return string.Empty;
		}
		
		public void SetPropertyValue (string name, string value)
		{
			SetPropertyValue (name, value, null);
		}
		
		public void SetPropertyValue (string name, string value, string locale)
		{
			foreach (var p in this) {
				if (p.Name == name && p.Locale == locale) {
					p.Value = value;
					return;
				}
			}
			AddinProperty prop = new AddinProperty ();
			prop.Name = name;
			prop.Value = value;
			prop.Locale = locale;
			Add (prop);
		}
		
		public void RemoveProperty (string name)
		{
			RemoveProperty (name, null);
		}
		
		public void RemoveProperty (string name, string locale)
		{
			foreach (var p in this) {
				if (p.Name == name && p.Locale == locale) {
					Remove (p);
					return;
				}
			}
		}
	}
}

