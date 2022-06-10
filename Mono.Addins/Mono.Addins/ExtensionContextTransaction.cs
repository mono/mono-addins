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
using System.Threading;

namespace Mono.Addins
{
    class ExtensionContextTransaction : IDisposable
    {
		List<TreeNode> loadedNodes;
		HashSet<TreeNode> childrenChanged;
		HashSet<string> extensionsChanged;
		List<(TreeNode Node, BaseCondition Condition)> nodeConditions;
		List<(TreeNode Node, BaseCondition Condition)> nodeConditionUnregistrations;

		public ExtensionContextTransaction (ExtensionContext context)
		{
			Context = context;
			Monitor.Enter (Context.LocalLock);
		}

		public ExtensionContext Context { get; }

		public bool DisableEvents { get; set; }

		public void Dispose ()
        {
			try {
				if (nodeConditions != null) {
					Context.BulkRegisterNodeConditions (this, nodeConditions);
				}

				if (nodeConditionUnregistrations != null) {
					Context.BulkUnregisterNodeConditions (this, nodeConditionUnregistrations);
				}
			} finally {
				Monitor.Exit (Context.LocalLock);
			}

			// Do notifications outside the lock

			if (loadedNodes != null) {
				foreach (var node in loadedNodes)
					node.NotifyAddinLoaded ();
			}
			if (childrenChanged != null) {
				foreach (var node in childrenChanged) {
					if (node.NotifyChildrenChanged ())
						NotifyExtensionsChangedEvent (node.GetPath ());
				}
			}
			if (extensionsChanged != null) {
				foreach (var path in extensionsChanged)
					Context.NotifyExtensionsChanged(new ExtensionEventArgs(path));
			}
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
			if (nodeConditions == null)
				nodeConditions = new List<(TreeNode Node, BaseCondition Condition)> ();
			nodeConditions.Add ((node, cond));
		}

		public void UnregisterNodeCondition (TreeNode node, BaseCondition cond)
		{
			if (nodeConditionUnregistrations == null)
				nodeConditionUnregistrations = new List<(TreeNode Node, BaseCondition Condition)> ();
			nodeConditionUnregistrations.Add ((node, cond));
		}
	}
}
