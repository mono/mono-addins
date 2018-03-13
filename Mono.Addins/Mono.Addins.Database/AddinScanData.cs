//
// AddinScanData.cs
//
// Author:
//       Lluis Sanchez <llsan@microsoft.com>
//
// Copyright (c) 2018 Microsoft
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
using System.Collections.Generic;
using System.IO;
using Mono.Addins.Serialization;

namespace Mono.Addins.Database
{
	class AddinScanDataIndex: IBinaryXmlElement
	{
		static BinaryXmlTypeMap typeMap = new BinaryXmlTypeMap (typeof (AddinScanDataIndex), typeof(AddinScanData));

		List<AddinScanData> files = new List<AddinScanData> ();
		List<string> assemblies = new List<string> ();

		string file;

		public static AddinScanDataIndex LoadFromFolder (IProgressStatus monitor, string path)
		{
			var file = Path.Combine (path, "dir.addindata");
			if (File.Exists (file)) {
				try {
					using (Stream s = File.OpenRead (file)) {
						BinaryXmlReader reader = new BinaryXmlReader (s, typeMap);
						reader.ContextData = file;
						return (AddinScanDataIndex)reader.ReadValue ("data");
					}
				} catch (Exception ex) {
					if (monitor != null)
						monitor.ReportError ("Could not load dir.addindata file", ex);
					// The addindata file is corrupted or changed format.
					// It is not useful anymore, so remove it
					try {
						File.Delete (file);
					} catch {
						// Ignore error deleting. Maybe there is a permission issue.
					}
				}
			}
			return null;
		}

		public void SaveToFolder (string path)
		{
			file = Path.Combine (path, "dir.addindata");
			using (Stream s = File.OpenWrite (file)) {
				var writter = new BinaryXmlWriter (s, typeMap);
				writter.WriteValue ("data", this);
			}
		}

		public void Delete ()
		{
			if (File.Exists (file))
				File.Delete (file);
		}

		void IBinaryXmlElement.Read (BinaryXmlReader reader)
		{
			file = (string) reader.ContextData;

			reader.ReadValue ("files", files);

			// Generate absolute paths

			var basePath = Path.GetDirectoryName (file);
			foreach (var f in files)
				f.FileName = Path.GetFullPath (Path.Combine (basePath, f.RelativeFileName));

			var asms = (string[])reader.ReadValue ("assemblies");

			// Generate absolute paths

			for (int n = 0; n < asms.Length; n++)
				asms [n] = Path.GetFullPath (Path.Combine (basePath, asms [n]));
			
			assemblies = new List<string> (asms);
		}

		void IBinaryXmlElement.Write (BinaryXmlWriter writer)
		{
			var basePath = Path.GetDirectoryName (file);

			// Store files as relative paths

			foreach (var f in files)
				f.RelativeFileName = Util.AbsoluteToRelativePath (basePath, f.FileName);
			
			writer.WriteValue ("files", files);

			// Store assemblies as relative paths

			var array = new string [assemblies.Count];
			for (int n = 0; n < assemblies.Count; n++)
				array [n] = Util.AbsoluteToRelativePath (basePath, assemblies [n]);
			
			writer.WriteValue ("assemblies", array);
		}

		public List<AddinScanData> Files {
			get { return files; }
		}

		public List<string> Assemblies {
			get { return assemblies; }
		}
	}

	class AddinScanData: IBinaryXmlElement
	{
		public string RelativeFileName { get; set; }
		public string FileName { get; set; }
		public string MD5 { get; set; }

		public AddinScanData ()
		{
		}

		public AddinScanData (string file, string md5)
		{
			FileName = file;
			MD5 = md5;
		}

		void IBinaryXmlElement.Read (BinaryXmlReader reader)
		{
			RelativeFileName = reader.ReadStringValue ("FileName");
			MD5 = reader.ReadStringValue ("MD5");
		}

		void IBinaryXmlElement.Write (BinaryXmlWriter writer)
		{
			writer.WriteValue ("FileName", RelativeFileName);
			writer.WriteValue ("MD5", MD5);
		}
	}
}
