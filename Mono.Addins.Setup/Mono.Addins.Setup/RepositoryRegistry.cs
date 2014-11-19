//
// RepositoryRegistry.cs
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
using System.Linq;
using System.IO;
using System.Collections;
using Mono.Addins.Setup.ProgressMonitoring;
using System.Collections.Generic;

namespace Mono.Addins.Setup
{
	/// <summary>
	/// A registry of on-line repositories
	/// </summary>
	/// <remarks>
	/// This class can be used to manage on-line repository subscriptions.
	/// </remarks>
	public class RepositoryRegistry
	{
		ArrayList repoList;
		SetupService service;
		
		internal RepositoryRegistry (SetupService service)
		{
			this.service = service;
		}
		
		/// <summary>
		/// Subscribes to an on-line repository
		/// </summary>
		/// <param name="monitor">
		/// Progress monitor where to show progress status and log
		/// </param>
		/// <param name="url">
		/// URL of the repository
		/// </param>
		/// <returns>
		/// A repository reference
		/// </returns>
		/// <remarks>
		/// The repository index is not downloaded by default. It can be downloaded
		/// by calling UpdateRepository.
		/// </remarks>
		public AddinRepository RegisterRepository (IProgressStatus monitor, string url)
		{
			return RegisterRepository (monitor, url, false);
		}
		
		/// <summary>
		/// Subscribes to an on-line repository
		/// </summary>
		/// <param name="monitor">
		/// Progress monitor where to show progress status and log
		/// </param>
		/// <param name="url">
		/// URL of the repository
		/// </param>
		/// <param name="updateNow">
		/// When set to True, the repository index will be downloaded.
		/// </param>
		/// <returns>
		/// A repository reference
		/// </returns>
		public AddinRepository RegisterRepository (IProgressStatus monitor, string url, bool updateNow)
		{
			if (string.IsNullOrEmpty (url))
				throw new ArgumentException ("Emtpy url");
			
			if (!url.EndsWith (".mrep")) {
				if (url [url.Length - 1] != '/')
					url += "/";
				url = url + "main.mrep";
			}
			
			RepositoryRecord rr = FindRepositoryRecord (url);
			if (rr != null)
				return rr;

			rr = RegisterRepository (url, false);
			
			try {
				if (updateNow) {
					UpdateRepository (monitor, url);
					rr = FindRepositoryRecord (url);
					Repository rep = rr.GetCachedRepository ();
					if (rep != null)
						rr.Name = rep.Name;
				}
				service.SaveConfiguration ();
				return rr;
			} catch (Exception ex) {
				if (monitor != null)
					monitor.ReportError ("The repository could not be registered", ex);
				if (ContainsRepository (url))
					RemoveRepository (url);
				return null;
			}
		}
		
		internal RepositoryRecord RegisterRepository (string url, bool isReference)
		{
			RepositoryRecord rr = FindRepositoryRecord (url);
			if (rr != null) {
				if (rr.IsReference && !isReference) {
					rr.IsReference = false;
					service.SaveConfiguration ();
				}
				return rr;
			}
			
			rr = new RepositoryRecord ();
			rr.Url = url;
			rr.IsReference = isReference;
			
			string name = service.RepositoryCachePath;
			if (!Directory.Exists (name))
				Directory.CreateDirectory (name);
			string host = new Uri (url).Host;
			if (host.Length == 0)
				host = "repo";
			name = Path.Combine (name, host);
			rr.File = name + "_" + service.Configuration.RepositoryIdCount + ".mrep";
			
			rr.Id = "rep" + service.Configuration.RepositoryIdCount;
			service.Configuration.Repositories.Add (rr);
			service.Configuration.RepositoryIdCount++;
			service.SaveConfiguration ();
			repoList = null;
			return rr;
		}
		
		internal RepositoryRecord FindRepositoryRecord (string url)
		{
			foreach (RepositoryRecord rr in service.Configuration.Repositories)
				if (rr.Url == url) return rr;
			return null;
		}
		
