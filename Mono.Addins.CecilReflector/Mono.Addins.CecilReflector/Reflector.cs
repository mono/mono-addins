// Reflector.cs
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
using System.Reflection;
using Mono.Addins;
using Mono.Addins.Database;
using Mono.Cecil;

namespace Mono.Addins.CecilReflector
{
	public class Reflector: IAssemblyReflector
	{
		IAssemblyLocator locator;
		Hashtable cachedAssemblies = new Hashtable ();
		
		public void Initialize (IAssemblyLocator locator)
		{
			this.locator = locator;
		}
		
		public object[] GetCustomAttributes (object obj, Type type, bool inherit)
		{
			Mono.Cecil.ICustomAttributeProvider aprov = obj as Mono.Cecil.ICustomAttributeProvider;
			if (aprov == null)
				return new object [0];
			
			ArrayList atts = new ArrayList ();
			foreach (CustomAttribute att in aprov.CustomAttributes) {
				object catt = ConvertAttribute (att, type);
				if (catt != null)
					atts.Add (catt);
			}
			if (inherit && (obj is TypeDefinition)) {
				TypeDefinition td = (TypeDefinition) obj;
				if (td.BaseType != null && td.BaseType.FullName != "System.Object") {
					TypeDefinition bt = FindTypeDefinition (td.Module.Assembly, td.BaseType);
					if (bt != null)
						atts.AddRange (GetCustomAttributes (bt, type, true));
				}
			}
			return atts.ToArray ();
		}
		
		object ConvertAttribute (CustomAttribute att, Type t)
		{
			string aname = att.Constructor.DeclaringType.FullName;
			if (aname != t.FullName)
				return null;
			
			object ob;
			
