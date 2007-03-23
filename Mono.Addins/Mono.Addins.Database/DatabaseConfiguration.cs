//
// DatabaseConfiguration.cs
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
using System.IO;
using System.Collections.Specialized;
using System.Xml;

namespace Mono.Addins.Database
{
	internal class DatabaseConfiguration
	{
		public StringCollection DisabledAddins = new StringCollection ();
		
		public static DatabaseConfiguration Read (string file)
		{
			DatabaseConfiguration config = new DatabaseConfiguration ();
			
			StreamReader s = new StreamReader (file);
			using (s) {
				XmlTextReader tr = new XmlTextReader (s);
				tr.MoveToContent ();
				tr.ReadStartElement ("Configuration");
				tr.MoveToContent ();
				tr.ReadStartElement ("DisabledAddins");
				tr.MoveToContent ();
				if (!tr.IsEmptyElement) {
					while (tr.NodeType != XmlNodeType.EndElement) {
						if (tr.NodeType == XmlNodeType.Element) {
							if (tr.LocalName == "Addin")
								config.DisabledAddins.Add (tr.ReadElementString ());
						}
						else
							tr.Skip ();
						tr.MoveToContent ();
					}
				}
			}
			return config;
		}
		
		public void Write (string file)
		{
			StreamWriter s = new StreamWriter (file);
			using (s) {
				XmlTextWriter tw = new XmlTextWriter (s);
				tw.Formatting = Formatting.Indented;
				tw.WriteStartElement ("Configuration");
				tw.WriteStartElement ("DisabledAddins");
				foreach (string ad in DisabledAddins)
					tw.WriteElementString ("Addin", ad);
				tw.WriteEndElement ();
				tw.WriteEndElement ();
			}
		}
	}
}
