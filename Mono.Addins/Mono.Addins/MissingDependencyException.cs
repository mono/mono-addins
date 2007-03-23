
using System;
using System.Runtime.Serialization;

namespace Mono.Addins
{
	[Serializable]
	internal class MissingDependencyException: Exception
	{
		public MissingDependencyException (SerializationInfo inf, StreamingContext ctx) : base (inf, ctx)
		{
		}
		
		public MissingDependencyException (string message): base (message)
		{
		}
	}
}
