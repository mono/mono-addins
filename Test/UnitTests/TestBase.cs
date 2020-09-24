
using System;
using System.IO;
using NUnit.Framework;
using Mono.Addins;
using SimpleApp;

namespace UnitTests
{
	public class TestBase
	{
		static bool firstRun = true;
		
		public static string TempDir {
			get {
				string dir = new Uri (typeof(TestBase).Assembly.CodeBase).LocalPath;
				return Path.Combine (Path.GetDirectoryName (dir), "temp");
			}
		}
		
		[OneTimeSetUp]
		public virtual void Setup ()
		{
			AddinManager.AddinLoadError += OnLoadError;
			AddinManager.AddinLoaded += OnLoad;
			AddinManager.AddinUnloaded += OnUnload;
			
			if (firstRun) {
				if (Directory.Exists (TempDir))
					Directory.Delete (TempDir, true);
				Directory.CreateDirectory (TempDir);
			}

			// Provide the current assembly as startup assembly, otherwise it will pick the
			// unit test runner as startup assembly

			AddinManager.AddinEngine.Initialize (GetType().Assembly, null, TempDir, null, null);
			
			if (firstRun)
				AddinManager.Registry.Update (new ConsoleProgressStatus (true));
			else
				AddinManager.Registry.ResetConfiguration ();
			
			firstRun = false;
		}
		
		[OneTimeTearDown]
		public virtual void Teardown ()
		{
			AddinManager.AddinLoadError -= OnLoadError;
			AddinManager.AddinLoaded -= OnLoad;
			AddinManager.AddinUnloaded -= OnUnload;
			AddinManager.Shutdown ();
		}
		
		void OnLoadError (object s, AddinErrorEventArgs args)
		{
			Console.WriteLine ("Add-in error (" + args.AddinId + "): " + args.Message);
			Console.WriteLine (args.Exception);
		}
		
		void OnLoad (object s, AddinEventArgs args)
		{
			Console.WriteLine ("Add-in loaded: " + args.AddinId);
		}
		
		void OnUnload (object s, AddinEventArgs args)
		{
			Console.WriteLine ("Add-in unloaded: " + args.AddinId);
		}
	}
}
