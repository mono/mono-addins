//
// SetupService.cs
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
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using Mono.Addins.Description;
using Mono.Addins.Setup.ProgressMonitoring;
using Microsoft.Win32;
using System.Diagnostics;
using Mono.PkgConfig;

namespace Mono.Addins.Setup
{
	/// <summary>
	/// Provides tools for managing add-ins
	/// </summary>
	/// <remarks>
	/// This class can be used to manage the add-ins of an application. It allows installing and uninstalling
	/// add-ins, taking into account add-in dependencies. It provides methods for installing add-ins from on-line
	/// repositories and tools for generating those repositories.
	/// </remarks>
	public class SetupService
	{
		RepositoryRegistry repositories;
		string applicationNamespace;
		string installDirectory;
		AddinStore store;
		AddinSystemConfiguration config;
		const string addinFilesDir = "_addin_files";
		
		AddinRegistry registry;
		
		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <remarks>
		/// If the add-in manager is initialized (AddinManager.Initialize has been called), then this instance
		/// will manage the add-in registry of the initialized engine.
		/// </remarks>
		public SetupService ()
		{
			if (AddinManager.IsInitialized)
				registry = AddinManager.Registry;
			else
				registry = AddinRegistry.GetGlobalRegistry ();
			
			repositories = new RepositoryRegistry (this);
			store = new AddinStore (this);
		}
		
		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="registry">
		/// Add-in registry to manage
		/// </param>
		public SetupService (AddinRegistry registry)
		{
			this.registry = registry;
			repositories = new RepositoryRegistry (this);
			store = new AddinStore (this);
		}
		
		/// <summary>
		/// The add-in registry being managed
		/// </summary>
		public AddinRegistry Registry {
			get { return registry; }
		}
		
		internal string RepositoryCachePath {
			get { return Path.Combine (registry.RegistryPath, "repository-cache"); }
		}
		
		string RootConfigFile {
			get { return Path.Combine (registry.RegistryPath, "addins-setup.config"); }
		}
		
		/// <summary>
		/// Default add-in namespace of the application (optional). If set, only add-ins that belong to that namespace
		/// will be shown in add-in lists.
		/// </summary>
		public string ApplicationNamespace {
			get { return applicationNamespace; }
			set { applicationNamespace = value; }
		}
		
		/// <summary>
		/// Directory where to install add-ins. If not specified, the 'addins' subdirectory of the
		/// registry location is used.
		/// </summary>
		public string InstallDirectory {
			get {
				if (installDirectory != null && installDirectory.Length > 0)
					return installDirectory;
				else
					return registry.DefaultAddinsFolder;
			}
			set { installDirectory = value; }
		}
		
		/// <summary>
		/// Returns a RepositoryRegistry which can be used to manage on-line repository references
		/// </summary>
		public RepositoryRegistry Repositories {
			get { return repositories; }
		}
		
		internal AddinStore Store {
			get { return store; }
		}
		
		/// <summary>
		/// Resolves add-in dependencies.
		/// </summary>
		/// <param name="statusMonitor">
		/// Progress monitor where to show progress status
		/// </param>
		/// <param name="addins">
		/// List of add-ins to check
		/// </param>
		/// <param name="resolved">
		/// Packages that need to be installed.
		/// </param>
		/// <param name="toUninstall">
		/// Packages that need to be uninstalled.
		/// </param>
		/// <param name="unresolved">
		/// Add-in dependencies that could not be resolved.
		/// </param>
		/// <returns>
		/// True if all dependencies could be resolved.
		/// </returns>
		/// <remarks>
		/// This method can be used to get a list of all packages that have to be installed in order to install
		/// an add-in or set of add-ins. The list of packages to install will include the package that provides the
		/// add-in, and all packages that provide the add-in dependencies. In some cases, packages may need to
		/// be installed (for example, when an installed add-in needs to be upgraded).
		/// </remarks>
		public bool ResolveDependencies (IProgressStatus statusMonitor, AddinRepositoryEntry[] addins, out PackageCollection resolved, out PackageCollection toUninstall, out DependencyCollection unresolved)
		{
			return store.ResolveDependencies (statusMonitor, addins, out resolved, out toUninstall, out unresolved);
		}
		
