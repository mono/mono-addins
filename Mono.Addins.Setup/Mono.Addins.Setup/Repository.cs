//
// Repository.cs
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
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Mono.Addins.Setup
{
	internal class Repository
	{
		RepositoryEntryCollection repositories;
		RepositoryEntryCollection addins;
		string name;
		internal string url;
		
		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		public string Url {
			get { return url; }
			set { url = value; }
		}
		
		internal string CachedFilesDir { get; set; }
	
		[XmlElement ("Repository", Type = typeof(ReferenceRepositoryEntry))]
		public RepositoryEntryCollection Repositories {
			get {
				if (repositories == null)	
					repositories = new RepositoryEntryCollection (this);
				return repositories;
			}
		}
	
		[XmlElement ("Addin", Type = typeof(PackageRepositoryEntry))]
		public RepositoryEntryCollection Addins {
			get {
				if (addins == null)
					addins = new RepositoryEntryCollection (this);
				return addins;
			}
		}
		
		public RepositoryEntry FindEntry (string url)
		{
			if (Repositories != null) {
				foreach (RepositoryEntry e in Repositories)
					if (e.Url == url) return e;
			}
			if (Addins != null) {
				foreach (RepositoryEntry e in Addins)
					if (e.Url == url) return e;
			}
			return null;
		}
		
		public void AddEntry (RepositoryEntry entry)
		{
			entry.owner = this;
			if (entry is ReferenceRepositoryEntry) {
				Repositories.Add (entry);
			} else {
				Addins.Add (entry);
			}
		}
		
		public void RemoveEntry (RepositoryEntry entry)
		{
			if (entry is PackageRepositoryEntry)
				Addins.Remove (entry);
			else
				Repositories.Remove (entry);
		}
		
		public IAsyncResult BeginDownloadSupportFile (string name, AsyncCallback cb, object state)
		{
			FileAsyncResult res = new FileAsyncResult ();
			res.AsyncState = state;
			res.Callback = cb;
			
			string cachedFile = Path.Combine (CachedFilesDir, Path.GetFileName (name));
			if (File.Exists (cachedFile)) {
				res.FilePath = cachedFile;
				res.CompletedSynchronously = true;
				res.SetDone ();
				return res;
			}
			
			Uri u = new Uri (new Uri (Url), name);
			if (u.Scheme == "file") {
				res.FilePath = u.AbsolutePath;
				res.CompletedSynchronously = true;
				res.SetDone ();
				return res;
			}

			res.FilePath = cachedFile;
			WebRequestHelper.GetResponseAsync (() => (HttpWebRequest)WebRequest.Create (u)).ContinueWith (t => {
				try {
					var resp = t.Result;
					string dir = Path.GetDirectoryName (res.FilePath);
					lock (this) {
						if (!Directory.Exists (dir))
							Directory.CreateDirectory (dir);
					}
					byte[] buffer = new byte [8092];
					using (var s = resp.GetResponseStream ()) {
						using (var f = File.OpenWrite (res.FilePath)) {
							int nr = 0;
							while ((nr = s.Read (buffer, 0, buffer.Length)) > 0)
								f.Write (buffer, 0, nr);
						}
					}
				} catch (Exception ex) {
					res.Error = ex;
				}
			});
			return res;
		}
		
		public Stream EndDownloadSupportFile (IAsyncResult ares)
		{
			FileAsyncResult res = ares as FileAsyncResult;
			if (res == null)
				throw new InvalidOperationException ("Invalid IAsyncResult instance");
			if (res.Error != null)
				throw res.Error;
			return File.OpenRead (res.FilePath);
		}
	}

	class FileAsyncResult: IAsyncResult
	{
		ManualResetEvent done;
		
		public string FilePath;
		public AsyncCallback Callback;
		public Exception Error;
		
		public void SetDone ()
		{
			lock (this) {
				IsCompleted = true;
				if (done != null)
					done.Set ();
			}
			if (Callback != null)
				Callback (this);
		}
		
		public object AsyncState { get; set; }
	
		public WaitHandle AsyncWaitHandle {
			get {
				lock (this) {
					if (done == null)
						done = new ManualResetEvent (IsCompleted);
				}
				return done;
			}
		}
	
		public bool CompletedSynchronously { get; set; }
	
		public bool IsCompleted { get; set; }
	}
	
}
