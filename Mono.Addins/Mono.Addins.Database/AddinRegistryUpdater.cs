//
// AddinRegistryUpdater.cs
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
using System.IO;
using System.Linq;

namespace Mono.Addins.Database
{
	class AddinRegistryUpdater: AddinFolderVisitor
	{
		AddinDatabase database;
		AddinScanFolderInfo currentFolderInfo;
		AddinScanResult scanResult;

		// Folder info object that already existed. It is usually the same as currentFolderInfo, since
		// folder info objects are reused, but not always.
		AddinScanFolderInfo oldFolderInfo;

		public AddinRegistryUpdater (AddinDatabase database, AddinScanResult scanResult): base (database)
		{
			this.database = database;
			this.scanResult = scanResult;
			ScanContext = scanResult.ScanContext;
		}

		protected override void OnVisitFolder (IProgressStatus monitor, string path, string domain, bool recursive)
		{
			AddinScanFolderInfo folderInfo;

			AddinScanFolderInfo previousOldFolderInfo = oldFolderInfo;

			// Don't reset oldFolderInfo here. When scanning a folder that had scan index and now it doesn't,
			// we need to keep the old folder data since the root folder info had the info for all folders
			// in the domain.

			if (!database.GetFolderInfoForPath (monitor, path, out folderInfo)) {
				// folderInfo file was corrupt.
				// Just in case, we are going to regenerate all relation data.
				if (!FileSystem.DirectoryExists (path))
					scanResult.RegenerateRelationData = true;
			} else {
				// Directory is included but it doesn't exist. Ignore it.
				if (folderInfo == null && !FileSystem.DirectoryExists (path))
					return;
			}

			// if domain is null it means that a new domain has to be created.

			// Look for an add-in scan data index file. If it is present, it means the folder has been pre-scanned

			var dirScanDataIndex = AddinScanDataIndex.LoadFromFolder (monitor, path);

			if (dirScanDataIndex != null && scanResult.CleanGeneratedAddinScanDataFiles) {
				// Remove any existing dir.addindata if data is being generated
				dirScanDataIndex.Delete ();
				dirScanDataIndex = null;
			}

			bool sharedFolder = domain == AddinDatabase.GlobalDomain;
			bool isNewFolder = folderInfo == null;
			bool folderHasIndex = dirScanDataIndex != null;

			if (isNewFolder) {
				// No folder info. It is the first time this folder is scanned.
				// There is no need to store this object if the folder does not
				// contain add-ins.
				folderInfo = new AddinScanFolderInfo (path);
				folderInfo.FolderHasScanDataIndex = folderHasIndex;
			} else if (folderInfo.FolderHasScanDataIndex != folderHasIndex) {
				// A scan data index appeared or disappeared. The information in folderInfo is not reliable.
				// Update the folder info and regenerate everything.

				// Keep a copy of the old folder info, to be used to retrieve the old status of the add-ins.
				oldFolderInfo = folderInfo;
				folderInfo = new AddinScanFolderInfo (oldFolderInfo);

				scanResult.RegenerateRelationData = true;
				folderInfo.Reset ();
				scanResult.RegisterModifiedFolderInfo (folderInfo);
				folderInfo.FolderHasScanDataIndex = folderHasIndex;
			}

			if (!sharedFolder && (folderInfo.SharedFolder || folderInfo.Domain != domain)) {
				// If the folder already has a domain, reuse it
				if (domain == null && folderInfo.RootsDomain != null && folderInfo.RootsDomain != AddinDatabase.GlobalDomain)
					domain = folderInfo.RootsDomain;
				else if (domain == null) {
					folderInfo.Domain = domain = database.GetUniqueDomainId ();
					scanResult.RegenerateRelationData = true;
				} else {
					folderInfo.Domain = domain;
					if (!isNewFolder) {
						// Domain has changed. Update the folder info and regenerate everything.
						scanResult.RegenerateRelationData = true;
						scanResult.RegisterModifiedFolderInfo (folderInfo);
					}
				}
			} else if (!folderInfo.SharedFolder && sharedFolder) {
				scanResult.RegenerateRelationData = true;
			}

			folderInfo.SharedFolder = sharedFolder;

			// If there is no domain assigned to the host, get one now
			if (scanResult.Domain == AddinDatabase.UnknownDomain)
				scanResult.Domain = domain;

			// Discard folders not belonging to the required domain
			if (scanResult.Domain != null && domain != scanResult.Domain && domain != AddinDatabase.GlobalDomain)
				return;

			if (monitor.LogLevel > 1)
				monitor.Log ("Checking: " + path);

			currentFolderInfo = folderInfo;

			if (dirScanDataIndex != null) {
				// Instead of scanning the folder, just register the files in the index
				if (oldFolderInfo != null && !oldFolderInfo.FolderHasScanDataIndex)
				{
					// There was no scan index in the previous scan but there is one in this scan.
					// The old folder info doesn't contain the info for all files, just for the files in this folder.
					// Since the new scan has an index, it can contain references to files not directly in this folder,
					// so for those files we need to find their corresponding old folder info.

					// We group by folder so that we only need to query for folderInfo once per folder
					foreach (var folder in dirScanDataIndex.Files.GroupBy(f => Path.GetDirectoryName(f.FileName)))
					{
						AddinScanFolderInfo oldFolderInfoForIncludedFolder;
						if (folder.Key != path)
						{
							// The file does not belong to this folder, so we need to get the folderInfo from
							// the right folder
							database.GetFolderInfoForPath(monitor, folder.Key, out oldFolderInfoForIncludedFolder);
						}
						else
						{
							// The file belongs to the folder being visited, so oldFolderInfo is correct
							oldFolderInfoForIncludedFolder = oldFolderInfo;
						}
						foreach(var file in folder)
							RegisterFileToScan(monitor, file.FileName, file, oldFolderInfoForIncludedFolder);
					}
				}
				else
				{
					foreach (var file in dirScanDataIndex.Files)
						RegisterFileToScan(monitor, file.FileName, file, oldFolderInfo);
				}
				foreach (var file in dirScanDataIndex.Assemblies)
					scanResult.AssemblyIndex.AddAssemblyLocation (file);
			} else {

				base.OnVisitFolder (monitor, path, domain, recursive);

				if (!FileSystem.DirectoryExists (path)) {
					// The folder has been deleted. All add-ins defined in that folder should also be deleted.
					scanResult.RegenerateRelationData = true;
					scanResult.ChangesFound = true;
					if (scanResult.CheckOnly)
						return;
					database.DeleteFolderInfo (monitor, oldFolderInfo ?? currentFolderInfo);
				}
			}

			// Look for deleted add-ins.

			UpdateDeletedAddins (monitor, oldFolderInfo ?? currentFolderInfo);

			oldFolderInfo = previousOldFolderInfo;
		}

