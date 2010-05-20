//
// AddinManager.cs
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
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Mono.Addins.Localization;

namespace Mono.Addins
{
	public class AddinManager
	{
		static AddinEngine sessionService;

		private AddinManager ()
		{
		}
		
		public static void Initialize ()
		{
			// Code not shared with the other Initialize since I need to get the calling assembly
			Assembly asm = Assembly.GetEntryAssembly ();
			if (asm == null) asm = Assembly.GetCallingAssembly ();
			AddinEngine.Initialize (null, asm);
		}
		
		public static void Initialize (string configDir)
		{
			Assembly asm = Assembly.GetEntryAssembly ();
			if (asm == null) asm = Assembly.GetCallingAssembly ();
			AddinEngine.Initialize (configDir, asm);
		}
		
		public static void Shutdown ()
		{
			AddinEngine.Shutdown ();
		}
		
		public static void InitializeDefaultLocalizer (IAddinLocalizer localizer)
		{
			AddinEngine.InitializeDefaultLocalizer (localizer);
		}
		
		internal static string StartupDirectory {
			get { return AddinEngine.StartupDirectory; }
		}
		
		public static bool IsInitialized {
			get { return AddinEngine.IsInitialized; }
		}
		
		public static IAddinInstaller DefaultInstaller {
			get { return AddinEngine.DefaultInstaller; }
			set { AddinEngine.DefaultInstaller = value; }
		}
		
		public static AddinLocalizer DefaultLocalizer {
			get {
				return AddinEngine.DefaultLocalizer;
			}
		}
		
		public static AddinLocalizer CurrentLocalizer {
			get {
				AddinEngine.CheckInitialized ();
				RuntimeAddin addin = AddinEngine.GetAddinForAssembly (Assembly.GetCallingAssembly ());
				if (addin != null)
					return addin.Localizer;
				else
					return AddinEngine.DefaultLocalizer;
			}
		}
		
		public static RuntimeAddin CurrentAddin {
			get {
				AddinEngine.CheckInitialized ();
				return AddinEngine.GetAddinForAssembly (Assembly.GetCallingAssembly ());
			}
		}
		
		public static AddinEngine AddinEngine {
			get {
				if (sessionService == null)
					sessionService = new AddinEngine();
				
				return sessionService;
			}
		}
	
		public static AddinRegistry Registry {
			get {
				return AddinEngine.Registry;
			}
		}
		
		// This method checks if the specified add-ins are installed.
		// If some of the add-ins are not installed, it will use
		// the installer assigned to the DefaultAddinInstaller property
		// to install them. If the installation fails, or if DefaultAddinInstaller
		// is not set, an exception will be thrown.
		public static void CheckInstalled (string message, params string[] addinIds)
		{
			AddinEngine.CheckInstalled (message, addinIds);
		}
	
		public static bool IsAddinLoaded (string id)
		{
			return AddinEngine.IsAddinLoaded (id);
		}
		
		public static void LoadAddin (IProgressStatus statusMonitor, string id)
		{
			AddinEngine.LoadAddin (statusMonitor, id);
		}
		
		public static ExtensionContext CreateExtensionContext ()
		{
			return AddinEngine.CreateExtensionContext ();
		}
		
		public static ExtensionNode GetExtensionNode (string path)
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionNode (path);
		}
		
		public static ExtensionNode GetExtensionNode<T> (string path) where T:ExtensionNode
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionNode<T> (path);
		}
		
		public static ExtensionNodeList GetExtensionNodes (string path)
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionNodes (path);
		}
		
		public static ExtensionNodeList GetExtensionNodes (string path, Type type)
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionNodes (path, type);
		}
		
		public static ExtensionNodeList<T> GetExtensionNodes<T> (string path) where T:ExtensionNode
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionNodes<T> (path);
		}
		
		public static ExtensionNodeList GetExtensionNodes (Type instanceType)
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionNodes (instanceType);
		}
		
		public static ExtensionNodeList GetExtensionNodes (Type instanceType, Type expectedNodeType)
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionNodes (instanceType, expectedNodeType);
		}
		
		public static ExtensionNodeList<T> GetExtensionNodes<T> (Type instanceType) where T: ExtensionNode
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionNodes<T> (instanceType);
		}
		
		public static object[] GetExtensionObjects (Type instanceType)
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionObjects (instanceType);
		}
		
		public static T[] GetExtensionObjects<T> ()
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionObjects<T> ();
		}
		
		public static object[] GetExtensionObjects (Type instanceType, bool reuseCachedInstance)
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionObjects (instanceType, reuseCachedInstance);
		}
		
		public static T[] GetExtensionObjects<T> (bool reuseCachedInstance)
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionObjects<T> (reuseCachedInstance);
		}
		
		public static object[] GetExtensionObjects (string path)
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionObjects (path);
		}
		
		public static object[] GetExtensionObjects (string path, bool reuseCachedInstance)
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionObjects (path, reuseCachedInstance);
		}
		
		public static object[] GetExtensionObjects (string path, Type arrayElementType)
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionObjects (path, arrayElementType);
		}
		
		public static T[] GetExtensionObjects<T> (string path)
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionObjects<T> (path);
		}
		
		public static object[] GetExtensionObjects (string path, Type arrayElementType, bool reuseCachedInstance)
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionObjects (path, arrayElementType, reuseCachedInstance);
		}
		
		public static T[] GetExtensionObjects<T> (string path, bool reuseCachedInstance)
		{
			AddinEngine.CheckInitialized ();
			return AddinEngine.GetExtensionObjects<T> (path, reuseCachedInstance);
		}
		
		public static event ExtensionEventHandler ExtensionChanged {
			add { AddinEngine.CheckInitialized(); AddinEngine.ExtensionChanged += value; }
			remove { AddinEngine.CheckInitialized(); AddinEngine.ExtensionChanged -= value; }
		}
		
		public static void AddExtensionNodeHandler (string path, ExtensionNodeEventHandler handler)
		{
			AddinEngine.CheckInitialized ();
			AddinEngine.AddExtensionNodeHandler (path, handler);
		}
		
		public static void RemoveExtensionNodeHandler (string path, ExtensionNodeEventHandler handler)
		{
			AddinEngine.CheckInitialized ();
			AddinEngine.RemoveExtensionNodeHandler (path, handler);
		}
		
		public static event AddinErrorEventHandler AddinLoadError {
			add { AddinEngine.AddinLoadError += value; }
			remove { AddinEngine.AddinLoadError -= value; }
		}
		
		public static event AddinEventHandler AddinLoaded {
			add { AddinEngine.AddinLoaded += value; }
			remove { AddinEngine.AddinLoaded -= value; }
		}
		
		public static event AddinEventHandler AddinUnloaded {
			add { AddinEngine.AddinUnloaded += value; }
			remove { AddinEngine.AddinUnloaded -= value; }
		}
	}

}
