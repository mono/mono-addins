
using System;
using System.Reflection;
using System.IO;
using System.Collections;
using Mono.Addins.Description;

namespace Mono.Addins.Database
{
	internal class AddinScanResult: MarshalByRefObject
	{
		internal ArrayList AddinsToScan = new ArrayList ();
		internal ArrayList AddinsToUpdateRelations = new ArrayList ();
		internal ArrayList FilesToScan = new ArrayList ();
		internal ArrayList ModifiedFolderInfos = new ArrayList ();
		internal ArrayList FilesWithScanFailure = new ArrayList ();
		internal AddinHostIndex HostIndex;
		internal ArrayList RemovedAddins = new ArrayList ();
		Hashtable visitedFolders = new Hashtable ();
		
		Hashtable assemblyLocations = new Hashtable ();
		Hashtable assemblyLocationsByFullName = new Hashtable (); 
		
		public bool RegenerateAllData;
		public bool RegenerateRelationData;
		public bool changesFound;
		public bool CheckOnly;
		public bool LocateAssembliesOnly;
		
		public bool ChangesFound {
			get { return changesFound; }
			set {
				changesFound = value;
//				if (changesFound)
//					Console.WriteLine ("CF: " + Environment.StackTrace);
			}
		}
		
		public bool VisitFolder (string folder)
		{
			if (visitedFolders.Contains (folder))
				return false;
			else {
				visitedFolders.Add (folder, folder);
				return true;
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
		
		public void AddFileToWithFailure (string file)
		{
			if (!FilesWithScanFailure.Contains (file))
				FilesWithScanFailure.Add (file);
		}
		
		public void AddFileToScan (string file, AddinScanFolderInfo folderInfo)
		{
			FileToScan di = new FileToScan ();
			di.File = file;
			di.AddinScanFolderInfo = folderInfo;
			FilesToScan.Add (di);
			if (!ModifiedFolderInfos.Contains (folderInfo))
				ModifiedFolderInfos.Add (folderInfo);
		}
		
		public void AddAddinToUpdateRelations (string addinId)
		{
			if (!AddinsToUpdateRelations.Contains (addinId))
				AddinsToUpdateRelations.Add (addinId);
		}
		
		public void AddAssemblyLocation (string file)
		{
			string name = Path.GetFileNameWithoutExtension (file);
			ArrayList list = assemblyLocations [name] as ArrayList;
			if (list == null) {
				list = new ArrayList ();
				assemblyLocations [name] = list;
			}
			list.Add (file);
		}
		
		public string GetAssemblyLocation (string fullName)
		{
			string loc = assemblyLocationsByFullName [fullName] as String;
			if (loc != null)
				return loc;

			int i = fullName.IndexOf (',');
			string name = fullName.Substring (0,i);
			ArrayList list = assemblyLocations [name] as ArrayList;
			if (list == null)
				return null;
			
			string lastAsm = null;
			foreach (string file in list.ToArray ()) {
				AssemblyName aname = AssemblyName.GetAssemblyName (file);
				list.Remove (file);
				lastAsm = file;
				assemblyLocationsByFullName [aname.FullName] = file;
				if (aname.FullName == fullName)
					return file;
			}
			
			if (lastAsm != null) {
				// If an exact version is not found, just take any of them
				return lastAsm;
			}
			return null;
		}
	}
		
	class FileToScan
	{
		public string File;
		public AddinScanFolderInfo AddinScanFolderInfo;
	}
	
	class ObjectTypeExtenderData
	{
		public DependencyCollection Deps;
		public string TypeName;
	}
}
