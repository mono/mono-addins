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
		TreeNode builtNode;
		TreeNodeBuilder parentNode;
		bool built;

		private TreeNodeBuilder ()
		{
		}

		private TreeNodeBuilder (TreeNode existingNode)
		{
			this.existingNode = builtNode = existingNode;
			this.id = existingNode.Id;
			ExtensionNodeSet = existingNode.ExtensionNodeSet;
		}

		public static TreeNodeBuilder FromNode (TreeNode existingNode)
		{
			return new TreeNodeBuilder (existingNode);
		}

		public static TreeNodeBuilder CreateNew (AddinEngine addinEngine, string id, TreeNodeBuilder parentNode)
		{
			return new TreeNodeBuilder {
				addinEngine = addinEngine,
				id = id,
				Parent = parentNode,
				builtNode = new TreeNode (addinEngine, id, parentNode.builtNode)
			};
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

		public TreeNodeBuilder Parent { get; private set; }

		public BaseCondition Condition { get; set; }

        public ExtensionNodeSet ExtensionNodeSet { get; set; }

		void LoadChildren ()
		{
			if (children == null) {
				children = new List<TreeNodeBuilder> ();
				builtNode.LoadChildrenIntoBuilder (this);
			}
		}

        public void AddChild (TreeNodeBuilder childBuilder)
		{
			LoadChildren ();

			// The parent is specified in CreateNew
			if (childBuilder.existingNode == null && childBuilder.Parent != this)
				throw new InvalidOperationException ();

			children.Add (childBuilder);
		}

		public void InsertChild (int curPos, TreeNodeBuilder childBuilder)
		{
			LoadChildren ();

			// The parent is specified in CreateNew
			if (childBuilder.Parent != this)
				throw new InvalidOperationException ();

			children.Insert (curPos, childBuilder);
		}

		public int IndexOfChild (string id)
		{
			LoadChildren ();
			for (int n = 0; n < children.Count; n++) {
				if (children[n].Id == id)
					return n;
			}
			return -1;
		}

		public int ChildrenCount {
			get {
				LoadChildren ();
				return children.Count;
			}
		}

		public TreeNode Build (ExtensionContextTransaction transaction)
		{
			if (built)
				return builtNode;

			built = true;

			var childNodes = children != null ? ImmutableArray.CreateRange (children.Select (builder => builder.Build (transaction))) : ImmutableArray<TreeNode>.Empty;

			if (existingNode != null) {
				if (children != null)
					existingNode.SetChildren (transaction, childNodes);
				return existingNode;
			} else {
				if (Condition != null)
					builtNode.Condition = Condition;
				if (ExtensionNodeSet != null)
					builtNode.ExtensionNodeSet = ExtensionNodeSet;
				if (extensionNode != null)
					builtNode.AttachExtensionNode (extensionNode);

				if (children != null)
					builtNode.SetChildren (transaction, childNodes);

				if (builtNode.Condition != null)
					transaction.RegisterNodeCondition (builtNode, builtNode.Condition);

				transaction.ReportLoadedNode (builtNode);
				return builtNode;
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
				int i = node.IndexOfChild (part);
				if (i == -1) {
					var child = TreeNodeBuilder.CreateNew (addinEngine, part, node);
					node.AddChild (child);
					node = child;
				} else
					node = node.children [i];
			}
			return node;
		}
    }
}
