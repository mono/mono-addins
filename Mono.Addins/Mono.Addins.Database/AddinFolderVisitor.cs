//
// AddinScannerBase.cs
//
// Author:
//       Lluis Sanchez <llsan@microsoft.com>
//
// Copyright (c) 2018 Microsoft
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
using System.IO;
using System.Linq;
using System.Xml;

namespace Mono.Addins.Database
{
	class AddinFolderVisitor
	{
		AddinDatabase database;
		HashSet<string> visitedFolders = new HashSet<string> ();

		public ScanContext ScanContext { get; set; } = new ScanContext();

		protected AddinFileSystemExtension FileSystem {
			get { return database.FileSystem; }
		}

		public AddinFolderVisitor (AddinDatabase database)
		{
			this.database = database;
		}

		public void VisitFolder (IProgressStatus monitor, string path, string domain, bool recursive)
		{
			VisitFolderInternal (monitor, Path.GetFullPath (path), domain, recursive);
		}

		void VisitFolderInternal (IProgressStatus monitor, string path, string domain, bool recursive)
		{
			// Avoid folders including each other
			if (!visitedFolders.Add (path) || ScanContext.IgnorePath (path))
				return;

			OnVisitFolder (monitor, path, domain, recursive);
		}

		protected virtual void OnVisitFolder (IProgressStatus monitor, string path, string domain, bool recursive)
		{
			if (!FileSystem.DirectoryExists (path))
				return;
			
			var files = FileSystem.GetFiles (path).ToArray();

			// First of all scan .addins files, since they can contain exclude paths.
			// Only extract the information, don't follow directory inclusions yet

			List<AddinsEntry> addinsFileEntries = new List<AddinsEntry>();

			foreach (string file in files) {
				if (file.EndsWith (".addins", StringComparison.Ordinal))
					addinsFileEntries.AddRange (ParseAddinsFile (monitor, file, domain));
			}

			// Now look for .addin files. Addin files must be processed before
			// assemblies, because they may add files to the ignore list (i.e., assemblies
			// included in .addin files won't be scanned twice).

			foreach (string file in files) {
				if ((file.EndsWith(".addin.xml", StringComparison.Ordinal) || file.EndsWith(".addin", StringComparison.Ordinal)) && !ScanContext.IgnorePath (file))
					OnVisitAddinManifestFile (monitor, file);
			}

			// Now scan assemblies. They can also add files to the ignore list.

			foreach (string file in files) {
				if ((file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) && !ScanContext.IgnorePath(file)) {
					OnVisitAssemblyFile(monitor, file);
				}
			}

			// Follow .addins file inclusions

			foreach (var entry in addinsFileEntries) {
				string dir = entry.Folder;
				if (!Path.IsPathRooted(dir))
					dir = Path.GetFullPath (Path.Combine (path, entry.Folder));
				
				VisitFolderInternal (monitor, dir, entry.Domain, entry.Recursive);
			}

			// Scan subfolders

			if (recursive) {
				foreach (string sd in FileSystem.GetDirectories (path))
					VisitFolderInternal (monitor, sd, domain, true);
			}
		}

		protected virtual void OnVisitAddinManifestFile (IProgressStatus monitor, string file)
		{

		}

		protected virtual void OnVisitAssemblyFile (IProgressStatus monitor, string file)
		{

		}

		List<AddinsEntry> ParseAddinsFile (IProgressStatus monitor, string file, string domain)
		{
			List<AddinsEntry> entries = new List<AddinsEntry>();
			XmlTextReader r = null;
			string basePath = Path.GetDirectoryName (file);

			try {
				r = new XmlTextReader (FileSystem.OpenTextFile (file));
				r.MoveToContent ();
				if (r.IsEmptyElement)
					return entries;
				r.ReadStartElement ();
				r.MoveToContent ();
				while (r.NodeType != XmlNodeType.EndElement) {
					if (r.NodeType == XmlNodeType.Element && r.LocalName == "Directory") {
						bool.TryParse(r.GetAttribute("include-subdirs"), out var subs);
						string sdom;
						string share = r.GetAttribute ("shared");
						if (share == "true")
							sdom = AddinDatabase.GlobalDomain;
						else if (share == "false")
							sdom = null;
						else
							sdom = domain; // Inherit the domain

						string path = r.ReadElementString ().Trim ();
						if (path.Length > 0) {
							path = Util.NormalizePath (path);
							entries.Add (new AddinsEntry { Folder = path, Domain = sdom, Recursive = subs });
						}
					} else if (r.NodeType == XmlNodeType.Element && r.LocalName == "GacAssembly") {
						string aname = r.ReadElementString ().Trim ();
						if (aname.Length > 0) {
							aname = Util.NormalizePath (aname);
							aname = Util.GetGacPath (aname);
							if (aname != null) {
								// Gac assemblies always use the global domain
								entries.Add (new AddinsEntry { Folder = aname, Domain = AddinDatabase.GlobalDomain });
							}
						}
					} else if (r.NodeType == XmlNodeType.Element && r.LocalName == "Exclude") {
						string path = r.ReadElementString ().Trim ();
						if (path.Length > 0) {
							path = Util.NormalizePath (path);
							if (!Path.IsPathRooted (path))
								path = Path.Combine (basePath, path);
							ScanContext.AddPathToIgnore (Path.GetFullPath (path));
						}
					} else
						r.Skip ();
					r.MoveToContent ();
				}
			} catch (Exception ex) {
				if (monitor != null)
					monitor.ReportError ("Could not process addins file: " + file, ex);
			} finally {
				if (r != null)
					r.Close ();
			}
			return entries;
		}

		class AddinsEntry
		{
			public string Folder;
			public string Domain;
			public bool Recursive;
		}
	}
}
