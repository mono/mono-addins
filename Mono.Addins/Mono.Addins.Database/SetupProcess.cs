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
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Mono.Addins.Database
{
	class SetupProcess: ISetupHandler
	{
		public void Scan (IProgressStatus monitor, AddinRegistry registry, string scanFolder, ScanOptions context)
		{
			var data = new List<string> (context.Write ());

			ExecuteCommand (monitor, registry.RegistryPath, registry.StartupDirectory, registry.DefaultAddinsFolder, registry.AddinCachePath, "scan", scanFolder, data);
		}
		
		public void GenerateScanDataFiles (IProgressStatus monitor, AddinRegistry registry, string scanFolder, bool recursive)
		{
			ExecuteCommand (monitor, registry.RegistryPath, registry.StartupDirectory, registry.DefaultAddinsFolder, registry.AddinCachePath, "pre-scan", scanFolder, new List<string> { recursive.ToString() });
		}
		
		public void GetAddinDescription (IProgressStatus monitor, AddinRegistry registry, string file, string outFile)
		{
			ExecuteCommand (monitor, registry.RegistryPath, registry.StartupDirectory, registry.DefaultAddinsFolder, registry.AddinCachePath, "get-desc", file, new List<string> { outFile });
		}
		
		internal static void ExecuteCommand (IProgressStatus monitor, string registryPath, string startupDir, string addinsDir, string databaseDir, string name, string arg1, List<string> data)
		{
			string verboseParam = monitor.LogLevel.ToString ();
			
			// Arguments string
			StringBuilder sb = new StringBuilder ();
			sb.Append (verboseParam).Append (' ').Append (name);
			sb.Append (" \"").Append (arg1).Append ("\"");

			Process process = new Process ();

			string thisAsmDir = Path.GetDirectoryName (typeof (SetupProcess).Assembly.Location);
			string asm;
			if (Util.IsMono)
				asm = Path.Combine (thisAsmDir, "Mono.Addins.SetupProcess.exe");
			else
				asm = Path.Combine (thisAsmDir, "Mono.Addins.SetupProcess.dll");

			try {
				if (!Util.IsMono)
					process.StartInfo = new ProcessStartInfo (asm, sb.ToString ());
				else {
					asm = asm.Replace(" ", @"\ ");
					process.StartInfo = new ProcessStartInfo ("mono", "--debug " + asm + " " + sb.ToString ());
				}
				process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.RedirectStandardInput = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.EnableRaisingEvents = true;
				process.Start ();
			
				process.StandardInput.WriteLine (registryPath);
				process.StandardInput.WriteLine (startupDir);
				process.StandardInput.WriteLine (addinsDir);
				process.StandardInput.WriteLine (databaseDir);

				if (data != null) {
					foreach (var d in data)
						process.StandardInput.WriteLine (d);
				}
				process.StandardInput.Flush ();
	
				StringCollection progessLog = new StringCollection ();
				ProcessProgressStatus.MonitorProcessStatus (monitor, process.StandardOutput, progessLog);
				process.WaitForExit ();
				if (process.ExitCode != 0)
					throw new ProcessFailedException (progessLog);
				
			} catch (Exception ex) {
				Console.WriteLine (ex);
				throw;
			}
		}
	}
	
	class ProcessFailedException: Exception
	{
		StringCollection progessLog;
		
		public ProcessFailedException (StringCollection progessLog): this (progessLog, null)
		{
		}
		
		public ProcessFailedException (StringCollection progessLog, Exception ex): base ("Setup process failed.", ex)
		{
			this.progessLog = progessLog;
		}
		
		public StringCollection ProgessLog {
			get { return progessLog; }
		}
		
		public string LastLog {
			get { return progessLog.Count > 0 ? progessLog [progessLog.Count - 1] : ""; }
		}
	}
}
