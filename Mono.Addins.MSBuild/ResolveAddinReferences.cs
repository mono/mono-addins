// 
// ResolveAddinReferences.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2009 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Mono.Addins;
using Mono.Addins.Setup;
using System.IO;

namespace Mono.Addins.MSBuild
{
	public class ResolveAddinReferences: Task
	{
		List<TaskItem> references = new List<TaskItem> ();
		ITaskItem[] addinReferences;
		string extensionDomain;
		
		public override bool Execute ()
		{
			if (string.IsNullOrEmpty (extensionDomain)) {
				Log.LogError ("ExtensionDomain item not found");
				return false;
			}
			if (addinReferences == null) {
				return true;
			}

			Application app = SetupService.GetExtensibleApplication (extensionDomain);
			if (app == null) {
				Log.LogError ("Extension domain '{0}' not found", extensionDomain);
				return false;
			}
			
			foreach (ITaskItem item in addinReferences) {
				string addinId = item.ItemSpec.Replace (':',',');
				Addin addin = app.Registry.GetAddin (addinId);
				if (addin == null) {
					Log.LogError ("Add-in '{0}' not found", addinId);
					return false;
				}
				if (addin.Description == null) {
					Log.LogError ("Add-in '{0}' could not be loaded", addinId);
					return false;
				}
				foreach (string asm in addin.Description.MainModule.Assemblies) {
					string file = Path.Combine (addin.Description.BasePath, asm);
					TaskItem ti = new TaskItem (file);
					references.Add (ti);
				}
			}
			return true;
		}
		
		public ITaskItem[] AddinReferences {
			get { return addinReferences; }
			set { addinReferences = value; }
		}
		
		public string ExtensionDomain {
			get { return extensionDomain; }
			set { extensionDomain = value; }
		}
		
		[Output]
		public ITaskItem[] References {
			get { return references.ToArray (); }
		}
	}
}
