//
// TreeNode.cs
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
using Mono.Addins.Description;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Mono.Addins
{
    class TreeNodeBuilder
	{
		AddinEngine addinEngine;
        string id;
		List<TreeNodeBuilder> children;
		ExtensionNode extensionNode;
		string path;
		TreeNode existingNode;

		public TreeNodeBuilder (TreeNode existingNode)
		{
			this.existingNode = existingNode;
			this.id = existingNode.Id;
		}

        public TreeNodeBuilder (AddinEngine addinEngine, string id)
        {
            this.addinEngine = addinEngine;
            this.id = id;
        }

		public string Id => id;

		public ExtensionPoint GetExtensionPoint ()
		{
			if (existingNode != null)
				return existingNode.ExtensionPoint;
			if (Parent != null)
				return Parent.GetExtensionPoint ();
			return null;
		}

		public TreeNodeBuilder Parent { get; set; }

		public IReadOnlyList<TreeNodeBuilder> Children => children;

		public BaseCondition Condition { get; set; }

        public ExtensionNodeType ExtensionNodeSet { get; set; }
        public ExtensionPoint ExtensionPoint { get; internal set; }

        public void AddChild (TreeNodeBuilder childBuilder)
		{
			childBuilder.Parent = this;
			if (children == null)
				children = new List<TreeNodeBuilder>();
			children.Add (childBuilder);
		}

		public void InsertChild (int curPos, TreeNodeBuilder childBuilder)
		{
			childBuilder.Parent = this;
			if (children == null)
				children = new List<TreeNodeBuilder>();
			children.Insert (curPos, childBuilder);
		}

		public int IndexOfChild (string id)
		{
			for (int n = 0; n < children.Count; n++) {
				if (children[n].Id == id)
					return n;
			}
			return -1;
		}

		public TreeNode Build (ExtensionContextTransaction transaction)
		{
			var childNodes = ImmutableArray.CreateRange (children.Select (builder => builder.Build (transaction)));
			if (existingNode != null) {
				existingNode.SetChildren (transaction, childNodes);
				return existingNode;
			} else {
				TreeNode cnode = new TreeNode (addinEngine, id);
				if (Condition != null)
					cnode.Condition = Condition;
				if (ExtensionNodeSet != null)
					cnode.ExtensionNodeSet = ExtensionNodeSet;
				if (extensionNode != null)
					cnode.AttachExtensionNode (extensionNode);
				cnode.SetChildren (transaction, childNodes);

				if (cnode.Condition != null)
					transaction.RegisterNodeCondition (cnode, cnode.Condition);

				transaction.ReportLoadedNode (cnode);
				return cnode;
			}
		}

		internal void AttachExtensionNode (ExtensionNode node)
        {
			extensionNode = node;
        }

        internal string GetPath ()
        {
			if (path == null) {
				if (existingNode != null)
					path = existingNode.GetPath ();
				else {
					if (Parent != null)
						path = Parent.GetPath () + "/" + id;
					else
						throw new InvalidOperationException (); // Should not happen
				}
			}
			return path;
        }

		public TreeNodeBuilder GetNode (string path)
		{
			var thisPath = GetPath ();

			if (path == thisPath)
				return this;

			if (!path.StartsWith (thisPath))
				throw new InvalidOperationException ("Invalid extension path. Should not happen!");

			var childPath = path.Substring (thisPath.Length);
			string[] parts = childPath.Split ('/');

			var node = this;

			foreach (var part in parts) {
				if (string.IsNullOrEmpty (part))
					continue;
				int i = IndexOfChild (part);
				if (i == -1) {
					var child = new TreeNodeBuilder (addinEngine, part);
					node.AddChild (child);
					node = child;
				} else
					node = children [i];
			}
			return node;
		}
    }
}