		/// <summary>
		/// Removes an on-line repository subscription.
		/// </summary>
		/// <param name="url">
		/// URL of the repository.
		/// </param>
		public void RemoveRepository (string url)
		{
			RepositoryRecord rep = FindRepositoryRecord (url);
			if (rep == null)
				return; // Nothing to do

			rep.IsReference = true;
			PurgeUnusedRepositories ();
			service.SaveConfiguration ();
			repoList = null;
		}

		void PurgeUnusedRepositories ()
		{
			bool changed;

			do {
				changed = false;

				HashSet<string> referencedRepos = new HashSet<string> ();

				// Get all referenced repos
				foreach (RepositoryRecord rr in service.Configuration.Repositories) {
					Repository repInfo = rr.GetCachedRepository ();
					if (repInfo == null)
						continue;

					foreach (ReferenceRepositoryEntry re in repInfo.Repositories)
						referencedRepos.Add (new Uri (new Uri (repInfo.Url), re.Url).ToString ());
				}

				foreach (RepositoryRecord rr in service.Configuration.Repositories.ToArray ()) {
					if (rr.IsReference && !referencedRepos.Contains (rr.Url)) {
						changed = true;
						service.Configuration.Repositories.Remove (rr);
						rr.ClearCachedRepository ();
					}
				}
			}
			while (changed);
		}
		
		/// <summary>
		/// Enables or disables a repository
		/// </summary>
		/// <param name='url'>
		/// URL of the repository
		/// </param>
		/// <param name='enabled'>
		/// 'true' if the repository has to be enabled.
		/// </param>
		/// <remarks>
		/// Disabled repositories are ignored when calling UpdateAllRepositories.
		/// </remarks>
		public void SetRepositoryEnabled (string url, bool enabled)
		{
			RepositoryRecord rep = FindRepositoryRecord (url);
			if (rep == null)
				return; // Nothing to do
			rep.Enabled = enabled;
			Repository crep = rep.GetCachedRepository ();
			if (crep != null) {
				foreach (RepositoryEntry re in crep.Repositories)
					SetRepositoryEnabled (new Uri (new Uri (url), re.Url).ToString (), enabled);
			}
				
			service.SaveConfiguration ();
		}
		
		/// <summary>
		/// Checks if a repository is already subscribed.
		/// </summary>
		/// <param name="url">
		/// URL of the repository
		/// </param>
		/// <returns>
		/// True if the repository is already subscribed.
		/// </returns>
		public bool ContainsRepository (string url)
		{
			return FindRepositoryRecord (url) != null;
		}
		
		ArrayList RepositoryList {
			get {
				if (repoList == null) {
					ArrayList list = new ArrayList ();
					foreach (RepositoryRecord rep in service.Configuration.Repositories) {
						if (!rep.IsReference)
							list.Add (rep);
					}
					repoList = list;
				}
				return repoList;
			}
		}
		
		/// <summary>
		/// Gets a list of subscribed repositories
		/// </summary>
		/// <returns>
		/// A list of repositories.
		/// </returns>
		public AddinRepository[] GetRepositories ()
		{
			return (AddinRepository[]) RepositoryList.ToArray (typeof(AddinRepository));
		}
			                     
		/// <summary>
		/// Updates the add-in index of all subscribed repositories.
		/// </summary>
		/// <param name="monitor">
		/// Progress monitor where to show progress status and log
		/// </param>
		public void UpdateAllRepositories (IProgressStatus monitor)
		{
			UpdateRepository (monitor, (string)null);
		}
		
