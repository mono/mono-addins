//
// SetupDomain.cs
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
using System.Collections.Specialized;

namespace Mono.Addins.Database
{
	class SetupDomain: ISetupHandler
	{
		AppDomain domain;
		RemoteSetupDomain remoteSetupDomain;
		int useCount;
		
		public void Scan (IProgressStatus monitor, AddinRegistry registry, string scanFolder, string[] filesToIgnore)
		{
			RemoteProgressStatus remMonitor = new RemoteProgressStatus (monitor);
			try {
				RemoteSetupDomain rsd = GetDomain ();
				rsd.Scan (remMonitor, registry.RegistryPath, registry.StartupDirectory, registry.DefaultAddinsFolder, registry.AddinCachePath, scanFolder, filesToIgnore);
			} catch (Exception ex) {
				throw new ProcessFailedException (remMonitor.ProgessLog, ex);
			} finally {
				System.Runtime.Remoting.RemotingServices.Disconnect (remMonitor);
				ReleaseDomain ();
			}
		}
		
		public void GetAddinDescription (IProgressStatus monitor, AddinRegistry registry, string file, string outFile)
		{
			RemoteProgressStatus remMonitor = new RemoteProgressStatus (monitor);
			try {
				RemoteSetupDomain rsd = GetDomain ();
				rsd.GetAddinDescription (remMonitor, registry.RegistryPath, registry.StartupDirectory, registry.DefaultAddinsFolder, registry.AddinCachePath, file, outFile);
			} catch (Exception ex) {
				throw new ProcessFailedException (remMonitor.ProgessLog, ex);
			} finally {
				System.Runtime.Remoting.RemotingServices.Disconnect (remMonitor);
				ReleaseDomain ();
			}
		}
		
		RemoteSetupDomain GetDomain ()
		{
			lock (this) {
				if (useCount++ == 0) {
					domain = AppDomain.CreateDomain ("SetupDomain", null, AppDomain.CurrentDomain.SetupInformation);
					remoteSetupDomain = (RemoteSetupDomain) domain.CreateInstanceFromAndUnwrap (typeof(RemoteSetupDomain).Assembly.Location, typeof(RemoteSetupDomain).FullName);
				}
				return remoteSetupDomain;
			}
		}
		
		void ReleaseDomain ()
		{
			lock (this) {
				if (--useCount == 0) {
					AppDomain.Unload (domain);
					domain = null;
					remoteSetupDomain = null;
				}
			}
		}
	}
	
	class RemoteSetupDomain: MarshalByRefObject
	{
		public override object InitializeLifetimeService ()
		{
			return null;
		}
		
		public void Scan (IProgressStatus monitor, string registryPath, string startupDir, string addinsDir, string databaseDir, string scanFolder, string[] filesToIgnore)
		{
			AddinDatabase.RunningSetupProcess = true;
			AddinRegistry reg = new AddinRegistry (registryPath, startupDir, addinsDir, databaseDir);
			StringCollection files = new StringCollection ();
			for (int n=0; n<filesToIgnore.Length; n++)
				files.Add (filesToIgnore[n]);
			reg.ScanFolders (monitor, scanFolder, files);
		}
		
		public void GetAddinDescription (IProgressStatus monitor, string registryPath, string startupDir, string addinsDir, string databaseDir, string file, string outFile)
		{
			AddinDatabase.RunningSetupProcess = true;
			AddinRegistry reg = new AddinRegistry (registryPath, startupDir, addinsDir, databaseDir);
			reg.ParseAddin (monitor, file, outFile);
		}
	}
	
	class RemoteProgressStatus: MarshalByRefObject, IProgressStatus
	{
		IProgressStatus local;
		StringCollection progessLog = new StringCollection ();
		
		public RemoteProgressStatus (IProgressStatus local)
		{
			this.local = local;
		}
		
		public StringCollection ProgessLog {
			get { return progessLog; }
		}
		
		public override object InitializeLifetimeService ()
		{
			return null;
		}
		
		public void SetMessage (string msg)
		{
			local.SetMessage (msg);
		}
		
		public void SetProgress (double progress)
		{
			local.SetProgress (progress);
		}
		
		public void Log (string msg)
		{
			if (msg.StartsWith ("plog:"))
				progessLog.Add (msg.Substring (5));
			else
				local.Log (msg);
		}
		
		public void ReportWarning (string message)
		{
			local.ReportWarning (message);
		}
		
		public void ReportError (string message, Exception exception)
		{
			local.ReportError (message, exception);
		}
		
		public void Cancel ()
		{
			local.Cancel ();
		}
		
		public int LogLevel {
			get {
				return local.LogLevel;
			}
		}
		
		public bool IsCanceled {
			get {
				return local.IsCanceled;
			}
		}
	}
}
