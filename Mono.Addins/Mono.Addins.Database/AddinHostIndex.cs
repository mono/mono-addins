
using System;
using System.Collections;
using Mono.Addins.Serialization;

namespace Mono.Addins.Database
{
	class AddinHostIndex: IBinaryXmlElement
	{
		static BinaryXmlTypeMap typeMap = new BinaryXmlTypeMap (typeof(AddinHostIndex));
		
		Hashtable index = new Hashtable ();
		
		public void RegisterAssembly (string assemblyLocation, string addinId, string addinLocation)
		{
			index [Util.GetFullPath (assemblyLocation)] = addinId + " " + addinLocation;
		}
		
		public bool GetAddinForAssembly (string assemblyLocation, out string addinId, out string addinLocation)
		{
			string s = index [Util.GetFullPath (assemblyLocation)] as string;
			if (s == null) {
				addinId = null;
				addinLocation = null;
				return false;
			}
			else {
				int i = s.IndexOf (' ');
				addinId = s.Substring (0, i);
				addinLocation = s.Substring (i+1);
				return true;
			}
		}
		
		public void RemoveHostData (string addinId, string addinLocation)
		{
			string loc = addinId + " " + Util.GetFullPath (addinLocation);
			ArrayList todelete = new ArrayList ();
			foreach (DictionaryEntry e in index) {
				if (((string)e.Value) == loc)
					todelete.Add (e.Key);
			}
			foreach (string s in todelete)
				index.Remove (s);
		}
		
		public static AddinHostIndex Read (FileDatabase fileDatabase, string file)
		{
			return (AddinHostIndex) fileDatabase.ReadObject (file, typeMap);
		}
		
		public void Write (FileDatabase fileDatabase, string file)
		{
			fileDatabase.WriteObject (file, this, typeMap);
		}
		
		void IBinaryXmlElement.Write (BinaryXmlWriter writer)
		{
			writer.WriteValue ("index", index);
		}
		
		void IBinaryXmlElement.Read (BinaryXmlReader reader)
		{
			reader.ReadValue ("index", index);
		}
	}
}
