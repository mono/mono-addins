//
// ExtensionContext.cs
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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using static Mono.Addins.ExtensionContext;

namespace Mono.Addins
{
	/// <summary>
	/// Represents a mutation action of an extension context. It is necessary to acquire a transaction object
	/// when doing changes in a context (methods that modify a context take a transaction as parameter).
	/// Getting a transaction is a blocking operation: if a thread has started a transaction, other threads trying
	/// to get one will be blocked until the current transaction is finished.
	/// The transaction objects collects two kind of information:
	/// 1) changes to be done in the context. For example, it collects conditions to be registered, and then all conditions
	///    are added to the context at once when the transaction is completed (so the immutable collection that holds
	///    the condition list can be updated with a single allocation).
	/// 2) events to be fired once the transaction is completed. This is to avoid firing events while the context
	///    lock is taken.
	/// </summary>
	class ExtensionContextTransaction : IDisposable
	{
		List<TreeNode> loadedNodes;
		HashSet<TreeNode> childrenChanged;
		HashSet<string> extensionsChanged;
		List<(TreeNode Node, BaseCondition Condition)> nodeConditions;
		List<(TreeNode Node, BaseCondition Condition)> nodeConditionUnregistrations;
		List<TreeNode> treeNodeTransactions;
		ExtensionContextSnapshot snapshot;
		bool snaphotChanged;
		AddinEngineTransaction nestedAddinEngineTransaction;

		public ExtensionContextTransaction (ExtensionContext context)
		{
			Context = context;
			Monitor.Enter (Context.LocalLock);
			snapshot = context.CurrentSnapshot;
		}

		public ExtensionContext Context { get; }

		public bool DisableEvents { get; set; }

		public AddinEngine.AddinEngineSnapshot AddinEngineSnapshot => (AddinEngine.AddinEngineSnapshot)Snapshot;

		public ExtensionContextSnapshot Snapshot => snapshot;

		/// <summary>
		/// Gets an add-in engine transaction, if there is one in progress. Returns null if there isn't one.
		/// </summary>
		public AddinEngineTransaction GetAddinEngineTransaction()
		{
			if (this is AddinEngineTransaction et)
				return et;
			if (nestedAddinEngineTransaction != null)
				return nestedAddinEngineTransaction;
			return null;
		}

		/// <summary>
		/// Gets or creates an add-in engine transaction, which will be committed together with this transaction
		/// </summary>
		public AddinEngineTransaction GetOrCreateAddinEngineTransaction()
		{
			if (this is AddinEngineTransaction et)
				return et;
			if (nestedAddinEngineTransaction != null)
				return nestedAddinEngineTransaction;

			return nestedAddinEngineTransaction = Context.AddinEngine.BeginEngineTransaction();
		}

		protected void EnsureNewSnapshot()
		{
			if (!snaphotChanged)
			{
				snaphotChanged = true;
				var newSnapshot = Context.CreateSnapshot();
				newSnapshot.CopyFrom(snapshot);
				snapshot = newSnapshot;
			}
		}

		public void Dispose ()
		{
			try
			{
				// Update the context

				UpdateSnapshot();

				if (snaphotChanged)
					Context.SetSnapshot(snapshot);

			} finally {
				Monitor.Exit (Context.LocalLock);
			}

			// If there is a nested engine transaction, make sure it is disposed
			using var _ = nestedAddinEngineTransaction;

			// Do notifications outside the lock

			DispatchNotifications();
		}

		protected virtual void UpdateSnapshot()
		{
			if (nodeConditions != null)
			{
				BulkRegisterNodeConditions(nodeConditions);
			}

			if (nodeConditionUnregistrations != null)
			{
				BulkUnregisterNodeConditions(nodeConditionUnregistrations);
			}

			// Commit tree node transactions
			if (treeNodeTransactions != null)
			{
				foreach (var node in treeNodeTransactions)
					node.CommitChildrenUpdateTransaction();
			}
		}

		protected virtual void DispatchNotifications()
		{
			if (loadedNodes != null)
			{
				foreach (var node in loadedNodes)
					node.NotifyAddinLoaded();
			}
			if (childrenChanged != null)
			{
				foreach (var node in childrenChanged)
				{
					if (node.NotifyChildrenChanged())
						NotifyExtensionsChangedEvent(node.GetPath());
				}
			}
			if (extensionsChanged != null)
			{
				foreach (var path in extensionsChanged)
					Context.NotifyExtensionsChanged(new ExtensionEventArgs(path));
			}
		}

