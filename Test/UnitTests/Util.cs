// 
// Util.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2011 Novell, Inc (http://www.novell.com)
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
using System.Xml;
using System.Text;
using System.Collections;
using System.IO;

namespace UnitTests
{
	public static class Util
	{
		static string rootDir;
		static int projectId;
		static bool tempDirClean;

		public static string TestsRootDir {
			get {
				if (rootDir == null)
					rootDir = Path.GetFullPath (Path.Combine (Path.GetDirectoryName (typeof(Util).Assembly.Location), "..", "..", "..", ".."));
				return rootDir;
			}
		}

		public static string TmpDir {
			get {
				var dir = Path.Combine (TestsRootDir, "tmp");
				if (!tempDirClean) {
					tempDirClean = true;
					ClearTmpDir ();
				}
				return dir;
			}
		}

		public static string Infoset (XmlNode nod)
		{
			StringBuilder sb = new StringBuilder ();
			GetInfoset (nod, sb);
			return sb.ToString ();
		}

		static void GetInfoset (XmlNode nod, StringBuilder sb)
		{
			switch (nod.NodeType) {
			case XmlNodeType.Document:
				GetInfoset (((XmlDocument)nod).DocumentElement, sb);
				break;
			case XmlNodeType.Attribute:
				if (nod.LocalName == "xmlns" && nod.NamespaceURI == "http://www.w3.org/2000/xmlns/") return;
				sb.Append (" " + nod.NamespaceURI + ":" + nod.LocalName + "='" + nod.Value + "'");
				break;

			case XmlNodeType.Element:
				XmlElement elem = (XmlElement) nod;
				sb.Append ("<" + elem.NamespaceURI + ":" + elem.LocalName);

				ArrayList ats = new ArrayList ();
				foreach (XmlAttribute at in elem.Attributes)
					ats.Add (at.LocalName + " " + at.NamespaceURI);

				ats.Sort ();

				foreach (string name in ats) {
					string[] nn = name.Split (' ');
					GetInfoset (elem.Attributes[nn[0], nn[1]], sb);
				}

				sb.Append (">");
				foreach (XmlNode cn in elem.ChildNodes)
					GetInfoset (cn, sb);
				sb.Append ("</>");
				break;

			default:
				sb.Append (nod.OuterXml);
				break;
			}
		}

		public static string GetSampleDirectory (string directoryName)
		{
			string srcDir = Path.Combine (TestsRootDir, "test-files", directoryName);
			string tmpDir = CreateTmpDir (Path.GetFileName (srcDir));
			CopyDir (srcDir, tmpDir);
			return tmpDir;
		}

		public static string CreateTmpDir (string hint)
		{
			string tmpDir = Path.Combine (TmpDir, hint + "-" + projectId.ToString ());
			projectId++;

			if (!Directory.Exists (tmpDir))
				Directory.CreateDirectory (tmpDir);
			return tmpDir;
		}

		public static void ClearTmpDir ()
		{
			if (Directory.Exists (TmpDir))
				Directory.Delete (TmpDir, true);
			projectId = 1;
		}

		static void CopyDir (string src, string dst)
		{
			if (!Directory.Exists (dst))
				Directory.CreateDirectory (dst);

			foreach (string file in Directory.GetFiles (src))
				File.Copy (file, Path.Combine (dst, Path.GetFileName (file)), overwrite: true);

			foreach (string dir in Directory.GetDirectories (src))
				CopyDir (dir, Path.Combine (dst, Path.GetFileName (dir)));
		}
	}
}

