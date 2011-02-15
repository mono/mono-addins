//
// PackageRepositoryEntry.cs
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
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Mono.Addins.Setup
{
	internal class PackageRepositoryEntry: RepositoryEntry, AddinRepositoryEntry, IComparable
	{
		AddinInfo addin;
		
		public AddinInfo Addin {
			get { return addin; }
			set { addin = value; }
		}
		
		AddinHeader AddinRepositoryEntry.Addin {
			get { return addin; }
		}
		
		public string RepositoryUrl {
			get { return Repository.Url; }
		}
		
		public string RepositoryName {
			get { return Repository.Name; }
		}
		
		public int CompareTo (object other)
		{
			PackageRepositoryEntry rep = (PackageRepositoryEntry) other;
			string n1 = Mono.Addins.Addin.GetIdName (Addin.Id);
			string n2 = Mono.Addins.Addin.GetIdName (rep.Addin.Id);
			if (n1 != n2)
				return n1.CompareTo (n2);
			else
				return Mono.Addins.Addin.CompareVersions (rep.Addin.Version, Addin.Version);
		}
		
		public IAsyncResult BeginDownloadSupportFile (string name, AsyncCallback cb, object state)
		{
			return Repository.BeginDownloadSupportFile (name, cb, state);
		}
		
		public Stream EndDownloadSupportFile (IAsyncResult ares)
		{
			return Repository.EndDownloadSupportFile (ares);
		}
	}
	
	/// <summary>
	/// A reference to an add-in available in an on-line repository
	/// </summary>
	public interface AddinRepositoryEntry
	{
		/// <summary>
		/// Add-in information
		/// </summary>
		AddinHeader Addin {
			get;
		}
		
		/// <summary>
		/// Url to the add-in package
		/// </summary>
		string Url {
			get;
		}
		
		/// <summary>
		/// The URL of the repository
		/// </summary>
		string RepositoryUrl {
			get;
		}
		
		/// <summary>
		/// Name of the repository
		/// </summary>
		string RepositoryName {
			get;
		}
		
		/// <summary>
		/// Begins downloading a support file
		/// </summary>
		/// <returns>
		/// Result of the asynchronous operation, to be used when calling EndDownloadSupportFile to
		/// get the download result.
		/// </returns>
		/// <param name='name'>
		/// Name of the file.
		/// </param>
		/// <param name='cb'>
		/// Callback to be called when the download operation ends.
		/// </param>
		/// <param name='state'>
		/// Custom state object provided by the caller.
		/// </param>
		/// <remarks>
		/// This method can be used to get the contents of a support file of an add-in.
		/// A support file is a file referenced in the custom properties of an add-in.
		/// </remarks>
		IAsyncResult BeginDownloadSupportFile (string name, AsyncCallback cb, object state);
		
		/// <summary>
		/// Gets the result of the asynchronous download of a file
		/// </summary>
		/// <returns>
		/// The downloaded file.
		/// </returns>
		/// <param name='ares'>
		/// The async result object returned by BeginDownloadSupportFile.
		/// </param>
		Stream EndDownloadSupportFile (IAsyncResult ares);
	}
}