		/// <summary>
		/// Resolves add-in dependencies.
		/// </summary>
		/// <param name="statusMonitor">
		/// Progress monitor where to show progress status
		/// </param>
		/// <param name="packages">
		/// Packages that need to be installed.
		/// </param>
		/// <param name="toUninstall">
		/// Packages that need to be uninstalled.
		/// </param>
		/// <param name="unresolved">
		/// Add-in dependencies that could not be resolved.
		/// </param>
		/// <returns>
		/// True if all dependencies could be resolved.
		/// </returns>
		/// <remarks>
		/// This method can be used to get a list of all packages that have to be installed in order to satisfy
		/// the dependencies of a package or set of packages. The 'packages' argument must have the list of packages
		/// to be resolved. When resolving dependencies, if there is any additional package that needs to be installed,
		/// it will be added to the same 'packages' collection. In some cases, packages may need to
		/// be installed (for example, when an installed add-in needs to be upgraded). Those packages will be added
		/// to the 'toUninstall' collection. Packages that could not be resolved are added to the 'unresolved'
		/// collection.
		/// </remarks>
		public bool ResolveDependencies (IProgressStatus statusMonitor, PackageCollection packages, out PackageCollection toUninstall, out DependencyCollection unresolved)
		{
			return store.ResolveDependencies (statusMonitor, packages, out toUninstall, out unresolved);
		}
		
		/// <summary>
		/// Installs add-in packages
		/// </summary>
		/// <param name="statusMonitor">
		/// Progress monitor where to show progress status
		/// </param>
		/// <param name="files">
		/// Paths to the packages to install
		/// </param>
		/// <returns>
		/// True if the installation succeeded
		/// </returns>
		public bool Install (IProgressStatus statusMonitor, params string[] files)
		{
			return store.Install (statusMonitor, files);
		}
		
		/// <summary>
		/// Installs add-in packages from on-line repositories
		/// </summary>
		/// <param name="statusMonitor">
		/// Progress monitor where to show progress status
		/// </param>
		/// <param name="addins">
		/// References to the add-ins to be installed
		/// </param>
		/// <returns>
		/// True if the installation succeeded
		/// </returns>
		public bool Install (IProgressStatus statusMonitor, params AddinRepositoryEntry[] addins)
		{
			return store.Install (statusMonitor, addins);
		}
		
		/// <summary>
		/// Installs add-in packages
		/// </summary>
		/// <param name="statusMonitor">
		/// Progress monitor where to show progress status
		/// </param>
		/// <param name="packages">
		/// Packages to install
		/// </param>
		/// <returns>
		/// True if the installation succeeded
		/// </returns>
		public bool Install (IProgressStatus statusMonitor, PackageCollection packages)
		{
			return store.Install (statusMonitor, packages);
		}
		
		/// <summary>
		/// Uninstalls an add-in.
		/// </summary>
		/// <param name="statusMonitor">
		/// Progress monitor where to show progress status
		/// </param>
		/// <param name="id">
		/// Full identifier of the add-in to uninstall.
		/// </param>
		public void Uninstall (IProgressStatus statusMonitor, string id)
		{
			store.Uninstall (statusMonitor, id);
		}
		
		/// <summary>
		/// Uninstalls a set of add-ins
		/// </summary>
		/// <param name='statusMonitor'>
		/// Progress monitor where to show progress status
		/// </param>
		/// <param name='ids'>
		/// Full identifiers of the add-ins to uninstall.
		/// </param>
		public void Uninstall (IProgressStatus statusMonitor, IEnumerable<string> ids)
		{
			store.Uninstall (statusMonitor, ids);
		}
		
