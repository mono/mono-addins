//
// LocalizerTypeDescription.cs
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
using System.Collections.Specialized;
using System.Xml;
using Mono.Addins.Serialization;

namespace Mono.Addins.Description
{
	/// <summary>
	/// A localizer type definition.
	/// </summary>
	public sealed class LocalizerTypeDescription : ObjectDescription
	{
		string id;
		string typeName;
		string description;

		/// <summary>
		/// Initializes a new instance of the <see cref="LocalizerTypeDescription"/> class.
		/// </summary>
		public LocalizerTypeDescription ()
		{
		}

		internal LocalizerTypeDescription (XmlElement elem) : base (elem)
		{
			id = elem.GetAttribute ("id");
			typeName = elem.GetAttribute ("type");
			description = ReadXmlDescription ();
		}

		/// <summary>
		/// Copies data from another localizer type definition
		/// </summary>
		/// <param name='cond'>
		/// Condition from which to copy
		/// </param>
		public void CopyFrom (LocalizerTypeDescription cond)
		{
			id = cond.id;
			typeName = cond.typeName;
			description = cond.description;
		}

		internal override void Verify (string location, StringCollection errors)
		{
			VerifyNotEmpty (location + "LocalizerType", errors, Id, "id");
			VerifyNotEmpty (location + "LocalizerType (" + Id + ")", errors, TypeName, "type");
		}

		/// <summary>
		/// Gets or sets the identifier of the localizer type
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		public string Id {
			get { return id ?? string.Empty; }
			set { id = value; }
		}

		/// <summary>
		/// Gets or sets the name of the type that implements the localizer
		/// </summary>
		/// <value>
		/// The name of the type.
		/// </value>
		public string TypeName {
			get { return typeName ?? string.Empty; }
			set { typeName = value; }
		}

		/// <summary>
		/// Gets or sets the description of the localizer.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
		public string Description {
			get { return description ?? string.Empty; }
			set { description = value; }
		}

		internal override void SaveXml (XmlElement parent)
		{
			CreateElement (parent, "LocalizerType");
			Element.SetAttribute ("id", id);
			Element.SetAttribute ("type", typeName);
			SaveXmlDescription (description);
		}

		internal override void Write (BinaryXmlWriter writer)
		{
			writer.WriteValue ("Id", Id);
			writer.WriteValue ("TypeName", TypeName);
			writer.WriteValue ("Description", Description);
		}

		internal override void Read (BinaryXmlReader reader)
		{
			Id = reader.ReadStringValue ("Id");
			TypeName = reader.ReadStringValue ("TypeName");
			if (!reader.IgnoreDescriptionData)
				Description = reader.ReadStringValue ("Description");
		}
	}
}
