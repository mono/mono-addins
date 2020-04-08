//
// AddinLocalizerAttribute.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2017 Xamarin Inc. (http://xamarin.com)
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

namespace Mono.Addins
{
	/// <summary>
	/// Declares a custom localizer for an add-in.
	/// </summary>
	[AttributeUsage (AttributeTargets.Assembly)]
	public class AddinLocalizerAttribute: Attribute
	{
		Type type;
		string typeName;

		/// <summary>
		/// Initializes a new instance of the <see cref="Mono.Addins.AddinLocalizerAttribute"/> class.
		/// </summary>
		public AddinLocalizerAttribute ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Mono.Addins.AddinLocalizerAttribute"/> class.
		/// </summary>
		/// <param name='type'>
		/// The type of the localizer. This type must implement the
		/// <see cref="Mono.Addins.Localization.IAddinLocalizerFactory"/> interface.
		/// </param>
		public AddinLocalizerAttribute (Type type)
		{
			Type = type;
		}

		/// <summary>
		/// Type of the localizer.
		/// </summary>
		public Type Type {
			get { return type; }
			set { type = value; typeName = value.AssemblyQualifiedName; }
		}

		internal string TypeName {
			get { return typeName; }
			set { typeName = value; type = null; }
		}
	}
}