		/// <summary>
		/// Gets information about an add-in
		/// </summary>
		/// <param name="addin">
		/// The add-in
		/// </param>
		/// <returns>
		/// Add-in header data
		/// </returns>
		public static AddinHeader GetAddinHeader (Addin addin)
		{
			return AddinInfo.ReadFromDescription (addin.Description);
		}
		
		/// <summary>
		/// Gets a list of add-ins which depend on an add-in
		/// </summary>
		/// <param name="id">
		/// Full identifier of an add-in.
		/// </param>
		/// <param name="recursive">
		/// When set to True, dependencies will be gathered recursivelly
		/// </param>
		/// <returns>
		/// List of dependent add-ins.
		/// </returns>
		/// <remarks>
		/// This methods returns a list of add-ins which have the add-in identified by 'id' as a direct
		/// (or indirect if recursive=True) dependency.
		/// </remarks>
		public Addin[] GetDependentAddins (string id, bool recursive)
		{
			return store.GetDependentAddins (id, recursive);
		}
		
		/// <summary>
		/// Packages an add-in
		/// </summary>
		/// <param name="statusMonitor">
		/// Progress monitor where to show progress status
		/// </param>
		/// <param name="targetDirectory">
		/// Directory where to generate the package
		/// </param>
		/// <param name="filePaths">
		/// Paths to the add-ins to be packaged. Paths can be either the main assembly of an add-in, or an add-in
		/// manifest (.addin or .addin.xml).
		/// </param>
		/// <remarks>
		/// This method can be used to create a package for an add-in, which can then be pushed to an on-line
		/// repository. The package will include the main assembly or manifest of the add-in and any external
		/// file declared in the add-in metadata.
		/// </remarks>
		public string[] BuildPackage (IProgressStatus statusMonitor, string targetDirectory, params string[] filePaths)
		{
			List<string> outFiles = new List<string> ();
			foreach (string file in filePaths) {
				string f = BuildPackageInternal (statusMonitor, targetDirectory, file);
				if (f != null)
					outFiles.Add (f);
			}
			return outFiles.ToArray ();
		}
		
		string BuildPackageInternal (IProgressStatus monitor, string targetDirectory, string filePath)
		{
			AddinDescription conf = registry.GetAddinDescription (monitor, filePath);
			if (conf == null) {
				monitor.ReportError ("Could not read add-in file: " + filePath, null);
				return null;
			}
			
			string basePath = Path.GetDirectoryName (filePath);
			
			if (targetDirectory == null)
				targetDirectory = basePath;

			// Generate the file name
			
			string name;
			if (conf.LocalId.Length == 0)
				name = Path.GetFileNameWithoutExtension (filePath);
			else
				name = conf.LocalId;
			name = Addin.GetFullId (conf.Namespace, name, conf.Version);
			name = name.Replace (',','_').Replace (".__", ".");
			
			string outFilePath = Path.Combine (targetDirectory, name) + ".mpack";
			
			ZipOutputStream s = new ZipOutputStream (File.Create (outFilePath));
			s.SetLevel(5);
			
			// Generate a stripped down description of the add-in in a file, since the complete
			// description may be declared as assembly attributes
			
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = false;
			doc.LoadXml (conf.SaveToXml ().OuterXml);
			CleanDescription (doc.DocumentElement);
			MemoryStream ms = new MemoryStream ();
			XmlTextWriter tw = new XmlTextWriter (ms, System.Text.Encoding.UTF8);
			tw.Formatting = Formatting.Indented;
			doc.WriteTo (tw);
			tw.Flush ();
			byte[] data = ms.ToArray ();
			
			ZipEntry infoEntry = new ZipEntry ("addin.info");
			s.PutNextEntry (infoEntry);
			s.Write (data, 0, data.Length);
			
			// Now add the add-in files
			
			ArrayList list = new ArrayList ();
			if (!conf.AllFiles.Contains (Path.GetFileName (filePath)))
				list.Add (Path.GetFileName (filePath));
			foreach (string f in conf.AllFiles) {
				list.Add (f);
			}
			
			foreach (var prop in conf.Properties) {
				try {
					if (File.Exists (Path.Combine (basePath, prop.Value)))
						list.Add (prop.Value);
				} catch {
					// Ignore errors
				}
			}
			
			monitor.Log ("Creating package " + Path.GetFileName (outFilePath));
			
			foreach (string file in list) {
				string fp = Path.Combine (basePath, file);
				using (FileStream fs = File.OpenRead (fp)) {
					byte[] buffer = new byte [fs.Length];
					fs.Read (buffer, 0, buffer.Length);
					
					ZipEntry entry = new ZipEntry (file);
					s.PutNextEntry (entry);
					s.Write (buffer, 0, buffer.Length);
				}
			}
			
			s.Finish();
			s.Close();		
			return outFilePath;
		}
		
