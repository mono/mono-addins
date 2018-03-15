//
// RepositoryRecord.cs
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
using System.IO;
using System.Xml;
using System.Xml.Serialization;


namespace Mono.Addins.Setup
{
	internal class RepositoryRecord: AddinRepository
	{
		string id;
		bool isReference;
		string file;
		string url;
		string name;
		bool enabled = true;
		DateTime lastModified = new DateTime (1900,1,1);
		
		[XmlAttribute ("id")]
		public string Id {
			get { return id; }
			set { id = value; }
		}

		public string ProviderId { get; set; }
		
		public bool IsReference {
			get { return isReference; }
			set { isReference = value; }
		}
		
		public string File {
			get { return file; }
			set { file = value; }
		}
		
		public string CachedFilesDir {
			get {
				return Path.Combine (Path.GetDirectoryName (File), Path.GetFileNameWithoutExtension (File) + "_files");
			}
		}
		
		public string Url {
			get { return url; }
			set { url = value; }
		}
		
		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		public string Title {
			get { return Name != null && Name != "" ? Name : Url; }
		}
				
		public DateTime LastModified {
			get { return lastModified; }
			set { lastModified = value; }
		}
		
		[System.ComponentModel.DefaultValue (true)]
		public bool Enabled {
			get { return this.enabled; }
			set { enabled = value; }
		}
		
		public Repository GetCachedRepository ()
		{
			Repository repo = (Repository) AddinStore.ReadObject (File, typeof(Repository));
			if (repo != null)
				repo.CachedFilesDir = CachedFilesDir;
			return repo;
		}
		
		public void ClearCachedRepository ()
		{
			if (System.IO.File.Exists (File))
				System.IO.File.Delete (File);
			if (Directory.Exists (CachedFilesDir))
				Directory.Delete (CachedFilesDir, true);
		}
		
		internal void UpdateCachedRepository (Repository newRep)
		{
			newRep.url = Url;
			if (newRep.Name == null)
				newRep.Name = new Uri (Url).Host;
			AddinStore.WriteObject (File, newRep);
			if (name == null)
				name = newRep.Name;
			newRep.CachedFilesDir = CachedFilesDir;
		}
	}

	/// <summary>
	/// An on-line add-in repository
	/// </summary>
	public interface AddinRepository
	{
		/// <summary>
		/// Path to the cached add-in repository file
		/// </summary>
		string File {
			get;
		}
		
		/// <summary>
		/// Url of the repository
		/// </summary>
		string Url {
			get;
		}
		
		/// <summary>
		/// Do not use. Use Title instead.
		/// </summary>
		string Name {
			get;
			set;
		}
		
		/// <summary>
		/// Title of the repository
		/// </summary>
		string Title {
			get;
		}
		
		/// <summary>
		/// Last change timestamp
		/// </summary>
		DateTime LastModified {
			get;
		}
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="Mono.Addins.Setup.AddinRepository"/> is enabled.
		/// </summary>
		/// <value>
		/// <c>true</c> if enabled; otherwise, <c>false</c>.
		/// </value>
		bool Enabled {
			get;
		}

		/// <summary>
		/// Defineds type of repository provider.
		/// </summary>
		/// <value>Provider string id.</value>
		string ProviderId {
			get;
		}
	}
}