		void BulkRegisterNodeConditions(IEnumerable<(TreeNode Node, BaseCondition Condition)> nodeConditions)
		{
			// We are going to do many changes, so create a builder for the dictionary
			var dictBuilder = Snapshot.ConditionsToNodes.ToBuilder();
			List<(string ConditionId, BaseCondition BoundCondition)> bindings = new();

			// Group nodes by the conditions, so that all nodes for a conditions can be processed together

			foreach (var group in nodeConditions.GroupBy(c => c.Condition))
			{
				var condition = group.Key;

				if (!dictBuilder.TryGetValue(condition, out var list))
				{

					// Condition not yet registered, register it now

					// Get a list of conditions on which this one depends
					var conditionTypeIds = new List<string>();
					condition.GetConditionTypes(conditionTypeIds);

					foreach (string cid in conditionTypeIds)
					{
						// For each condition on which 'condition' depends, register the dependency
						// so that it if the condition changes, the dependencies are notified
						bindings.Add((cid, condition));
					}
					list = ImmutableArray<TreeNode>.Empty;
				}

				dictBuilder[condition] = list.AddRange(group.Select(item => item.Node));
			}

			foreach (var binding in bindings.GroupBy(b => b.ConditionId, b => b.BoundCondition))
			{
				ConditionInfo info = Context.GetOrCreateConditionInfo(this, binding.Key, null);
				info.BoundConditions = info.BoundConditions.AddRange(binding);
			}

			Snapshot.ConditionsToNodes = dictBuilder.ToImmutable();
		}

		void BulkUnregisterNodeConditions(IEnumerable<(TreeNode Node, BaseCondition Condition)> nodeConditions)
		{
			ImmutableDictionary<BaseCondition, ImmutableArray<TreeNode>>.Builder dictBuilder = null;

			foreach (var group in nodeConditions.GroupBy(c => c.Condition))
			{
				var condition = group.Key;
				if (!Snapshot.ConditionsToNodes.TryGetValue(condition, out var list))
					continue;

				var newList = list.RemoveRange(group.Select(item => item.Node));

				// If there are no changes, continue, no need to create the dictionary builder
				if (newList == list)
					continue;

				if (dictBuilder == null)
					dictBuilder = Snapshot.ConditionsToNodes.ToBuilder();

				if (newList.Length == 0)
				{

					// The condition is not used anymore. Remove it from the dictionary
					// and unregister it from any condition it was bound to

					dictBuilder.Remove(condition);
					var conditionTypeIds = new List<string>();
					condition.GetConditionTypes(conditionTypeIds);
					foreach (string cid in conditionTypeIds)
					{
						var info = Snapshot.ConditionTypes[cid];
						if (info != null)
							info.BoundConditions = info.BoundConditions.Remove(condition);
					}
				}
				else
					dictBuilder[condition] = newList;
			}
			if (dictBuilder != null)
				Snapshot.ConditionsToNodes = dictBuilder.ToImmutable();
		}

		public void ReportLoadedNode (TreeNode node)
		{
			if (loadedNodes == null)
				loadedNodes = new List<TreeNode> ();
			loadedNodes.Add (node);
		}

		public void ReportChildrenChanged (TreeNode node)
		{
			if (!DisableEvents) {
				if (childrenChanged == null)
					childrenChanged = new HashSet<TreeNode> ();
				childrenChanged.Add (node);
			}
		}

		public void NotifyExtensionsChangedEvent (string path)
		{
			if (!DisableEvents) {
				if (extensionsChanged == null)
					extensionsChanged = new HashSet<string> ();
				extensionsChanged.Add (path);
			}
		}

		public void RegisterNodeCondition (TreeNode node, BaseCondition cond)
		{
			EnsureNewSnapshot();
			if (nodeConditions == null)
				nodeConditions = new List<(TreeNode Node, BaseCondition Condition)> ();
			nodeConditions.Add ((node, cond));
		}

