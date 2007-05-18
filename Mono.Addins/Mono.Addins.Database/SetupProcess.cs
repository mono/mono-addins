//
// SetupProcess.cs
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
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

namespace Mono.Addins.Database
{
	internal class SetupProcess
	{
		internal static void ExecuteCommand (IProgressStatus monitor, string registryPath, string startupDir, string name, params string[] args)
		{
			string asm = new Uri (typeof(SetupProcess).Assembly.CodeBase).LocalPath;
			string verboseParam = monitor.VerboseLog ? "v " : "nv";
			
			Process process = new Process ();
			if (Util.IsWindows)
				process.StartInfo = new ProcessStartInfo (asm, verboseParam + " " + name + " " + string.Join (" ", args));
			else
				process.StartInfo = new ProcessStartInfo ("mono", "--debug " + asm + " " + verboseParam + " " + name + " " + string.Join (" ", args));
			process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.EnableRaisingEvents = true;
			try {
				process.Start ();
			} catch (Exception ex) {
				Console.WriteLine (ex);
				throw;
			}
			
			process.StandardInput.WriteLine (registryPath);
			process.StandardInput.WriteLine (startupDir);
			process.StandardInput.Flush ();

//			string rr = process.StandardOutput.ReadToEnd ();
//			Console.WriteLine (rr);
			
			ProcessProgressStatus.MonitorProcessStatus (monitor, process.StandardOutput);
			process.WaitForExit ();
			if (process.ExitCode != 0)
				throw new ProcessFailedException ();
		}
		
		public static int Main (string[] args)
		{
			ProcessProgressStatus monitor = new ProcessProgressStatus (args[0] == "v");
			
			try {
				string registryPath = Console.In.ReadLine ();
				string startupDir = Console.In.ReadLine ();
				
				AddinDatabase.RunningSetupProcess = true;
				AddinRegistry reg = new AddinRegistry (registryPath, startupDir);
			
				switch (args [1]) {
				case "scan":
					reg.ScanFolders (monitor, args.Length > 2 ? args [2] : null);
					break;
				case "get-desc":
					reg.ParseAddin (monitor, args[2], args[3]);
					break;
				}
			} catch (Exception ex) {
				monitor.ReportError ("Unexpected error in setup process", ex);
				return 1;
			}
			return 0;
		}
	}
	
	class ProcessFailedException: Exception
	{
	}
}
