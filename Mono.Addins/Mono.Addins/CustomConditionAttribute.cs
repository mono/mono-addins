// 
// CustomConditionAttribute.cs
// 
// Copyright (c) Microsoft Corp.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

namespace Mono.Addins
{
	/// <summary>
	/// Base class for custom condition attributes.
	/// </summary>
	/// <remarks>
	/// Custom condition attributes can be used to apply conditions to extensions.
	/// All custom condition attributes must subclass CustomConditionAttribute.
	/// All arguments and properties must be tagged with NodeAttribute.
	/// The ID of the condition is the simple name of this class without the "Attribute"
	/// or "ConditionAttribute" suffix. For example "FooConditionAttribute" maps to the
	/// condition ID "Foo" and "BarAttribute" maps to "Bar".
	/// </remarks>
	[AttributeUsage (AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public abstract class CustomConditionAttribute : Attribute
	{
	}
}