		public void UnregisterNodeCondition (TreeNode node, BaseCondition cond)
		{
			EnsureNewSnapshot();
			if (nodeConditionUnregistrations == null)
				nodeConditionUnregistrations = new List<(TreeNode Node, BaseCondition Condition)> ();
			nodeConditionUnregistrations.Add ((node, cond));
			if (nodeConditions != null)
				nodeConditions.Remove ((node, cond));
		}

		public void RegisterChildrenUpdateTransaction (TreeNode node)
		{
			if (treeNodeTransactions == null)
				treeNodeTransactions = new List<TreeNode> ();
			treeNodeTransactions.Add (node);
		}

	}

    class AddinEngineTransaction : ExtensionContextTransaction
    {
		List<KeyValuePair<string, string>> registeredAutoExtensionPoints;
		List<string> unregisteredAutoExtensionPoints;
		List<KeyValuePair<string, RuntimeAddin>> registeredAssemblyResolvePaths;
		List<string> unregisteredAssemblyResolvePaths;
		List<RuntimeAddin> addinLoadEvents;
		List<string> addinUnloadEvents;

		public AddinEngineTransaction (AddinEngine engine) : base (engine)
        {
        }

		public void RegisterAssemblyResolvePaths(string assembly, RuntimeAddin addin)
		{
			EnsureNewSnapshot();
			if (registeredAssemblyResolvePaths == null)
				registeredAssemblyResolvePaths = new List<KeyValuePair<string, RuntimeAddin>>();
			registeredAssemblyResolvePaths.Add(new KeyValuePair<string, RuntimeAddin>(assembly, addin));
		}

		public void UnregisterAssemblyResolvePaths(string assembly)
		{
			EnsureNewSnapshot();
			if (unregisteredAssemblyResolvePaths == null)
				unregisteredAssemblyResolvePaths = new List<string>();
			unregisteredAssemblyResolvePaths.Add(assembly);
		}

		public void ReportAddinLoad(RuntimeAddin addin)
		{
			if (addinLoadEvents == null)
				addinLoadEvents = new List<RuntimeAddin>();
			addinLoadEvents.Add(addin);
		}

		public void ReportAddinUnload(string id)
		{
			if (addinUnloadEvents == null)
				addinUnloadEvents = new List<string>();
			addinUnloadEvents.Add(id);
		}

		public void RegisterAutoTypeExtensionPoint(string typeName, string path)
		{
			EnsureNewSnapshot();
			if (registeredAutoExtensionPoints == null)
				registeredAutoExtensionPoints = new List<KeyValuePair<string, string>>();
			registeredAutoExtensionPoints.Add(new KeyValuePair<string, string>(typeName, path));
		}

		public void UnregisterAutoTypeExtensionPoint(string typeName)
		{
			EnsureNewSnapshot();
			if (unregisteredAutoExtensionPoints == null)
				unregisteredAutoExtensionPoints = new List<string>();
			unregisteredAutoExtensionPoints.Add(typeName);
		}

		protected override void UpdateSnapshot()
		{
			base.UpdateSnapshot();

			if (registeredAutoExtensionPoints != null)
				AddinEngineSnapshot.AutoExtensionTypes = AddinEngineSnapshot.AutoExtensionTypes.SetItems(registeredAutoExtensionPoints);

			if (unregisteredAutoExtensionPoints != null)
				AddinEngineSnapshot.AutoExtensionTypes = AddinEngineSnapshot.AutoExtensionTypes.RemoveRange(unregisteredAutoExtensionPoints);

			if (registeredAssemblyResolvePaths != null)
				AddinEngineSnapshot.AssemblyResolvePaths = AddinEngineSnapshot.AssemblyResolvePaths.SetItems(registeredAssemblyResolvePaths);

			if (unregisteredAssemblyResolvePaths != null)
				AddinEngineSnapshot.AssemblyResolvePaths = AddinEngineSnapshot.AssemblyResolvePaths.RemoveRange(unregisteredAssemblyResolvePaths);
		}

		protected override void DispatchNotifications()
		{
			base.DispatchNotifications();

			var engine = (AddinEngine)Context;

			if (addinLoadEvents != null)
			{
				foreach (var addin in addinLoadEvents)
					engine.ReportAddinLoad(addin);
			}
			if (addinUnloadEvents != null)
			{
				foreach (var id in addinUnloadEvents)
					engine.ReportAddinUnload(id);
			}
		}
	}
}
