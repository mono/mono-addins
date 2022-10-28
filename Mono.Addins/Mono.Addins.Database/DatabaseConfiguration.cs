//
// DatabaseConfiguration.cs
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
using System.Collections.Generic;
using System.Xml;
using System.Collections.Immutable;

namespace Mono.Addins.Database
{
	internal class DatabaseConfiguration
	{
		ImmutableDictionary<string, AddinStatus> addinStatus = ImmutableDictionary<string, AddinStatus>.Empty;

		internal class AddinStatus
		{
			public AddinStatus (string addinId, bool configEnabled = false, bool? sessionEnabled = null, bool uninstalled = false, ImmutableArray<string> files = default)
			{
				this.AddinId = addinId;
				ConfigEnabled = configEnabled;
				SessionEnabled = sessionEnabled;
				Uninstalled = uninstalled;
				if (files.IsDefault)
					Files = ImmutableArray<string>.Empty;
				else
					Files = files;
			}

			AddinStatus Copy ()
			{
				return new AddinStatus (AddinId,
					configEnabled: ConfigEnabled,
					sessionEnabled: SessionEnabled,
					uninstalled: Uninstalled,
					files: Files
				);
			}

			public AddinStatus AsEnabled (bool enabled, bool sessionOnly)
			{
				var copy = Copy ();
				if (sessionOnly)
					copy.SessionEnabled = enabled;
				else {
					copy.ConfigEnabled = enabled;
					copy.SessionEnabled = null;
				}
				return copy;
			}

			public AddinStatus AsUninstalled (ImmutableArray<string> oldAddinFiles)
			{
				var copy = Copy ();
				copy.ConfigEnabled = false;
				copy.Uninstalled = true;
				copy.Files = oldAddinFiles;
				return copy;
			}

			public string AddinId { get; private set;}
			public bool ConfigEnabled { get; private set; }
			public bool? SessionEnabled { get; private set; }
			public bool Uninstalled { get; private set; }
			public ImmutableArray<string> Files { get; private set; }

			public bool Enabled {
				get { return SessionEnabled ?? ConfigEnabled; }
			}
		}

		public bool IsEnabled (string addinId, bool defaultValue)
		{
			var addinName = Addin.GetIdName (addinId);

			AddinStatus s;

			var status = addinStatus;

			// If the add-in is globaly disabled, it is disabled no matter what the version specific status is
			if (status.TryGetValue (addinName, out s)) {
				if (!s.Enabled)
					return false;
			}

			if (status.TryGetValue (addinId, out s))
				return s.Enabled && !s.Uninstalled;
			else
				return defaultValue;
		}
		
		public void SetEnabled (AddinDatabaseTransaction transaction, string addinId, bool enabled, bool defaultValue, bool exactVersionMatch, bool onlyForTheSession = false)
		{
			if (IsRegisteredForUninstall (addinId))
				return;

			var addinName = exactVersionMatch ? addinId : Addin.GetIdName (addinId);

			if (!addinStatus.TryGetValue (addinName, out var s))
				s = new AddinStatus (addinName);

			s = s.AsEnabled (enabled, onlyForTheSession);
			addinStatus = addinStatus.SetItem (addinName, s);

			// If enabling a specific version of an add-in, make sure the add-in is enabled as a whole
			if (enabled && exactVersionMatch)
				SetEnabled (transaction, addinId, true, defaultValue, false, onlyForTheSession);
		}
		
		public void RegisterForUninstall (AddinDatabaseTransaction transaction, string addinId, IEnumerable<string> files)
		{
			AddinStatus s;
			if (!addinStatus.TryGetValue (addinId, out s))
				s = new AddinStatus (addinId);
			
			s = s.AsUninstalled (ImmutableArray<string>.Empty.AddRange(files));
			addinStatus = addinStatus.SetItem (addinId, s);
		}

		public void UnregisterForUninstall (AddinDatabaseTransaction transaction, string addinId)
		{
			addinStatus = addinStatus.Remove (addinId);
		}
		