		void CleanDescription (XmlElement parent)
		{
			ArrayList todelete = new ArrayList ();
			
			foreach (XmlNode nod in parent.ChildNodes) {
				XmlElement elem = nod as XmlElement;
				if (elem == null) {
					todelete.Add (nod);
					continue;
				}
				if (elem.LocalName == "Module")
					CleanDescription (elem);
				else if (elem.LocalName != "Dependencies" && elem.LocalName != "Runtime" && elem.LocalName != "Header")
					todelete.Add (elem);
			}
			foreach (XmlNode e in todelete)
				parent.RemoveChild (e);
		}
		
		/// <summary>
		/// Generates an on-line repository
		/// </summary>
		/// <param name="statusMonitor">
		/// Progress monitor where to show progress status
		/// </param>
		/// <param name="path">
		/// Path to the directory that contains the add-ins and that is going to be published
		/// </param>
		/// <remarks>
		/// This method generates the index files required to publish a directory as an online repository
		/// of add-ins.
		/// </remarks>
		public void BuildRepository (IProgressStatus statusMonitor, string path)
		{
			string mainPath = Path.Combine (path, "main.mrep");
			ArrayList allAddins = new ArrayList ();
			
			Repository rootrep = (Repository) AddinStore.ReadObject (mainPath, typeof(Repository));
			if (rootrep == null)
				rootrep = new Repository ();
			
			IProgressMonitor monitor = ProgressStatusMonitor.GetProgressMonitor (statusMonitor);
			BuildRepository (monitor, rootrep, path, "root.mrep", allAddins);
			AddinStore.WriteObject (mainPath, rootrep);
			GenerateIndexPage (rootrep, allAddins, path);
			monitor.Log.WriteLine ("Updated main.mrep");
		}
		
