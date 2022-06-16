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
using System.IO;
using System.Collections.Generic;

namespace Mono.Addins.Database
{
	class AddinHostIndex: IBinaryXmlElement
	{
		static BinaryXmlTypeMap typeMap = new BinaryXmlTypeMap (typeof(AddinHostIndex));

		Dictionary<string, string> index;

		public AddinHostIndex ()
		{
			index = new Dictionary<string, string> ();
		}

		public AddinHostIndex (ImmutableAddinHostIndex immutableIndex)
		{
			this.index = immutableIndex.ToDictionary();
		}

		public ImmutableAddinHostIndex ToImmutableAddinHostIndex ()
		{
			return new ImmutableAddinHostIndex (new Dictionary<string, string> (index));
		}

		public void RegisterAssembly (string assemblyLocation, string addinId, string addinLocation, string domain)
		{
			assemblyLocation = NormalizeFileName (assemblyLocation);
			index [Path.GetFullPath (assemblyLocation)] = addinId + " " + addinLocation + " " + domain;
		}

		public bool GetAddinForAssembly (string assemblyLocation, out string addinId, out string addinLocation, out string domain)
		{
			return LookupAddinForAssembly (index, assemblyLocation, out addinId, out addinLocation, out domain);
		}

		internal static bool LookupAddinForAssembly (Dictionary<string, string> index, string assemblyLocation, out string addinId, out string addinLocation, out string domain)
		{
			assemblyLocation = NormalizeFileName (assemblyLocation);
			if (!index.TryGetValue(Path.GetFullPath (assemblyLocation), out var s)) {
				addinId = null;
				addinLocation = null;
				domain = null;
				return false;
			}
			else {
				int i = s.IndexOf (' ');
				int j = s.LastIndexOf (' ');
				addinId = s.Substring (0, i);
				addinLocation = s.Substring (i+1, j-i-1);
				domain = s.Substring (j+1);
				return true;
			}
		}
		
		public void RemoveHostData (string addinId, string addinLocation)
		{
			string loc = addinId + " " + Path.GetFullPath (addinLocation) + " ";
			ArrayList todelete = new ArrayList ();
			foreach (var e in index) {
				if (((string)e.Value).StartsWith (loc))
					todelete.Add (e.Key);
			}
			foreach (string s in todelete)
				index.Remove (s);
		}
		
		public static AddinHostIndex Read (FileDatabase fileDatabase, string file)
		{
			return (AddinHostIndex) fileDatabase.ReadObject (file, typeMap);
		}

		public static ImmutableAddinHostIndex ReadAsImmutable (FileDatabase fileDatabase, string file)
		{
			var hostIndex = (AddinHostIndex)fileDatabase.ReadObject (file, typeMap);
			return new ImmutableAddinHostIndex (hostIndex.index);
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
		
		internal static string NormalizeFileName (string name)
		{
			if (Util.IsWindows)
				return name.ToLower ();
			else
				return name;
		}
	}

	class ImmutableAddinHostIndex
	{
		Dictionary<string, string> index;

		public ImmutableAddinHostIndex () : this (new Dictionary<string, string> ())
		{
		}

		public ImmutableAddinHostIndex (Dictionary<string, string> index)
		{
			this.index = index;
		}

		public bool GetAddinForAssembly (string assemblyLocation, out string addinId, out string addinLocation, out string domain)
		{
			return AddinHostIndex.LookupAddinForAssembly (index, assemblyLocation, out addinId, out addinLocation, out domain);
		}

		public Dictionary<string, string> ToDictionary ()
		{
			return new Dictionary<string, string> (index);
		}
	}
}