		/// <summary>
		/// Updates the add-in index of the provided repository
		/// </summary>
		/// <param name="statusMonitor">
		/// Progress monitor where to show progress status and log
		/// </param>
		/// <param name="url">
		/// URL of the repository
		/// </param>
		public void UpdateRepository (IProgressStatus statusMonitor, string url)
		{
			repoList = null;
			
			IProgressMonitor monitor = ProgressStatusMonitor.GetProgressMonitor (statusMonitor);
		
			monitor.BeginTask ("Updating repositories", service.Configuration.Repositories.Count);
			try {
				int num = service.Configuration.Repositories.Count;
				for (int n=0; n<num; n++) {
					RepositoryRecord rr = (RepositoryRecord) service.Configuration.Repositories [n];
					if (((url == null && rr.Enabled) || rr.Url == url) && !rr.IsReference)
						UpdateRepository (monitor, new Uri (rr.Url), rr);
					monitor.Step (1);
				}
			} catch (Exception ex) {
				statusMonitor.ReportError ("Could not get information from repository", ex);
				return;
			} finally {
				monitor.EndTask ();
			}

			PurgeUnusedRepositories ();
			service.SaveConfiguration ();
		}

		void UpdateRepository (IProgressMonitor monitor, Uri baseUri, RepositoryRecord rr)
		{
			Uri absUri = new Uri (baseUri, rr.Url);
			monitor.BeginTask ("Updating from " + absUri.ToString (), 2);
			Repository newRep = null;
			Exception error = null;
			
			try {
				newRep = (Repository) service.Store.DownloadObject (monitor, absUri.ToString (), typeof(Repository));
			} catch (Exception ex) {
				error = ex;
			}
			
			if (newRep == null) {
				monitor.ReportError ("Could not get information from repository" + ": " + absUri.ToString (), error);
				return;
			}
			
			monitor.Step (1);
			
			foreach (ReferenceRepositoryEntry re in newRep.Repositories) {
				Uri refRepUri = new Uri (absUri, re.Url);
				string refRepUrl = refRepUri.ToString ();
				RepositoryRecord refRep = FindRepositoryRecord (refRepUrl);
				if (refRep == null)
					refRep = RegisterRepository (refRepUrl, true);
				refRep.Enabled = rr.Enabled;
				// Update the repo if the modified timestamp changes or if there is no timestamp info
				if (refRep.LastModified != re.LastModified || re.LastModified == DateTime.MinValue || !File.Exists (refRep.File)) {
					refRep.LastModified = re.LastModified;
					UpdateRepository (monitor, refRepUri, refRep);
				}
			}
			monitor.EndTask ();
			rr.UpdateCachedRepository (newRep);
		}
		
		/// <summary>
		/// Gets a list of available add-in updates.
		/// </summary>
		/// <returns>
		/// A list of add-in references.
		/// </returns>
		/// <remarks>
		/// The list is generated by looking at the add-ins currently installed and checking if there is any
		/// add-in with a newer version number in any of the subscribed repositories. This method uses cached
		/// information from on-line repositories. Make sure you call UpdateRepository or UpdateAllRepositories
		/// before using this method to ensure that the latest information is available.
		/// </remarks>
		public AddinRepositoryEntry[] GetAvailableUpdates ()
		{
			return GetAvailableAddin (null, null, null, true, RepositorySearchFlags.None);
		}

		/// <summary>
		/// Gets a list of available add-in updates.
		/// </summary>
		/// <param name="flags">
		/// Search flags
		/// </param>
		/// <returns>
		/// A list of add-in references.
		/// </returns>
		/// <remarks>
		/// The list is generated by looking at the add-ins currently installed and checking if there is any
		/// add-in with a newer version number in any of the subscribed repositories. This method uses cached
		/// information from on-line repositories. Make sure you call UpdateRepository or UpdateAllRepositories
		/// before using this method to ensure that the latest information is available.
		/// </remarks>
		public AddinRepositoryEntry[] GetAvailableUpdates (RepositorySearchFlags flags)
		{
			return GetAvailableAddin (null, null, null, true, flags);
		}