		protected override void OnVisitAddinManifestFile (IProgressStatus monitor, string file)
		{
			RegisterFileToScan (monitor, file, null, oldFolderInfo);
		}

		protected override void OnVisitAssemblyFile (IProgressStatus monitor, string file)
		{
			RegisterFileToScan (monitor, file, null, oldFolderInfo);
			scanResult.AssemblyIndex.AddAssemblyLocation (file);
		}

		public void UpdateDeletedAddins (IProgressStatus monitor, AddinScanFolderInfo folderInfo)
		{
			var missing = folderInfo.GetMissingAddins (FileSystem);
			if (missing.Count > 0) {
				if (FileSystem.DirectoryExists (folderInfo.Folder))
					scanResult.RegisterModifiedFolderInfo (folderInfo);
				scanResult.ChangesFound = true;
				if (scanResult.CheckOnly)
					return;

				foreach (AddinFileInfo info in missing) {
					database.UninstallAddin (monitor, info.Domain, info.AddinId, info.File, scanResult);
				}
			}
		}

		void RegisterFileToScan (IProgressStatus monitor, string file, AddinScanData scanData, AddinScanFolderInfo oldFolderInfo)
		{
			AddinFileInfo finfo = (oldFolderInfo ?? currentFolderInfo).GetAddinFileInfo (file);
			bool added = false;

			if (finfo != null && (!finfo.IsAddin || finfo.Domain == currentFolderInfo.GetDomain (finfo.IsRoot)) && !finfo.HasChanged (FileSystem, scanData?.MD5) && !scanResult.RegenerateAllData) {
				if (finfo.ScanError) {
					// Always schedule the file for scan if there was an error in a previous scan.
					// However, don't set ChangesFound=true, in this way if there isn't any other
					// change in the registry, the file won't be scanned again.
					scanResult.AddFileToScan (file, currentFolderInfo, finfo, scanData);
					added = true;
				}

				if (!finfo.IsAddin)
					return;

				if (database.AddinDescriptionExists (finfo.Domain, finfo.AddinId)) {
					// It is an add-in and it has not changed. Paths in the ignore list
					// are still valid, so they can be used.
					if (finfo.IgnorePaths != null)
						scanResult.ScanContext.AddPathsToIgnore (finfo.IgnorePaths);
					return;
				}
			}

			scanResult.ChangesFound = true;

			if (!scanResult.CheckOnly && !added)
				scanResult.AddFileToScan (file, currentFolderInfo, finfo, scanData);
		}
	}
}
