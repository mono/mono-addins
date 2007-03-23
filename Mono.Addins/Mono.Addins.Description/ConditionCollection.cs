
using System;
using System.Collections;

namespace Mono.Addins.Description
{
	public class ConditionCollection: ObjectDescriptionCollection
	{
		public Condition this [int n] {
			get { return (Condition) List [n]; }
		}
	}
}
