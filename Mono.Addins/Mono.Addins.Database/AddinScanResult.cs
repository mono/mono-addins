//
// AddinScanResult.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

namespace Mono.Addins.Database
{
	internal class AddinScanResult: MarshalByRefObject
	{
		internal List<string> AddinsToScan = new List<string> ();
		internal List<string> AddinsToUpdateRelations = new List<string> ();
		internal List<string> AddinsToUpdate = new List<string> ();
		internal List<FileToScan> FilesToScan = new List<FileToScan> ();
		internal List<AddinScanFolderInfo> ModifiedFolderInfos = new List<AddinScanFolderInfo> ();
		internal AddinHostIndex HostIndex;
		internal List<string> RemovedAddins = new List<string> ();

		bool regenerateRelationData;
		bool changesFound;
		
		public bool RegenerateAllData;
		public bool CheckOnly;
		public bool LocateAssembliesOnly;
		public string Domain;

		public ScanContext ScanContext { get; } = new ScanContext ();

		public AssemblyIndex AssemblyIndex { get; } = new AssemblyIndex ();

		public bool CleanGeneratedAddinScanDataFiles { get; set; }

		public bool ChangesFound {
			get { return changesFound; }
			set { changesFound = value; }
		}

		public bool RegenerateRelationData {
			get { return regenerateRelationData; }
			set {
				regenerateRelationData = value;
				if (value)
					ChangesFound = true;
			}
		}
		
		public void AddAddinToScan (string addinId)
		{
			if (!AddinsToScan.Contains (addinId))
				AddinsToScan.Add (addinId);
		}
		
		public void AddRemovedAddin (string addinId)
		{
			if (!RemovedAddins.Contains (addinId))
				RemovedAddins.Add (addinId);
		}
		
		public void AddFileToScan (string file, AddinScanFolderInfo folderInfo, AddinFileInfo oldFileInfo, AddinScanData scanData)
		{
			FileToScan di = new FileToScan ();
			di.File = file;
			di.AddinScanFolderInfo = folderInfo;
			di.OldFileInfo = oldFileInfo;
			di.ScanDataMD5 = scanData?.MD5;
			FilesToScan.Add (di);
			RegisterModifiedFolderInfo (folderInfo);
		}
		
		public void RegisterModifiedFolderInfo (AddinScanFolderInfo folderInfo)
		{
			if (!ModifiedFolderInfos.Contains (folderInfo))
				ModifiedFolderInfos.Add (folderInfo);
		}
		
		public void AddAddinToUpdateRelations (string addinId)
		{
			if (!AddinsToUpdateRelations.Contains (addinId))
				AddinsToUpdateRelations.Add (addinId);
		}
		
		public void AddAddinToUpdate (string addinId)
		{
			if (!AddinsToUpdate.Contains (addinId))
				AddinsToUpdate.Add (addinId);
		}
	}
		
	class FileToScan
	{
		public string File;
		public AddinScanFolderInfo AddinScanFolderInfo;
		public AddinFileInfo OldFileInfo;
		public string ScanDataMD5;
	}

	class ScanContext
	{
		HashSet<string> filesToIgnore;

		public void AddPathToIgnore (string path)
		{
			if (filesToIgnore == null)
				filesToIgnore = new HashSet<string> ();
			filesToIgnore.Add (path);
		}

		public bool IgnorePath (string file)
		{
			if (filesToIgnore == null)
				return false;
			string root = Path.GetPathRoot (file);
			while (root != file) {
				if (filesToIgnore.Contains (file))
					return true;
				file = Path.GetDirectoryName (file);
			}
			return false;
		}

		public void AddPathsToIgnore (IEnumerable paths)
		{
			foreach (string p in paths)
				AddPathToIgnore (p);
		}
	}
}
