//
// AddinHostIndex.cs
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
using System.Collections;
using Mono.Addins.Serialization;

namespace Mono.Addins.Database
{
	class AddinHostIndex: IBinaryXmlElement
	{
		static BinaryXmlTypeMap typeMap = new BinaryXmlTypeMap (typeof(AddinHostIndex));
		
		Hashtable index = new Hashtable ();
		
		public void RegisterAssembly (string assemblyLocation, string addinId, string addinLocation)
		{
			index [Util.GetFullPath (assemblyLocation)] = addinId + " " + addinLocation;
		}
		
		public bool GetAddinForAssembly (string assemblyLocation, out string addinId, out string addinLocation)
		{
			string s = index [Util.GetFullPath (assemblyLocation)] as string;
			if (s == null) {
				addinId = null;
				addinLocation = null;
				return false;
			}
			else {
				int i = s.IndexOf (' ');
				addinId = s.Substring (0, i);
				addinLocation = s.Substring (i+1);
				return true;
			}
		}
		
		public void RemoveHostData (string addinId, string addinLocation)
		{
			string loc = addinId + " " + Util.GetFullPath (addinLocation);
			ArrayList todelete = new ArrayList ();
			foreach (DictionaryEntry e in index) {
				if (((string)e.Value) == loc)
					todelete.Add (e.Key);
			}
			foreach (string s in todelete)
				index.Remove (s);
		}
		
		public static AddinHostIndex Read (FileDatabase fileDatabase, string file)
		{
			return (AddinHostIndex) fileDatabase.ReadObject (file, typeMap);
		}
		
		public void Write (FileDatabase fileDatabase, string file)
		{
			fileDatabase.WriteObject (file, this, typeMap);
		}
		
		void IBinaryXmlElement.Write (BinaryXmlWriter writer)
		{
			writer.WriteValue ("index", index);
		}
		
		void IBinaryXmlElement.Read (BinaryXmlReader reader)
		{
			reader.ReadValue ("index", index);
		}
	}
}
