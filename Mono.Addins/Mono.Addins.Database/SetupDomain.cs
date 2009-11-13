// http://bugzilla.novell.com/enter_bug.cgi?alias=&assigned_to=&blocked=&bug_file_loc=http%3A%2F%2F&bug_severity=Normal&bug_status=NEW&cf_foundby=---&cf_nts_priority=&cf_nts_support_num=&cf_partnerid=&comment=Description%20of%20Problem%3A%0D%0A%0D%0A%0D%0ASteps%20to%20reproduce%20the%20problem%3A%0D%0A1.%20%0D%0A2.%20%0D%0A%0D%0A%0D%0AActual%20Results%3A%0D%0A%0D%0A%0D%0AExpected%20Results%3A%0D%0A%0D%0A%0D%0AHow%20often%20does%20this%20happen%3F%20%0D%0A%0D%0A%0D%0AAdditional%20Information%3A%0D%0A%0D%0A%0D%0A&component=&contenttypeentry=&contenttypemethod=autodetect&contenttypeselection=text%2Fplain&data=&deadline=&dependson=&description=&estimated_time=0.0&flag_type-2=X&form_name=enter_bug&keywords=&maketemplate=Remember%20values%20as%20bookmarkable%20template&op_sys=Other&priority=P5%20-%20None&product=MonoDevelop%20&qa_contact=&rep_platform=Other&short_desc=&version=unspecified
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
		
		public void Scan (IProgressStatus monitor, string registryPath, string startupDir, string scanFolder, string[] filesToIgnore)
		{
			RemoteProgressStatus remMonitor = new RemoteProgressStatus (monitor);
			try {
				RemoteSetupDomain rsd = GetDomain ();
				rsd.Scan (remMonitor, registryPath, startupDir, scanFolder, filesToIgnore);
			} catch (Exception ex) {
				throw new ProcessFailedException (remMonitor.ProgessLog, ex);
			} finally {
				System.Runtime.Remoting.RemotingServices.Disconnect (remMonitor);
				ReleaseDomain ();
			}
		}
		
		public void GetAddinDescription (IProgressStatus monitor, string registryPath, string startupDir, string file, string outFile)
		{
			RemoteProgressStatus remMonitor = new RemoteProgressStatus (monitor);
			try {
				RemoteSetupDomain rsd = GetDomain ();
				rsd.GetAddinDescription (remMonitor, registryPath, startupDir, file, outFile);
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
					domain = AppDomain.CreateDomain ("SetupDomain");
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
		
		public void Scan (IProgressStatus monitor, string registryPath, string startupDir, string scanFolder, string[] filesToIgnore)
		{
			AddinDatabase.RunningSetupProcess = true;
			AddinRegistry reg = new AddinRegistry (registryPath, startupDir);
			StringCollection files = new StringCollection ();
			for (int n=3; n<filesToIgnore.Length; n++)
				files.Add (filesToIgnore[n]);
			reg.ScanFolders (monitor, scanFolder, files);
		}
		
		public void GetAddinDescription (IProgressStatus monitor, string registryPath, string startupDir, string file, string outFile)
		{
			AddinDatabase.RunningSetupProcess = true;
			AddinRegistry reg = new AddinRegistry (registryPath, startupDir);
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