		/// <summary>
		/// Gets a list of available add-in updates in a specific repository.
		/// </summary>
		/// <param name="repositoryUrl">
		/// The repository URL
		/// </param>
		/// <returns>
		/// A list of add-in references.
		/// </returns>
		/// <remarks>
		/// The list is generated by looking at the add-ins currently installed and checking if there is any
		/// add-in with a newer version number in the provided repository. This method uses cached
		/// information from on-line repositories. Make sure you call UpdateRepository or UpdateAllRepositories
		/// before using this method to ensure that the latest information is available.
		/// </remarks>
		public AddinRepositoryEntry[] GetAvailableUpdates (string repositoryUrl)
		{
			return GetAvailableAddin (repositoryUrl, null, null, true, RepositorySearchFlags.None);
		}
		
#pragma warning disable 1591
		[Obsolete ("Use GetAvailableAddinUpdates (id) instead")]
		public AddinRepositoryEntry[] GetAvailableUpdates (string id, string version)
		{
			return GetAvailableAddin (null, id, version, true, RepositorySearchFlags.None);
		}
		
		[Obsolete ("Use GetAvailableAddinUpdates (repositoryUrl, id) instead")]
		public AddinRepositoryEntry[] GetAvailableUpdates (string repositoryUrl, string id, string version)
		{
			return GetAvailableAddin (repositoryUrl, id, version, true, RepositorySearchFlags.None);
		}
#pragma warning restore 1591
		
		/// <summary>
		/// Gets a list of available updates for an add-in.
		/// </summary>
		/// <param name="id">
		/// Identifier of the add-in.
		/// </param>
		/// <returns>
		/// List of updates for the specified add-in.
		/// </returns>
		/// <remarks>
		/// The list is generated by checking if there is any
		/// add-in with a newer version number in any of the subscribed repositories. This method uses cached
		/// information from on-line repositories. Make sure you call UpdateRepository or UpdateAllRepositories
		/// before using this method to ensure that the latest information is available.
		/// </remarks>
		public AddinRepositoryEntry[] GetAvailableAddinUpdates (string id)
		{
			return GetAvailableAddin (null, id, null, true, RepositorySearchFlags.None);
		}
		
		/// <summary>
		/// Gets a list of available updates for an add-in.
		/// </summary>
		/// <param name="id">
		/// Identifier of the add-in.
		/// </param>
		/// <param name='flags'>
		/// Search flags.
		/// </param>
		/// <returns>
		/// List of updates for the specified add-in.
		/// </returns>
		/// <remarks>
		/// The list is generated by checking if there is any
		/// add-in with a newer version number in any of the subscribed repositories. This method uses cached
		/// information from on-line repositories. Make sure you call UpdateRepository or UpdateAllRepositories
		/// before using this method to ensure that the latest information is available.
		/// </remarks>
		public AddinRepositoryEntry[] GetAvailableAddinUpdates (string id, RepositorySearchFlags flags)
		{
			return GetAvailableAddin (null, id, null, true, flags);
		}
		
		/// <summary>
		/// Gets a list of available updates for an add-in in a specific repository
		/// </summary>
		/// <param name="repositoryUrl">
		/// Identifier of the add-in.
		/// </param>
		/// <param name="id">
		/// Identifier of the add-in.
		/// </param>
		/// <returns>
		/// List of updates for the specified add-in.
		/// </returns>
		/// <remarks>
		/// The list is generated by checking if there is any
		/// add-in with a newer version number in the provided repository. This method uses cached
		/// information from on-line repositories. Make sure you call UpdateRepository or UpdateAllRepositories
		/// before using this method to ensure that the latest information is available.
		/// </remarks>
		public AddinRepositoryEntry[] GetAvailableAddinUpdates (string repositoryUrl, string id)
		{
			return GetAvailableAddin (repositoryUrl, id, null, true, RepositorySearchFlags.None);
		}
		
