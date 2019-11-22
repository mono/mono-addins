//
// SetupProcessTool.cs
//
// Author:
//       Marius Ungureanu <maungu@microsoft.com>
//
// Copyright (c) 2019 Microsoft Inc.
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
using Mono.Addins.Database;

namespace Mono.Addins.SetupProcess
{
	class SetupProcessTool
	{
		public static int Main (string [] args)
		{
			ProcessProgressStatus monitor = new ProcessProgressStatus (int.Parse (args [0]));

			try {
				string registryPath = Console.In.ReadLine ();
				string startupDir = Console.In.ReadLine ();
				string addinsDir = Console.In.ReadLine ();
				string databaseDir = Console.In.ReadLine ();

				AddinDatabase.RunningSetupProcess = true;
				AddinRegistry reg = new AddinRegistry (registryPath, startupDir, addinsDir, databaseDir);

				switch (args [1]) {
				case "scan": {
						string folder = args.Length > 2 ? args [2] : null;
						if (folder.Length == 0) folder = null;

						var context = new ScanOptions ();
						context.Read (Console.In);
						reg.ScanFolders (monitor, folder, context);
						break;
					}
				case "pre-scan": {
						string folder = args.Length > 2 ? args [2] : null;
						if (folder.Length == 0) folder = null;
						var recursive = bool.Parse (Console.In.ReadLine ());
						reg.GenerateScanDataFilesInProcess (monitor, folder, recursive);
						break;
					}
				case "get-desc":
					var outFile = Console.In.ReadLine ();
					reg.ParseAddin (monitor, args [2], args [3]);
					break;
				}
			} catch (Exception ex) {
				monitor.ReportError ("Unexpected error in setup process", ex);
				return 1;
			}
			return 0;
		}
	}
}