			if (att.ConstructorParameters.Count > 0) {
				object[] cargs = new object [att.ConstructorParameters.Count];
				att.ConstructorParameters.CopyTo (cargs, 0);
				ArrayList typeParameters = null;

				// Constructor parameters of type System.Type can't be set because types from the assembly
				// can't be loaded. The parameter value will be set later using a type name property.
				for (int n=0; n<cargs.Length; n++) {
					string atype = att.Constructor.Parameters[n].ParameterType.FullName;
					if (atype == "System.Type") {
						if (typeParameters == null)
							typeParameters = new ArrayList ();
						cargs [n] = typeof(object);
						typeParameters.Add (n);
					}
				}
				ob = Activator.CreateInstance (t, cargs);
				
				// If there are arguments of type System.Type, set them using the property
				if (typeParameters != null) {
					Type[] ptypes = new Type [cargs.Length];
					for (int n=0; n<cargs.Length; n++) {
						ptypes [n] = cargs [n].GetType ();
					}
					ConstructorInfo ci = t.GetConstructor (ptypes);
					ParameterInfo[] ciParams = ci.GetParameters ();
					
					for (int n=0; n<typeParameters.Count; n++) {
						int ip = (int) typeParameters [n];
						string propName = ciParams[ip].Name;
						propName = char.ToUpper (propName [0]) + propName.Substring (1) + "Name";
						PropertyInfo pi = t.GetProperty (propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						pi.SetValue (ob, (string) att.ConstructorParameters [ip], null);
						if (pi == null)
							throw new InvalidOperationException ("Property '" + propName + "' not found in type '" + t + "'.");
					}
				}
			} else {
				ob = Activator.CreateInstance (t);
			}
			
			foreach (DictionaryEntry de in att.Properties) {
				string pname = (string)de.Key;
				PropertyInfo prop = t.GetProperty (pname);
				if (prop != null) {
					if (prop.PropertyType == typeof(System.Type)) {
						// We can't load the type. We have to use the typeName property instead.
						pname += "Name";
						prop = t.GetProperty (pname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					}
					if (prop == null) {
						throw new InvalidOperationException ("Property '" + pname + "' not found in type '" + t + "'.");
					}
					prop.SetValue (ob, de.Value, null);
				}
			}
			return ob;
		}

		public object LoadAssembly (string file)
		{
			return LoadAssembly (file, false);
		}

		public AssemblyDefinition LoadAssembly (string file, bool cache)
		{
			AssemblyDefinition adef = (AssemblyDefinition) cachedAssemblies [file];
			if (adef != null)
				return adef;
			adef = AssemblyFactory.GetAssembly (file);
			if (adef != null && cache)
				cachedAssemblies [file] = adef;
			return adef;
		}

		public object LoadAssemblyFromReference (object asmReference)
		{
			AssemblyNameReference aref = (AssemblyNameReference) asmReference;
			string loc = locator.GetAssemblyLocation (aref.FullName);
			if (loc != null)
				return LoadAssembly (loc);
			else
				return null;
		}

		public System.Collections.IEnumerable GetAssemblyTypes (object asm)
		{
			TypeDefinitionCollection types = ((AssemblyDefinition)asm).MainModule.Types;
			foreach (IAnnotationProvider t in types)
				t.Annotations [typeof(AssemblyDefinition)] = asm;
			return types;
		}

		public System.Collections.IEnumerable GetAssemblyReferences (object asm)
		{
			return ((AssemblyDefinition)asm).MainModule.AssemblyReferences;
		}

		public object GetType (object asm, string typeName)
		{
			IAnnotationProvider t = ((AssemblyDefinition)asm).MainModule.Types [typeName];
			if (t != null) {
				t.Annotations [typeof(AssemblyDefinition)] = asm;
				return t;
			} else
				return null;
		}

		public object GetCustomAttribute (object obj, Type type, bool inherit)
		{
			foreach (object att in GetCustomAttributes (obj, type, inherit))
				if (type.IsInstanceOfType (att))
					return att;
			return null;
		}

		public string GetTypeName (object type)
		{
			return ((TypeDefinition)type).Name;
		}

		public string GetTypeFullName (object type)
		{
			return ((TypeDefinition)type).FullName;
		}

		public string GetTypeAssemblyQualifiedName (object type)
		{
			AssemblyDefinition asm = GetAssemblyDefinition ((TypeDefinition)type);
			return ((TypeDefinition)type).FullName + ", " + asm.Name.FullName;
		}
		
		AssemblyDefinition GetAssemblyDefinition (TypeDefinition t)
		{
			IAnnotationProvider aprov = (IAnnotationProvider) t;
			return (AssemblyDefinition) aprov.Annotations [typeof(AssemblyDefinition)];
		}

		public System.Collections.IEnumerable GetBaseTypeFullNameList (object type)
		{
			TypeDefinition t = (TypeDefinition) type;
			AssemblyDefinition asm = GetAssemblyDefinition (t);

			ArrayList list = new ArrayList ();
			Hashtable visited = new Hashtable ();
			GetBaseTypeFullNameList (visited, list, asm, t);
			list.Remove (t.FullName);
			return list;
		}

		void GetBaseTypeFullNameList (Hashtable visited, ArrayList list, AssemblyDefinition asm, TypeReference tr)
		{
			if (tr.FullName == "System.Object" || visited.Contains (tr.FullName))
				return;
			
			visited [tr.FullName] = tr;
			list.Add (tr.FullName);
			
			TypeDefinition type = FindTypeDefinition (asm, tr);
			if (type == null)
				return;

			asm = GetAssemblyDefinition (type);

			if (type.BaseType != null)
				GetBaseTypeFullNameList (visited, list, asm, type.BaseType);

			foreach (TypeReference interf in type.Interfaces)
				GetBaseTypeFullNameList (visited, list, asm, interf);
		}
		
		TypeDefinition FindTypeDefinition (AssemblyDefinition referencer, TypeReference rt)
		{
			if (rt is TypeDefinition)
				return (TypeDefinition) rt;

			string name = rt.FullName;
			TypeDefinition td = GetType (referencer, name) as TypeDefinition;
			if (td != null)
				return td;
			
			foreach (AssemblyNameReference aref in referencer.MainModule.AssemblyReferences) {
				string loc = locator.GetAssemblyLocation (aref.FullName);
				if (loc == null)
					continue;
				AssemblyDefinition asm = LoadAssembly (loc, true);
				td = GetType (asm, name) as TypeDefinition;
				if (td != null)
					return td;
			}
			return null;
		}

		public bool TypeIsAssignableFrom (object baseType, object type)
		{
			string baseName = ((TypeDefinition)baseType).FullName;
			foreach (string bt in GetBaseTypeFullNameList (type))
				if (bt == baseName)
					return true;
			return false;
		}

		public IEnumerable GetFields (object type)
		{
			return ((TypeDefinition)type).Fields;
		}

		public string GetFieldName (object field)
		{
			return ((FieldDefinition)field).Name;
		}

		public string GetFieldTypeFullName (object field)
		{
			return ((FieldDefinition)field).FieldType.FullName;
		}

	}
}