		/// <summary>
		/// Gets a list of available updates for an add-in in a specific repository
		/// </summary>
		/// <param name="repositoryUrl">
		/// Identifier of the add-in.
		/// </param>
		/// <param name="id">
		/// Identifier of the add-in.
		/// </param>
		/// <param name='flags'>
		/// Search flags.
		/// </param>
		/// <returns>
		/// List of updates for the specified add-in.
		/// </returns>
		/// <remarks>
		/// The list is generated by checking if there is any
		/// add-in with a newer version number in the provided repository. This method uses cached
		/// information from on-line repositories. Make sure you call UpdateRepository or UpdateAllRepositories
		/// before using this method to ensure that the latest information is available.
		/// </remarks>
		public AddinRepositoryEntry[] GetAvailableAddinUpdates (string repositoryUrl, string id, RepositorySearchFlags flags)
		{
			return GetAvailableAddin (repositoryUrl, id, null, true, flags);
		}
		
		/// <summary>
		/// Gets a list of all available add-ins
		/// </summary>
		/// <returns>
		/// A list of add-ins
		/// </returns>
		/// <remarks>
		/// This method uses cached
		/// information from on-line repositories. Make sure you call UpdateRepository or UpdateAllRepositories
		/// before using this method to ensure that the latest information is available.
		/// </remarks>
		public AddinRepositoryEntry[] GetAvailableAddins ()
		{
			return GetAvailableAddin (null, null, null, false, RepositorySearchFlags.None);
		}
		
		/// <summary>
		/// Gets a list of all available add-ins
		/// </summary>
		/// <returns>
		/// The available addins.
		/// </returns>
		/// <param name='flags'>
		/// Search flags.
		/// </param>
		/// <remarks>
		/// This method uses cached
		/// information from on-line repositories. Make sure you call UpdateRepository or UpdateAllRepositories
		/// before using this method to ensure that the latest information is available.
		/// </remarks>
		public AddinRepositoryEntry[] GetAvailableAddins (RepositorySearchFlags flags)
		{
			return GetAvailableAddin (null, null, null, false, flags);
		}
		
		/// <summary>
		/// Gets a list of all available add-ins in a repository
		/// </summary>
		/// <param name="repositoryUrl">
		/// A repository URL
		/// </param>
		/// <returns>
		/// A list of add-ins
		/// </returns>
		/// <remarks>
		/// This method uses cached
		/// information from on-line repositories. Make sure you call UpdateRepository or UpdateAllRepositories
		/// before using this method to ensure that the latest information is available.
		/// </remarks>
		public AddinRepositoryEntry[] GetAvailableAddins (string repositoryUrl)
		{
			return GetAvailableAddin (repositoryUrl, null, null);
		}
		
		/// <summary>
		/// Gets a list of all available add-ins in a repository
		/// </summary>
		/// <param name="repositoryUrl">
		/// A repository URL
		/// </param>
		/// <param name='flags'>
		/// Search flags.
		/// </param>
		/// <returns>
		/// A list of add-ins
		/// </returns>
		/// <remarks>
		/// This method uses cached
		/// information from on-line repositories. Make sure you call UpdateRepository or UpdateAllRepositories
		/// before using this method to ensure that the latest information is available.
		/// </remarks>
		public AddinRepositoryEntry[] GetAvailableAddins (string repositoryUrl, RepositorySearchFlags flags)
		{
			return GetAvailableAddin (repositoryUrl, null, null, false, flags);
		}
		
		/// <summary>
		/// Checks if an add-in is available to be installed
		/// </summary>
		/// <param name="id">
		/// Identifier of the add-in
		/// </param>
		/// <param name="version">
		/// Version of the add-in (optional, it can be null)
		/// </param>
		/// <returns>
		/// A list of add-ins
		/// </returns>
		/// <remarks>
		/// List of references to add-ins available in on-line repositories. This method uses cached
		/// information from on-line repositories. Make sure you call UpdateRepository or UpdateAllRepositories
		/// before using this method to ensure that the latest information is available.
		/// </remarks>
		public AddinRepositoryEntry[] GetAvailableAddin (string id, string version)
		{
			return GetAvailableAddin (null, id, version);
		}
		
