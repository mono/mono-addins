
using System;
using System.IO;
using NUnit.Framework;
using Mono.Addins;
using SimpleApp;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

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

				testData.CheckCounters (1, 10000);

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
					LoadAll(AddinManager.GetExtensionNodes<ItemSetNode>("/SimpleApp/ItemTree"));
					var writers = AddinManager.GetExtensionObjects<IWriter> ("/SimpleApp/Writers");
					testData.Counters [index] = writers.Length;
				}
				testData.Counters[index] = 0;
			});

			for (int n = 0; n < steps; n++) {
				Console.WriteLine ("Step " + n);

				testData.CheckCounters (4, 10000);

				var ainfo1 = AddinManager.Registry.GetAddin ("SimpleApp.HelloWorldExtension");
				ainfo1.Enabled = false;

				testData.CheckCounters (3, 10000);

				var ainfo2 = AddinManager.Registry.GetAddin ("SimpleApp.FileContentExtension");
				ainfo2.Enabled = false;

				testData.CheckCounters (2, 10000);

				ainfo1.Enabled = true;
				ainfo2.Enabled = true;
			}

			testData.Stopped = true;
			testData.CheckCounters(0, 10000);
		}

		void LoadAll(IEnumerable<ExtensionNode> nodes)
		{
			foreach (var n in nodes.OfType<ItemSetNode>())
				LoadAll(n.GetChildNodes());
		}

		[Test]
		public void EventsMultithread()
		{
			int threads = 50;

			int totalAdded = 0;
			int totalRemoved = 0;
			int nodesCount = 0;
			int minCount = 0;
			int maxCount = 0;

			var node = AddinManager.GetExtensionNode("/SimpleApp/Writers");

			nodesCount = 0;

			node.ExtensionNodeChanged += (s, args) =>
			{
				if (args.Change == ExtensionChange.Add)
				{
					nodesCount++;
					totalAdded++;
				}
				else
				{
					nodesCount--;
					totalRemoved++;
				}

				if (nodesCount < minCount)
					minCount = nodesCount;
				if (nodesCount > maxCount)
					maxCount = nodesCount;
			};

			Assert.AreEqual(4, nodesCount);

			minCount = 4;
			maxCount = 4;
			totalAdded = 0;
			totalRemoved = 0;

			var ainfo1 = AddinManager.Registry.GetAddin("SimpleApp.HelloWorldExtension");
			var ainfo2 = AddinManager.Registry.GetAddin("SimpleApp.FileContentExtension");

			using var enablers = new TestData(threads);

			enablers.StartThreads((index, data) =>
			{
				var random = new Random(10000 + index);
				int iterations = 100;
				while (--iterations > 0)
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
				data.Counters[index] = 1;
			});

			// Wait for the threads to do the work. 5 seconds should be enough
			enablers.CheckCounters(1, 100000);

			// Go back to the initial status

			ainfo1.Enabled = true;
			ainfo2.Enabled = true;

			// If all events have been sent correctly, the node count should have never gone below 2 and over 4.

			Assert.That(nodesCount, Is.EqualTo(4));
			Assert.That(totalAdded, Is.AtLeast(100));
			Assert.That(totalAdded, Is.EqualTo(totalRemoved));
			Assert.That(minCount, Is.AtLeast(2));
			Assert.That(maxCount, Is.AtMost(4));
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

				testData.CheckCounters (1, 10000);

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