		void BuildRepository (IProgressMonitor monitor, Repository rootrep, string rootPath, string relFilePath, ArrayList allAddins)
		{
			DateTime lastModified = DateTime.MinValue;
			
			string mainFile = Path.Combine (rootPath, relFilePath);
			string mainPath = Path.GetDirectoryName (mainFile);
			string supportFileDir = Path.Combine (mainPath, addinFilesDir);
			
			if (File.Exists (mainFile))
				lastModified = File.GetLastWriteTime (mainFile);
			
			Repository mainrep = (Repository) AddinStore.ReadObject (mainFile, typeof(Repository));
			if (mainrep == null) {
				mainrep = new Repository ();
			}
			
			ReferenceRepositoryEntry repEntry = (ReferenceRepositoryEntry) rootrep.FindEntry (relFilePath);
			DateTime rootLastModified = repEntry != null ? repEntry.LastModified : DateTime.MinValue;
			
			bool modified = false;
			
			monitor.Log.WriteLine ("Checking directory: " + mainPath);
			foreach (string file in Directory.GetFiles (mainPath, "*.mpack")) {
				
				DateTime date = File.GetLastWriteTime (file);
				string fname = Path.GetFileName (file);
				PackageRepositoryEntry entry = (PackageRepositoryEntry) mainrep.FindEntry (fname);
				
				if (entry != null && date > rootLastModified) {
					mainrep.RemoveEntry (entry);
					DeleteSupportFiles (supportFileDir, entry.Addin);
					entry = null;
				}

				if (entry == null) {
					entry = new PackageRepositoryEntry ();
					AddinPackage p = (AddinPackage) Package.FromFile (file);
					entry.Addin = (AddinInfo) p.Addin;
					entry.Url = fname;
					entry.Addin.Properties.SetPropertyValue ("DownloadSize", new FileInfo (file).Length.ToString ());
					ExtractSupportFiles (supportFileDir, file, entry.Addin);
					mainrep.AddEntry (entry);
					modified = true;
					monitor.Log.WriteLine ("Added addin: " + fname);
				}
				allAddins.Add (entry);
			}
			
			ArrayList toRemove = new ArrayList ();
			foreach (PackageRepositoryEntry entry in mainrep.Addins) {
				if (!File.Exists (Path.Combine (mainPath, entry.Url))) {
					toRemove.Add (entry);
					modified = true;
				}
			}
					
			foreach (PackageRepositoryEntry entry in toRemove) {
				DeleteSupportFiles (supportFileDir, entry.Addin);
				mainrep.RemoveEntry (entry);
			}
			
			if (modified) {
				AddinStore.WriteObject (mainFile, mainrep);
				monitor.Log.WriteLine ("Updated " + relFilePath);
				lastModified = File.GetLastWriteTime (mainFile);
			}

			if (repEntry != null) {
				if (repEntry.LastModified < lastModified)
					repEntry.LastModified = lastModified;
			} else if (modified) {
				repEntry = new ReferenceRepositoryEntry ();
				repEntry.LastModified = lastModified;
				repEntry.Url = relFilePath;
				rootrep.AddEntry (repEntry);
			}
			
			foreach (string dir in Directory.GetDirectories (mainPath)) {
				if (Path.GetFileName (dir) == addinFilesDir)
					continue;
				string based = dir.Substring (rootPath.Length + 1);
				BuildRepository (monitor, rootrep, rootPath, Path.Combine (based, "main.mrep"), allAddins);
			}
		}
		
		void DeleteSupportFiles (string targetDir, AddinInfo ainfo)
		{
			foreach (var prop in ainfo.Properties) {
				if (prop.Value.StartsWith (addinFilesDir + Path.DirectorySeparatorChar)) {
					string file = Path.Combine (targetDir, Path.GetFileName (prop.Value));
					if (File.Exists (file))
						File.Delete (file);
				}
			}
			if (Directory.Exists (targetDir) && Directory.GetFileSystemEntries (targetDir).Length == 0)
				Directory.Delete (targetDir, true);
		}
		
		void ExtractSupportFiles (string targetDir, string file, AddinInfo ainfo)
		{
			Random r = new Random ();
			ZipFile zfile = new ZipFile (file);
			foreach (var prop in ainfo.Properties) {
				ZipEntry ze = zfile.GetEntry (prop.Value);
				if (ze != null) {
					string fname;
					do {
						fname = Path.Combine (targetDir, r.Next().ToString ("x") + Path.GetExtension (prop.Value));
					} while (File.Exists (fname));
					
					if (!Directory.Exists (targetDir))
						Directory.CreateDirectory (targetDir);
					
					using (var f = File.OpenWrite (fname)) {
						using (Stream s = zfile.GetInputStream (ze)) {
							byte[] buffer = new byte [8092];
							int nr = 0;
							while ((nr = s.Read (buffer, 0, buffer.Length)) > 0)
								f.Write (buffer, 0, nr);
						}
					}
					prop.Value = Path.Combine (addinFilesDir, Path.GetFileName (fname));
				}
			}
		}
		
