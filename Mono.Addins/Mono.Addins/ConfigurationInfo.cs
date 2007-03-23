
using System;
using System.IO;
using System.Collections.Specialized;

namespace Mono.Addins
{
	[Serializable]
	public class ConfigurationInfo
	{
		string configDir;
		string startupDirectory;
		internal bool Locked;
		
		public ConfigurationInfo (string configDirectory)
		{
			configDir = configDirectory;
		}
		
		public string ConfigDirectory {
			get { return configDir != null ? configDir : string.Empty; }
			internal set { configDir = value; }
		}
		
		public string UserAddinPath {
			get { return Path.Combine (ConfigDirectory, "addins"); }
		}
		
		internal string StartupDirectory {
			get { return startupDirectory != null ? startupDirectory : string.Empty; }
			set { startupDirectory = value; }
		}
	}
}
