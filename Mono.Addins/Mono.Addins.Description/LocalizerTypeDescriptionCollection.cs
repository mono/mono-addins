//
// LocalizerTypeDescriptionCollection.cs
//
// Author:
//       Marius Ungureanu <maungu@microsoft.com>
//
// Copyright (c) 2018 Microsoft Inc.
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
namespace Mono.Addins.Description
{
	/// <summary>
	/// A collection of localizer types
	/// </summary>
	public class LocalizerTypeDescriptionCollection : ObjectDescriptionCollection<LocalizerTypeDescription>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LocalizerTypeDescriptionCollection"/> class.
		/// </summary>
		public LocalizerTypeDescriptionCollection ()
		{
		}

		internal LocalizerTypeDescriptionCollection (object owner) : base (owner)
		{
		}

		/// <summary>
		/// Gets the <see cref="LocalizerTypeDescriptionCollection"/> at the specified index.
		/// </summary>
		/// <param name='n'>
		/// Index.
		/// </param>
		/// <returns>
		/// The condition.
		/// </returns>
		public LocalizerTypeDescription this [int n] {
			get { return (LocalizerTypeDescription)List [n]; }
		}
	}
}
