
using System;

namespace Mono.Addins.Serialization
{
	internal interface IBinaryXmlElement
	{
		void Read (BinaryXmlReader reader);
		void Write (BinaryXmlWriter writer);
	}
}
