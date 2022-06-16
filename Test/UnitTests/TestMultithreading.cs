
using System;
using System.IO;
using NUnit.Framework;
using Mono.Addins;
using SimpleApp;
using System.Threading;
using System.Diagnostics;
using System.Linq;

namespace UnitTests
{
	[TestFixture ()]
	public class TestMultiThreading: TestBase
	{
		public override void Setup ()
		{
			base.Setup ();
			GlobalInfoCondition.Value = "res";
		}

		[Test]
		public void ChildConditionAttributeMultithread ()
		{
			int threads = 50;
			using var testData = new TestData(threads);

			GlobalInfoCondition.Value = "";

			testData.StartThreads (RunChildConditionAttributeTest);

			for (int n = 0; n < 20; n++) {
				Console.WriteLine ("Step " + n);
				testData.CheckCounters (0, 10000);

				GlobalInfoCondition.Value = "foo";

				testData.CheckCounters (1, 1000);

				GlobalInfoCondition.Value = "";
			}
		}

		void RunChildConditionAttributeTest (int index, TestData data)
		{
			while (!data.Stopped) {
				var writers = AddinManager.GetExtensionObjects<IWriter> ("/SimpleApp/ConditionedWriters");
				data.Counters [index] = writers.Length;
			}
		}

		[Test]
		public void EnableDisableMultithread ()
		{
			int threads = 50;
			int steps = 20;

			using var testData = new TestData (threads);

			testData.StartThreads ((index, data) => {
				while (!data.Stopped) {
					var writers = AddinManager.GetExtensionObjects<IWriter> ("/SimpleApp/Writers");
					testData.Counters [index] = writers.Length;
				}
			});

			for (int n = 0; n < steps; n++) {
				Console.WriteLine ("Step " + n);

				testData.CheckCounters (4, 10000);

				var ainfo1 = AddinManager.Registry.GetAddin ("SimpleApp.HelloWorldExtension");
				ainfo1.Enabled = false;

				testData.CheckCounters (3, 1000);

				var ainfo2 = AddinManager.Registry.GetAddin ("SimpleApp.FileContentExtension");
				ainfo2.Enabled = false;

				testData.CheckCounters (2, 1000);

				ainfo1.Enabled = true;
				ainfo2.Enabled = true;
			}
		}

		[Test]
		public void EventsMultithread ()
		{
			int threads = 50;
			int steps = 20;

			var testData = new TestData (threads);

			GlobalInfoCondition.Value = "";

			testData.StartThreads (RunEventsMultithread);

			for (int n = 0; n < steps; n++) {
				Console.WriteLine ("Step " + n);
				testData.CheckCounters (0, 10000);

				GlobalInfoCondition.Value = "foo";

				testData.CheckCounters (1, 1000);

				GlobalInfoCondition.Value = "";
			}
		}

		void RunEventsMultithread (int index, TestData data)
		{
			while (!data.Stopped) {
				int count = 0;
				ExtensionNodeEventHandler handler = (s, args) => {
					count++;
				};
				AddinManager.AddExtensionNodeHandler("/SimpleApp/ConditionedWriters", handler);
				data.Counters [index] = count;
				AddinManager.RemoveExtensionNodeHandler ("/SimpleApp/ConditionedWriters", handler);
			}
		}

		[Test]
		public void ChildConditionWithContextMultithread ()
		{
			int threads = 50;
			using var testData = new TestData (threads);

			GlobalInfoCondition.Value = "";

			testData.StartThreads (RunChildConditionWithContextMultithread);

			for (int n = 0; n < 20; n++) {
				Console.WriteLine ("Step " + n);
				testData.CheckCounters (0, 10000);

				GlobalInfoCondition.Value = "foo";

				testData.CheckCounters (1, 1000);

				GlobalInfoCondition.Value = "";
			}
		}

		void RunChildConditionWithContextMultithread (int index, TestData data)
		{
			var ctx = AddinManager.CreateExtensionContext ();
			var writers = ctx.GetExtensionNode ("/SimpleApp/ConditionedWriters");
			while (!data.Stopped) {
				data.Counters [index] = writers.ChildNodes.Count;
			}
		}
		class TestData: IDisposable
		{
			int numThreads;

			public bool Stopped;
			public int [] Counters;

			public TestData (int numThreads)
			{
				this.numThreads = numThreads;
				Counters = new int [numThreads];
			}

			public void CheckCounters (int expectedResult, int timeout)
			{
				var sw = Stopwatch.StartNew ();
				do {
					if (Counters.All (c => c == expectedResult))
						return;
					Thread.Sleep (10);
				} while (sw.ElapsedMilliseconds < timeout);

				Assert.Fail ();
			}

			public void Dispose ()
			{
				Stopped = true;
			}

			public void StartThreads (Action<int, TestData> threadAction)
			{
				for (int n = 0; n < numThreads; n++) {
					Counters [n] = -1;
					var index = n;
					var t = new Thread (() => threadAction(index, this));
					t.Start ();
				}
			}
		}
	}
}
