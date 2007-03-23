using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;
using System.Collections;
using System.Globalization;

namespace Mono.Addins.Setup
{
	internal class RepositorySerializer : XmlSerializer 
	{
		protected override void Serialize (object o, XmlSerializationWriter writer)
		{
			RepositoryWriter xsWriter = writer as RepositoryWriter;
			xsWriter.WriteRoot_Repository (o);
		}
		
		protected override object Deserialize (XmlSerializationReader reader)
		{
			RepositoryReader xsReader = reader as RepositoryReader;
			return xsReader.ReadRoot_Repository ();
		}
		
		protected override XmlSerializationWriter CreateWriter ()
		{
			return new RepositoryWriter ();
		}
		
		protected override XmlSerializationReader CreateReader ()
		{
			return new RepositoryReader ();
		}
	}		
}

