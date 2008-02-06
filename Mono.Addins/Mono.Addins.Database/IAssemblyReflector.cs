// IAssemblyReflector.cs
//
// Author:
//   Lluis Sanchez Gual <lluis@novell.com>
//
// Copyright (c) 2007 Novell, Inc (http://www.novell.com)
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
//
//

using System;
using System.Collections;

namespace Mono.Addins.Database
{
	public interface IAssemblyReflector
	{
		void Initialize (IAssemblyLocator locator);
		
		object[] GetCustomAttributes (object obj, Type type, bool inherit);
		
		object LoadAssembly (string file);
		object LoadAssemblyFromReference (object asmReference);
		IEnumerable GetAssemblyTypes (object asm);
		IEnumerable GetAssemblyReferences (object asm);
		
		object GetType (object asm, string typeName);
		object GetCustomAttribute (object obj, Type type, bool inherit);
		string GetTypeName (object type);
		string GetTypeFullName (object type);
		string GetTypeAssemblyQualifiedName (object type);
		IEnumerable GetBaseTypeFullNameList (object type);
		bool TypeIsAssignableFrom (object baseType, object type);
		
		IEnumerable GetFields (object type);
		string GetFieldName (object field);
		string GetFieldTypeFullName (object field);
	}
	
	public interface IAssemblyLocator
	{
		string GetAssemblyLocation (string fullName);
	}
}
