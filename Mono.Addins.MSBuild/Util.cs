//
// Util.cs
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
using System.IO;

namespace Mono.Addins.MSBuild
{
	internal class Util
	{
		public static bool IsWindows {
			get { return Path.DirectorySeparatorChar == '\\'; }
		}

		public static string NormalizePath (string path)
		{
			if (path == null)
				return null;
			if (path.Length > 2 && path [0] == '[') {
				int i = path.IndexOf (']', 1);
				if (i != -1) {
					try {
						string fname = path.Substring (1, i - 1);
						Environment.SpecialFolder sf = (Environment.SpecialFolder)Enum.Parse (typeof (Environment.SpecialFolder), fname, true);
						path = Environment.GetFolderPath (sf) + path.Substring (i + 1);
					} catch {
						// Ignore
					}
				}
			}
			if (IsWindows)
				return path.Replace ('/', '\\');
			else
				return path.Replace ('\\', '/');
		}
	}
}
