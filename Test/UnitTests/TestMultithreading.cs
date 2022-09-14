
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

			// Threads check the number of nodes in /SimpleApp/ConditionedWriters, which changes
			// from 0 to 1 as the GlobalInfoCondition condition changes

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

		int EnableDisableStress_totalAdd;
		int EnableDisableStress_totalRemove;
		int EnableDisableStress_nodesCount;
		int EnableDisableStress_minCount;
		int EnableDisableStress_maxCount;

		[Test]
		public void EventsMultithread()
		{
			int threads = 50;

			int addinsLoaded = 0;
			int addinsUnloaded = 0;

			var node = AddinManager.GetExtensionNode("/SimpleApp/Writers");

			try
			{
				EnableDisableStress_nodesCount = 0;

				node.ExtensionNodeChanged += Node_ExtensionNodeChanged;

				Assert.AreEqual(4, EnableDisableStress_nodesCount);

				EnableDisableStress_minCount = 4;
				EnableDisableStress_maxCount = 4;
				EnableDisableStress_totalAdd = 0;
				EnableDisableStress_totalRemove = 0;

				var ainfo1 = AddinManager.Registry.GetAddin("SimpleApp.HelloWorldExtension");
				var ainfo2 = AddinManager.Registry.GetAddin("SimpleApp.FileContentExtension");

				AddinManager.AddinLoaded += (s,a) => addinsLoaded++;
				AddinManager.AddinUnloaded += (s,a) => addinsUnloaded++;

				using var enablers = new TestData(threads);

				enablers.StartThreads((index, data) =>
				{
					var random = new Random(10000 + index);
					while (!data.Stopped)
					{
						var action = random.Next(4);
						switch (action)
						{
							case 0: ainfo1.Enabled = false; break;
							case 1: ainfo1.Enabled = true; break;
							case 2: ainfo2.Enabled = false; break;
							case 3: ainfo2.Enabled = true; break;
						}
					}
				});
				Thread.Sleep(3000);
			}
			finally
			{
				node.ExtensionNodeChanged -= Node_ExtensionNodeChanged;
			}

			// If all events have been sent correctly, the node count should have never gone below 2 and over 4.

			Assert.That(EnableDisableStress_minCount, Is.AtLeast(2));
			Assert.That(EnableDisableStress_maxCount, Is.AtMost(4));

			// Every time one of these add-ins is enabled, a new node is added (likewise when removed), so
			// the total count of nodes added must match the number of times the addins were enabled.

			Assert.AreEqual(EnableDisableStress_totalAdd, addinsLoaded);
			Assert.AreEqual(EnableDisableStress_totalRemove, addinsUnloaded);
		}

        private void Node_ExtensionNodeChanged (object sender, ExtensionNodeEventArgs args)
        {
			if (args.Change == ExtensionChange.Add)
			{
				EnableDisableStress_nodesCount++;
				EnableDisableStress_totalAdd++;
			}
			else
			{
				EnableDisableStress_nodesCount--;
				EnableDisableStress_totalRemove++;
			}

			if (EnableDisableStress_nodesCount < EnableDisableStress_minCount)
				EnableDisableStress_minCount = EnableDisableStress_nodesCount;
			if (EnableDisableStress_nodesCount > EnableDisableStress_maxCount)
				EnableDisableStress_maxCount = EnableDisableStress_nodesCount;
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
				data.Counters [index] = writers.GetChildNodes().Count;
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

				Assert.Fail ("Expected " + expectedResult);
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