		void GenerateIndexPage (Repository rep, ArrayList addins, string basePath)
		{
			StreamWriter sw = new StreamWriter (Path.Combine (basePath, "index.html"));
			sw.WriteLine ("<html><body>");
			sw.WriteLine ("<h1>Add-in Repository</h1>");
			if (rep.Name != null && rep.Name != "")
				sw.WriteLine ("<h2>" + rep.Name + "</h2>");
			sw.WriteLine ("<p>This is a list of add-ins available in this repository.</p>");
			sw.WriteLine ("<table border=1><thead><tr><th>Add-in</th><th>Version</th><th>Description</th></tr></thead>");
			
			foreach (PackageRepositoryEntry entry in addins) {
				sw.WriteLine ("<tr><td>" + entry.Addin.Name + "</td><td>" + entry.Addin.Version + "</td><td>" + entry.Addin.Description + "</td></tr>");
			}
			
			sw.WriteLine ("</table>");
			sw.WriteLine ("</body></html>");
			sw.Close ();
		}
		
		internal AddinSystemConfiguration Configuration {
			get {
				if (config == null) {
					config = (AddinSystemConfiguration) AddinStore.ReadObject (RootConfigFile, typeof(AddinSystemConfiguration));
					if (config == null)
						config = new AddinSystemConfiguration ();
				}
				return config;
			}
		}
		
		internal void SaveConfiguration ()
		{
			if (config != null) {
				AddinStore.WriteObject (RootConfigFile, config); 
			}
		}

		internal void ResetConfiguration ()
		{
			if (File.Exists (RootConfigFile))
				File.Delete (RootConfigFile);
			ResetAddinInfo ();
		}
				
		internal void ResetAddinInfo ()
		{
			if (Directory.Exists (RepositoryCachePath))
				Directory.Delete (RepositoryCachePath, true);
		}
		
		/// <summary>
		/// Gets a reference to an extensible application
		/// </summary>
		/// <param name="name">
		/// Name of the application
		/// </param>
		/// <returns>
		/// The Application object. Null if not found.
		/// </returns>
		public static Application GetExtensibleApplication (string name)
		{
			return GetExtensibleApplication (name, null);
		}
		
		/// <summary>
		/// Gets a reference to an extensible application
		/// </summary>
		/// <param name="name">
		/// Name of the application
		/// </param>
		/// <param name="searchPaths">
		/// Custom paths where to look for the application.
		/// </param>
		/// <returns>
		/// The Application object. Null if not found.
		/// </returns>
		public static Application GetExtensibleApplication (string name, IEnumerable<string> searchPaths)
		{
			AddinsPcFileCache pcc = GetAddinsPcFileCache (searchPaths);
			PackageInfo pi = pcc.GetPackageInfoByName (name, searchPaths);
			if (pi != null)
				return new Application (pi);
			else
				return null;
		}
		
		/// <summary>
		/// Gets a lis of all known extensible applications
		/// </summary>
		/// <returns>
		/// A list of applications.
		/// </returns>
		public static Application[] GetExtensibleApplications ()
		{
			return GetExtensibleApplications (null);
		}
		
		/// <summary>
		/// Gets a lis of all known extensible applications
		/// </summary>
		/// <param name="searchPaths">
		/// Custom paths where to look for applications.
		/// </param>
		/// <returns>
		/// A list of applications.
		/// </returns>
		public static Application[] GetExtensibleApplications (IEnumerable<string> searchPaths)
		{
			List<Application> list = new List<Application> ();
			
			AddinsPcFileCache pcc = GetAddinsPcFileCache (searchPaths);
			foreach (PackageInfo pinfo in pcc.GetPackages (searchPaths)) {
				if (pinfo.IsValidPackage)
					list.Add (new Application (pinfo));
			}
			return list.ToArray ();
		}
		
		static AddinsPcFileCache pcFileCache;
		
		static AddinsPcFileCache GetAddinsPcFileCache (IEnumerable<string> searchPaths)
		{
			if (pcFileCache == null) {
				pcFileCache = new AddinsPcFileCache ();
				if (searchPaths != null)
					pcFileCache.Update (searchPaths);
				else
					pcFileCache.Update ();
			}
			return pcFileCache;
		}
	}
	
