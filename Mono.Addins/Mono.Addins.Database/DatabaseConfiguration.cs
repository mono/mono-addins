
using System;
using System.IO;
using System.Collections.Specialized;
using System.Xml;

namespace Mono.Addins.Database
{
	internal class DatabaseConfiguration
	{
		public StringCollection DisabledAddins = new StringCollection ();
		
		public static DatabaseConfiguration Read (string file)
		{
			DatabaseConfiguration config = new DatabaseConfiguration ();
			
			StreamReader s = new StreamReader (file);
			using (s) {
				XmlTextReader tr = new XmlTextReader (s);
				tr.MoveToContent ();
				tr.ReadStartElement ("Configuration");
				tr.MoveToContent ();
				tr.ReadStartElement ("DisabledAddins");
				tr.MoveToContent ();
				if (!tr.IsEmptyElement) {
					while (tr.NodeType != XmlNodeType.EndElement) {
						if (tr.NodeType == XmlNodeType.Element) {
							if (tr.LocalName == "Addin")
								config.DisabledAddins.Add (tr.ReadElementString ());
						}
						else
							tr.Skip ();
						tr.MoveToContent ();
					}
				}
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
				tw.WriteStartElement ("DisabledAddins");
				foreach (string ad in DisabledAddins)
					tw.WriteElementString ("Addin", ad);
				tw.WriteEndElement ();
				tw.WriteEndElement ();
			}
		}
	}
}