		public bool IsRegisteredForUninstall (string addinId)
		{
			AddinStatus s;
			if (addinStatus.TryGetValue (addinId, out s))
				return s.Uninstalled;
			else
				return false;
		}
		
		public bool HasPendingUninstalls {
			get { return addinStatus.Values.Where (s => s.Uninstalled).Any (); }
		}
		
		public AddinStatus[] GetPendingUninstalls ()
		{
			return addinStatus.Values.Where (s => s.Uninstalled).ToArray ();
		}
		
		public static DatabaseConfiguration Read (string file)
		{
			var config = ReadInternal (file);
			// Try to read application level config to support disabling add-ins by default.
			var appConfig = ReadAppConfig ();

			if (appConfig == null)
				return config;

			// Overwrite app config values with user config values
			appConfig.addinStatus = appConfig.addinStatus.SetItems (config.addinStatus);

			return appConfig;
		}
		
		public static DatabaseConfiguration ReadAppConfig()
		{
			var assemblyPath = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			var assemblyDirectory = Path.GetDirectoryName (assemblyPath);
			var appAddinsConfigFilePath = Path.Combine (assemblyDirectory, "addins-config.xml");

			if (!File.Exists (appAddinsConfigFilePath))
				return new DatabaseConfiguration ();

			return ReadInternal (appAddinsConfigFilePath);
		}

		static DatabaseConfiguration ReadInternal (string file)
		{
			DatabaseConfiguration config = new DatabaseConfiguration ();
			XmlDocument doc = new XmlDocument ();
			doc.Load (file);
			
			XmlElement disabledElem = (XmlElement) doc.DocumentElement.SelectSingleNode ("DisabledAddins");
			if (disabledElem != null) {
				// For back compatibility
				var dictionary = ImmutableDictionary.CreateBuilder<string, AddinStatus> ();
				foreach (XmlElement elem in disabledElem.SelectNodes ("Addin")) {
					AddinStatus status = new AddinStatus (Addin.GetIdName (elem.GetAttribute ("id")), configEnabled: false);
					dictionary [status.AddinId] = status;
				}
				config.addinStatus = dictionary.ToImmutable ();
				return config;
			}

			XmlElement statusElem = (XmlElement) doc.DocumentElement.SelectSingleNode ("AddinStatus");
			if (statusElem != null) {
				var dictionary = ImmutableDictionary.CreateBuilder<string, AddinStatus> ();
				foreach (XmlElement elem in statusElem.SelectNodes ("Addin")) {
					string senabled = elem.GetAttribute ("enabled");

					AddinStatus status = new AddinStatus (elem.GetAttribute ("id"),
						configEnabled: senabled.Length == 0 || senabled == "True",
						uninstalled: elem.GetAttribute ("uninstalled") == "True",
						files: elem.SelectNodes ("File").OfType<XmlElement> ().Select (fileElem => fileElem.InnerText).ToImmutableArray ()
					);
					dictionary [status.AddinId] = status;
				}
				config.addinStatus = dictionary.ToImmutable ();
			}
			return config;
		}
		
		public void Write (string file)
		{
			StreamWriter s = new StreamWriter (file);
			using (s) {
				XmlTextWriter tw = new XmlTextWriter (s);
				tw.Formatting = Formatting.Indented;
				tw.WriteStartElement ("Configuration");
				
				tw.WriteStartElement ("AddinStatus");
				foreach (AddinStatus e in addinStatus.Values) {
					tw.WriteStartElement ("Addin");
					tw.WriteAttributeString ("id", e.AddinId);
					tw.WriteAttributeString ("enabled", e.ConfigEnabled.ToString ());
					if (e.Uninstalled)
						tw.WriteAttributeString ("uninstalled", "True");
					if (e.Files.Length > 0) {
						foreach (var f in e.Files)
							tw.WriteElementString ("File", f);
					}
					tw.WriteEndElement ();
				}
				tw.WriteEndElement (); // AddinStatus
				tw.WriteEndElement (); // Configuration
			}
		}
	}
}
