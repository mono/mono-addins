
using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;
using Mono.Addins.Database;
using Mono.Addins.Description;

namespace Mono.Addins
{
	public class AddinRegistry: IDisposable
	{
		AddinDatabase database;
		StringCollection addinDirs;
		string basePath;
		
		public AddinRegistry (string registryPath): this (registryPath, null)
		{
		}
		
		internal AddinRegistry (string registryPath, string startupDirectory)
		{
			basePath = Util.GetFullPath (registryPath);
			database = new AddinDatabase (this);
			addinDirs = new StringCollection ();
			addinDirs.Add (Path.Combine (basePath, "addins"));
		}
		
		public static AddinRegistry GetGlobalRegistry ()
		{
			return GetGlobalRegistry (null);
		}
		
		internal static AddinRegistry GetGlobalRegistry (string startupDirectory)
		{
			AddinRegistry reg = new AddinRegistry (GlobalRegistryPath, startupDirectory);
			// TODO: What about windows?
			reg.AddinDirectories.Add ("/etc/mono.addins");
			return reg;
		}
		
		internal static string GlobalRegistryPath {
			get {
				string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("HOME"), ".config");
				path = Path.Combine (path, "mono.addins");
				return Util.GetFullPath (path);
			}
		}
		
		public string RegistryPath {
			get { return basePath; }
		}
		
		public void Dispose ()
		{
			database.Shutdown ();
		}
		
		public Addin GetAddin (string id)
		{
			return database.GetInstalledAddin (id);
		}
		
		public Addin GetAddin (string id, bool exactVersionMatch)
		{
			return database.GetInstalledAddin (id, exactVersionMatch);
		}
		
		public Addin[] GetAddins ()
		{
			ArrayList list = database.GetInstalledAddins ();
			return (Addin[]) list.ToArray (typeof(Addin));
		}
		
		public Addin[] GetAddinRoots ()
		{
			ArrayList list = database.GetAddinRoots ();
			return (Addin[]) list.ToArray (typeof(Addin));
		}
		
		public AddinDescription GetAddinDescription (IProgressStatus progressStatus, string file)
		{
			string outFile = Path.GetTempFileName ();
			try {
				database.ParseAddin (progressStatus, file, outFile, false);
			}
			catch {
				File.Delete (outFile);
				throw;
			}
			
			try {
				AddinDescription desc = AddinDescription.Read (outFile);
				if (desc != null)
					desc.AddinFile = file;
				return desc;
			}
			catch {
				// Errors are already reported using the progress status object
				return null;
			}
			finally {
				File.Delete (outFile);
			}
		}
		
		public bool IsAddinEnabled (string id)
		{
			return database.IsAddinEnabled (id);
		}
		
		public void EnableAddin (string id)
		{
			database.EnableAddin (id, true);
		}
		
		public void DisableAddin (string id)
		{
			database.DisableAddin (id);
		}
		
		public void DumpFile (string file)
		{
			Mono.Addins.Serialization.BinaryXmlReader.DumpFile (file);
		}
		
		public void ResetConfiguration ()
		{
			database.ResetConfiguration ();
		}

		public void Update (IProgressStatus monitor)
		{
			database.Update (monitor);
		}

		public void Rebuild (IProgressStatus monitor)
		{
			database.Repair (monitor);
		}
		
		internal Addin GetAddinForHostAssembly (string filePath)
		{
			return database.GetAddinForHostAssembly (filePath);
		}
		
		internal bool AddinDependsOn (string id1, string id2)
		{
			return database.AddinDependsOn (id1, id2);
		}
		
		internal void ScanFolders (IProgressStatus monitor, string folderToScan)
		{
			database.ScanFolders (monitor, folderToScan);
		}
		
		internal void ParseAddin (IProgressStatus progressStatus, string file, string outFile)
		{
			database.ParseAddin (progressStatus, file, outFile, true);
		}
		
		public string DefaultAddinsFolder {
			get { return Path.Combine (basePath, "addins"); }
		}
		
		internal StringCollection AddinDirectories {
			get { return addinDirs; }
		}
		
		internal bool CreateHostAddinsFile (string hostFile)
		{
			hostFile = Util.GetFullPath (hostFile);
			string baseName = Path.GetFileNameWithoutExtension (hostFile);
			
			foreach (string s in Directory.GetFiles (DefaultAddinsFolder, baseName + "*.host.addins")) {
				try {
					using (StreamReader sr = new StreamReader (s)) {
						XmlTextReader tr = new XmlTextReader (sr);
						tr.MoveToContent ();
						string host = tr.GetAttribute ("host-reference");
						if (host == hostFile)
							return false;
					}
				}
				catch {
					// Ignore this file
				}
			}
			
			string file = Path.Combine (DefaultAddinsFolder, baseName) + ".host.addins";
			int n=1;
			while (File.Exists (file)) {
				file = Path.Combine (DefaultAddinsFolder, baseName) + "_" + n + ".host.addins";
				n++;
			}
			
			using (StreamWriter sw = new StreamWriter (file)) {
				XmlTextWriter tw = new XmlTextWriter (sw);
				tw.Formatting = Formatting.Indented;
				tw.WriteStartElement ("Addins");
				tw.WriteAttributeString ("host-reference", hostFile);
				tw.WriteElementString ("Directory", Path.GetDirectoryName (hostFile));
				tw.WriteEndElement ();
				tw.Close ();
			}
			return true;
		}
	}
}
