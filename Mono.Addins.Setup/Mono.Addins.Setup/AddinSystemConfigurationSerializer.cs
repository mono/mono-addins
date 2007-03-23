using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;
using System.Collections;
using System.Globalization;

namespace Mono.Addins.Setup
{
	internal class AddinSystemConfigurationSerializer : XmlSerializer 
	{
		protected override void Serialize (object o, XmlSerializationWriter writer)
		{
			AddinSystemConfigurationWriter xsWriter = writer as AddinSystemConfigurationWriter;
			xsWriter.WriteRoot_AddinSystemConfiguration (o);
		}
		
		protected override object Deserialize (XmlSerializationReader reader)
		{
			AddinSystemConfigurationReader xsReader = reader as AddinSystemConfigurationReader;
			return xsReader.ReadRoot_AddinSystemConfiguration ();
		}
		
		protected override XmlSerializationWriter CreateWriter ()
		{
			return new AddinSystemConfigurationWriter ();
		}
		
		protected override XmlSerializationReader CreateReader ()
		{
			return new AddinSystemConfigurationReader ();
		}
	}		
}

