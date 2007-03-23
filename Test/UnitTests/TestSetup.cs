
using System;
using System.IO;
using NUnit.Framework;
using Mono.Addins;
using SimpleApp;

namespace UnitTests
{
	[TestFixture]
	public class TestSetup
	{
		[Test]
		public void TestInitialize ()
		{
			AddinManager.Initialize (Path.GetDirectoryName (GetType().Assembly.Location));
			AddinManager.Registry.ResetConfiguration ();
			AddinManager.Registry.Update (new ConsoleProgressStatus (true));
		}
		
		[Test]
		public void TestShutdown ()
		{
			AddinManager.Shutdown ();
		}
	}
}