	class AddinsPcFileCacheContext: IPcFileCacheContext
	{
		public bool IsCustomDataComplete (string pcfile, PackageInfo pkg)
		{
			return true;
		}
		
		public void StoreCustomData (Mono.PkgConfig.PcFile pcfile, PackageInfo pkg)
		{
		}

		public void ReportError (string message, System.Exception ex)
		{
			Console.WriteLine (message);
			Console.WriteLine (ex);
		}
	}
	
	class AddinsPcFileCache: PcFileCache
	{
		public AddinsPcFileCache (): base (new AddinsPcFileCacheContext ())
		{
		}
		
		protected override string CacheDirectory {
			get {
				string path = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
				path = Path.Combine (path, "mono.addins");
				return path;
			}
		}
		
		protected override void ParsePackageInfo (PcFile file, PackageInfo pinfo)
		{
			string rootPath = file.GetVariable ("MonoAddinsRoot");
			string regPath = file.GetVariable ("MonoAddinsRegistry");
			string addinsPath = file.GetVariable ("MonoAddinsInstallPath");
			string databasePath = file.GetVariable ("MonoAddinsCachePath");
			string testCmd = file.GetVariable ("MonoAddinsTestCommand");
			if (string.IsNullOrEmpty (rootPath) || string.IsNullOrEmpty (regPath))
				return;
			pinfo.SetData ("MonoAddinsRoot", rootPath);
			pinfo.SetData ("MonoAddinsRegistry", regPath);
			pinfo.SetData ("MonoAddinsInstallPath", addinsPath);
			pinfo.SetData ("MonoAddinsCachePath", databasePath);
			pinfo.SetData ("MonoAddinsTestCommand", testCmd);
		}
	}
	
	/// <summary>
	/// A registered extensible application
	/// </summary>
	public class Application
	{
		AddinRegistry registry;
		string description;
		string name;
		string testCommand;
		string startupPath;
		string registryPath;
		string addinsPath;
		string databasePath;
		
		internal Application (PackageInfo pinfo)
		{
			name = pinfo.Name;
			description = pinfo.Description;
			startupPath = pinfo.GetData ("MonoAddinsRoot");
			registryPath = pinfo.GetData ("MonoAddinsRegistry");
			addinsPath = pinfo.GetData ("MonoAddinsInstallPath");
			databasePath = pinfo.GetData ("MonoAddinsCachePath");
			testCommand = pinfo.GetData ("MonoAddinsTestCommand");
		}
		
		/// <summary>
		/// Add-in registry of the application
		/// </summary>
		public AddinRegistry Registry {
			get {
				if (registry == null)
					registry = new AddinRegistry (RegistryPath, StartupPath, AddinsPath, AddinCachePath);
				return registry;
			}
		}

		/// <summary>
		/// Description of the application
		/// </summary>
		public string Description {
			get {
				return description;
			}
		}

		/// <summary>
		/// Name of the application
		/// </summary>
		public string Name {
			get {
				return name;
			}
		}

		/// <summary>
		/// Path to the add-in registry
		/// </summary>
		public string RegistryPath {
			get {
				return registryPath;
			}
		}

		/// <summary>
		/// Path to the directory that contains the main executable assembly of the application
		/// </summary>
		public string StartupPath {
			get {
				return startupPath;
			}
		}

		/// <summary>
		/// Command to be used to execute the application in add-in development mode.
		/// </summary>
		public string TestCommand {
			get {
				return testCommand;
			}
		}

		/// <summary>
		/// Path to the default add-ins directory for the aplpication
		/// </summary>
		public string AddinsPath {
			get {
				return addinsPath;
			}
		}

		/// <summary>
		/// Path to the add-in cache for the application
		/// </summary>
		public string AddinCachePath {
			get {
				return databasePath;
			}
		}
	}
}
