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
		void SetPropertyValue (string name, string locale, string value);
		void RemoveProperty (string name, string locale);
	}
	
	class AddinPropertyCollectionImpl: List<AddinProperty>, AddinPropertyCollection
	{
		public string GetPropertyValue (string name)
		{
			return GetPropertyValue (name, System.Threading.Thread.CurrentThread.CurrentCulture.ToString ());
		}
		
		public string GetPropertyValue (string name, string locale)
		{
			locale = NormalizeLocale (locale);
			string lang = GetLocaleLang (locale);
			AddinProperty best = null;
			AddinProperty defaultLoc = null;
			foreach (var p in this) {
				if (p.Name == name) {
					if (p.Locale == locale)
						return p.Value;
					else if (GetLocaleLang (p.Locale) == lang)
						best = p;
					else if (p.Locale == null)
						defaultLoc = p;
				}
			}
			if (best != null)
				return best.Value;
			else if (defaultLoc != null)
				return defaultLoc.Value;
			else
				return string.Empty;
		}
		
		string NormalizeLocale (string loc)
		{
			if (string.IsNullOrEmpty (loc))
				return null;
			return loc.Replace ('_','-');
		}
		
		string GetLocaleLang (string loc)
		{
			if (loc == null)
				return null;
			int i = loc.IndexOf ('-');
			if (i != -1)
				return loc.Substring (0, i);
			else
				return loc;
		}
		
		public void SetPropertyValue (string name, string locale, string value)
		{
			if (string.IsNullOrEmpty (name))
				throw new ArgumentException ("name can't be null or empty");
			
			if (value == null)
				throw new ArgumentNullException ("value");
			
			locale = NormalizeLocale (locale);
			
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
		
		public void RemoveProperty (string name, string locale)
		{
			locale = NormalizeLocale (locale);
			
			foreach (var p in this) {
				if (p.Name == name && p.Locale == locale) {
					Remove (p);
					return;
				}
			}
		}
	}
}

