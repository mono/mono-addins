//
// PackageCollection.cs
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
using System.Collections.Specialized;

namespace Mono.Addins.Setup
{
	/// <summary>
	/// A collection of packages
	/// </summary>
	public class PackageCollection: CollectionBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Mono.Addins.Setup.PackageCollection"/> class.
		/// </summary>
		public PackageCollection ()
		{
		}
		
		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="col">
		/// Collection where to copy from
		/// </param>
		public PackageCollection (ICollection col)
		{
			AddRange (col);
		}
		
		/// <summary>
		/// Gets a package
		/// </summary>
		/// <param name="n">
		/// Package index
		/// </param>
		public Package this [int n] {
			get { return (Package) List [n]; }
		}
		
		/// <summary>
		/// Adds a package
		/// </summary>
		/// <param name="p">
		/// A package
		/// </param>
		public void Add (Package p)
		{
			List.Add (p);
		}
		
		/// <summary>
		/// Checks if a package is present in the collection
		/// </summary>
		/// <param name="p">
		/// The package
		/// </param>
		/// <returns>
		/// True if the package is preent
		/// </returns>
		public bool Contains (Package p)
		{
			return List.Contains (p);
		}
		
		/// <summary>
		/// Adds a list of packages to the collection
		/// </summary>
		/// <param name="col">
		/// The list of packages to add
		/// </param>
		public void AddRange (ICollection col)
		{
			foreach (Package p in col)
				Add (p);
		}
	}
}
