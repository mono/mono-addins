
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
			string asm = typeof(SetupProcess).Assembly.Location;
			string verboseParam = monitor.VerboseLog ? "v " : "nv";
			
			Process process = new Process ();
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
		}
		
		public static void Main (string[] args)
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
			}
		}
	}
}
