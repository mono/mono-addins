using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mono.Addins
{
	/// <summary>
	/// Base class for custom condition attributes. Will be treated as a condition of the same short name, but without the "ConditionAttribute" suffix. For example, Foo.NameConditionAttribute will map to a condition with the ID "Name".
	/// </summary>
	/// <remarks>
	/// Properties and constructor arguments must be tagged with NodeAttributes to indicate the condition attributes to which they should be mapped.
	/// </remarks>
	public abstract class CustomConditionAttribute : Attribute
	{
	}
}