		/// <summary>
		/// Checks if an add-in is available to be installed from a repository
		/// </summary>
		/// <param name="repositoryUrl">
		/// A repository URL
		/// </param>
		/// <param name="id">
		/// Identifier of the add-in
		/// </param>
		/// <param name="version">
		/// Version of the add-in (optional, it can be null)
		/// </param>
		/// <returns>
		/// A list of add-ins
		/// </returns>
		/// <remarks>
		/// List of references to add-ins available in the repository. This method uses cached
		/// information from on-line repositories. Make sure you call UpdateRepository or UpdateAllRepositories
		/// before using this method to ensure that the latest information is available.
		/// </remarks>
		public AddinRepositoryEntry[] GetAvailableAddin (string repositoryUrl, string id, string version)
		{
			return GetAvailableAddin (repositoryUrl, id, version, false, RepositorySearchFlags.None);
		}
		
		PackageRepositoryEntry[] GetAvailableAddin (string repositoryUrl, string id, string version, bool updates, RepositorySearchFlags flags)
		{
			List<PackageRepositoryEntry> list = new List<PackageRepositoryEntry> ();
			
			IEnumerable ee;
			if (repositoryUrl != null) {
				ArrayList repos = new ArrayList ();
				GetRepositoryTree (repositoryUrl, repos);
				ee = repos;
			} else
				ee = service.Configuration.Repositories;
			
			foreach (RepositoryRecord rr in ee) {
				if (!rr.Enabled)
					continue;
				Repository rep = rr.GetCachedRepository();
				if (rep == null) continue;
				foreach (PackageRepositoryEntry addin in rep.Addins) {
					if ((id == null || Addin.GetIdName (addin.Addin.Id) == id) && (version == null || addin.Addin.Version == version)) {
						if (updates) {
							Addin ainfo = service.Registry.GetAddin (Addin.GetIdName (addin.Addin.Id));
							if (ainfo == null || Addin.CompareVersions (ainfo.Version, addin.Addin.Version) <= 0)
								continue;
						}
						list.Add (addin);
					}
				}
			}
			
			if ((flags & RepositorySearchFlags.LatestVersionsOnly) != 0)
				FilterOldVersions (list);
			
			// Old versions are returned first
			list.Sort ();
			return list.ToArray ();
		}
		
		void FilterOldVersions (List<PackageRepositoryEntry> addins)
		{
			Dictionary<string,string> versions = new Dictionary<string, string> ();
			foreach (PackageRepositoryEntry a in addins) {
				string last;
				string id, version;
				Addin.GetIdParts (a.Addin.Id, out id, out version);
				if (!versions.TryGetValue (id, out last) || Addin.CompareVersions (last, version) > 0)
					versions [id] = version;
			}
			for (int n=0; n<addins.Count; n++) {
				PackageRepositoryEntry a = addins [n];
				string id, version;
				Addin.GetIdParts (a.Addin.Id, out id, out version);
				if (versions [id] != version)
					addins.RemoveAt (n--);
			}
		}
		
		void GetRepositoryTree (string url, ArrayList list)
		{
			RepositoryRecord rr = FindRepositoryRecord (url);
			if (rr == null) return;
			
			if (list.Contains (rr))
				return;
				
			list.Add (rr);
			Repository rep = rr.GetCachedRepository ();
			if (rep == null)
				return;
			
			Uri absUri = new Uri (url);
			foreach (ReferenceRepositoryEntry re in rep.Repositories) {
				Uri refRepUri = new Uri (absUri, re.Url);
				GetRepositoryTree (refRepUri.ToString (), list);
			}
		}
	}

	/// <summary>
	/// Repository search flags.
	/// </summary>
	public enum RepositorySearchFlags
	{
		/// <summary>
		/// No special search options
		/// </summary>
		None,
		
		/// <summary>
		/// Only the latest version of every add-in is included in the search
		/// </summary>
		LatestVersionsOnly = 1,
	}
}
