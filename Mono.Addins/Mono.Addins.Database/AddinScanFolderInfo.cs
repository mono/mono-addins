
using System;
using System.IO;
using System.Collections;
using Mono.Addins.Serialization;

namespace Mono.Addins.Database
{
	class AddinScanFolderInfo: IBinaryXmlElement
	{
		Hashtable files = new Hashtable ();
		string folder;
		string fileName;
		
		static BinaryXmlTypeMap typeMap = new BinaryXmlTypeMap (
			typeof(AddinScanFolderInfo),
			typeof(AddinFileInfo)
		);
		
		internal AddinScanFolderInfo ()
		{
		}
		
		public AddinScanFolderInfo (string folder)
		{
			this.folder = folder;
		}
		
		public string FileName {
			get { return fileName; }
		}
		
		public static AddinScanFolderInfo Read (FileDatabase filedb, string file)
		{
			AddinScanFolderInfo finfo = (AddinScanFolderInfo) filedb.ReadSharedObject (file, typeMap);
			if (finfo != null)
				finfo.fileName = file;
			return finfo;
		}
		
		public static AddinScanFolderInfo Read (FileDatabase filedb, string basePath, string folderPath)
		{
			string fileName;
			AddinScanFolderInfo finfo = (AddinScanFolderInfo) filedb.ReadSharedObject (basePath, GetFolderId (folderPath), ".data", Util.GetFullPath (folderPath), typeMap, out fileName);
			if (finfo != null)
				finfo.fileName = fileName;
			return finfo;
		}
		
		static string GetFolderId (string path)
		{
			path = Util.GetFullPath (path);
			string s = path.Replace (Path.DirectorySeparatorChar, '_');
			s = s.Replace (Path.AltDirectorySeparatorChar, '_');
			s = s.Replace (Path.VolumeSeparatorChar, '_');
			s = s.Trim ('_');
			if (s.Length > 200)
				s = s.Substring (s.Length - 200);
			return s;
		}
		
		public void Write (FileDatabase filedb, string basePath)
		{
			filedb.WriteSharedObject (basePath, GetFolderId (folder), ".data", Util.GetFullPath (folder), fileName, typeMap, this);
		}
		
		public string Folder {
			get { return folder; }
		}
		
		public DateTime GetLastScanTime (string file)
		{
			AddinFileInfo info = (AddinFileInfo) files [file];
			if (info == null)
				return DateTime.MinValue;
			else
				return info.LastScan;
		}
		
		public AddinFileInfo GetAddinFileInfo (string file)
		{
			return (AddinFileInfo) files [file];
		}
		
		public void SetLastScanTime (string file, string addinId, bool isRoot, DateTime time, bool scanError)
		{
			AddinFileInfo info = (AddinFileInfo) files [file];
			if (info == null) {
				info = new AddinFileInfo ();
				info.File = file;
				files [file] = info;
			}
			info.LastScan = time;
			info.AddinId = addinId;
			info.IsRoot = isRoot;
			info.ScanError = scanError;
		}
		
		public ArrayList GetMissingAddins ()
		{
			ArrayList missing = new ArrayList ();
			
			if (!Directory.Exists (folder)) {
				// All deleted
				foreach (AddinFileInfo info in files.Values)
					missing.Add (info);
				files.Clear ();
				return missing;
			}
			ArrayList toDelete = new ArrayList ();
			foreach (AddinFileInfo info in files.Values) {
				if (!File.Exists (info.File)) {
					if (info.AddinId != null && info.AddinId.Length > 0)
						missing.Add (info);
					toDelete.Add (info.File);
				}
			}
			foreach (string file in toDelete)
				files.Remove (file);
				
			return missing;
		}
		
		void IBinaryXmlElement.Write (BinaryXmlWriter writer)
		{
			writer.WriteValue ("folder", folder);
			writer.WriteValue ("files", files);
		}
		
		void IBinaryXmlElement.Read (BinaryXmlReader reader)
		{
			folder = reader.ReadStringValue ("folder");
			reader.ReadValue ("files", files);
		}
	}
	
	
	class AddinFileInfo: IBinaryXmlElement
	{
		public string File;
		public DateTime LastScan;
		public string AddinId;
		public bool IsRoot;
		public bool ScanError;
		
		void IBinaryXmlElement.Write (BinaryXmlWriter writer)
		{
			writer.WriteValue ("File", File);
			writer.WriteValue ("LastScan", LastScan);
			writer.WriteValue ("AddinId", AddinId);
			writer.WriteValue ("IsRoot", IsRoot);
			writer.WriteValue ("ScanError", ScanError);
		}
		
		void IBinaryXmlElement.Read (BinaryXmlReader reader)
		{
			File = reader.ReadStringValue ("File");
			LastScan = reader.ReadDateTimeValue ("LastScan");
			AddinId = reader.ReadStringValue ("AddinId");
			IsRoot = reader.ReadBooleanValue ("IsRoot");
			ScanError = reader.ReadBooleanValue ("ScanError");
		}
	}
}
