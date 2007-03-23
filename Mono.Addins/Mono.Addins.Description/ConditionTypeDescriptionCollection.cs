
using System;

namespace Mono.Addins.Description
{
	public class ConditionTypeDescriptionCollection: ObjectDescriptionCollection
	{
		public ConditionTypeDescription this [int n] {
			get { return (ConditionTypeDescription) List [n]; }
		}
	}
}
