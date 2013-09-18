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
using CustomAttribute = Mono.Cecil.CustomAttribute;
using MA = Mono.Addins.Database;
using System.Collections.Generic;

namespace Mono.Addins.CecilReflector
{
	public class Reflector: IAssemblyReflector
	{
		IAssemblyLocator locator;
		Hashtable cachedAssemblies = new Hashtable ();
		DefaultAssemblyResolver defaultAssemblyResolver;
		
		public void Initialize (IAssemblyLocator locator)
		{
			this.locator = locator;
			defaultAssemblyResolver = new DefaultAssemblyResolver ();
			defaultAssemblyResolver.ResolveFailure += delegate (object sender, AssemblyNameReference reference) {
				var file = locator.GetAssemblyLocation (reference.FullName);
				if (file != null)
					return LoadAssembly (file, true);
				else
					return null;
			};
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
		
		object ConvertAttribute (CustomAttribute att, Type expectedType)
		{
			Type attype = typeof(IAssemblyReflector).Assembly.GetType (att.Constructor.DeclaringType.FullName);

			if (attype == null || !expectedType.IsAssignableFrom (attype))
				return null;
			
			object ob;
			
			if (att.ConstructorArguments.Count > 0) {
				object[] cargs = new object [att.ConstructorArguments.Count];
				ArrayList typeParameters = null;

				// Constructor parameters of type System.Type can't be set because types from the assembly
				// can't be loaded. The parameter value will be set later using a type name property.
				for (int n=0; n<cargs.Length; n++) {
					var atype = Type.GetType (att.Constructor.Parameters[n].ParameterType.FullName);
					if (atype == null)
						atype = typeof(IAssemblyReflector).Assembly.GetType (att.Constructor.Parameters[n].ParameterType.FullName);

					if (atype.IsEnum)
						cargs [n] = Enum.ToObject (atype, att.ConstructorArguments [n].Value);
					else
						cargs [n] = att.ConstructorArguments [n].Value;

					if (typeof(System.Type).IsAssignableFrom (atype)) {
						if (typeParameters == null)
							typeParameters = new ArrayList ();
						cargs [n] = typeof(object);
						typeParameters.Add (n);
					}
				}
				ob = Activator.CreateInstance (attype, cargs);
				
				// If there are arguments of type System.Type, set them using the property
				if (typeParameters != null) {
					Type[] ptypes = new Type [cargs.Length];
					for (int n=0; n<cargs.Length; n++) {
						ptypes [n] = cargs [n].GetType ();
					}
					ConstructorInfo ci = attype.GetConstructor (ptypes);
					ParameterInfo[] ciParams = ci.GetParameters ();
					
					for (int n=0; n<typeParameters.Count; n++) {
						int ip = (int) typeParameters [n];
						string propName = ciParams[ip].Name;
						propName = char.ToUpper (propName [0]) + propName.Substring (1) + "Name";
						PropertyInfo pi = attype.GetProperty (propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

						if (pi == null)
							throw new InvalidOperationException ("Property '" + propName + "' not found in type '" + attype + "'.");

						pi.SetValue (ob, ((TypeReference) att.ConstructorArguments [ip].Value).FullName, null);
					}
				}
			} else {
				ob = Activator.CreateInstance (attype);
			}
			
			foreach (Mono.Cecil.CustomAttributeNamedArgument namedArgument in att.Properties) {
				string pname = namedArgument.Name;
				PropertyInfo prop = attype.GetProperty (pname);
				if (prop != null) {
					if (prop.PropertyType == typeof(System.Type)) {
						// We can't load the type. We have to use the typeName property instead.
						pname += "Name";
						prop = attype.GetProperty (pname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						
						if (prop == null)
							throw new InvalidOperationException ("Property '" + pname + "' not found in type '" + attype + "'.");

						prop.SetValue (ob, ((TypeReference) namedArgument.Argument.Value).FullName, null);
					} else
						prop.SetValue (ob, namedArgument.Argument.Value, null);
				}
			}
			return ob;
		}
		
		public List<MA.CustomAttribute> GetRawCustomAttributes (object obj, Type type, bool inherit)
		{
			List<MA.CustomAttribute> atts = new List<MA.CustomAttribute> ();
			Mono.Cecil.ICustomAttributeProvider aprov = obj as Mono.Cecil.ICustomAttributeProvider;
			if (aprov == null)
				return atts;
			
			foreach (CustomAttribute att in aprov.CustomAttributes) {
				MA.CustomAttribute catt = ConvertToRawAttribute (att, type.FullName);
				if (catt != null)
					atts.Add (catt);
			}
			if (inherit && (obj is TypeDefinition)) {
				TypeDefinition td = (TypeDefinition) obj;
				if (td.BaseType != null && td.BaseType.FullName != "System.Object") {
					TypeDefinition bt = FindTypeDefinition (td.Module.Assembly, td.BaseType);
					if (bt != null)
						atts.AddRange (GetRawCustomAttributes (bt, type, true));
				}
			}
			return atts;
		}
		
		MA.CustomAttribute ConvertToRawAttribute (CustomAttribute att, string expectedType)
		{
			TypeDefinition attType = FindTypeDefinition (att.Constructor.DeclaringType.Module.Assembly, att.Constructor.DeclaringType);
			
			if (attType == null || !TypeIsAssignableFrom (expectedType, attType))
				return null;

			MA.CustomAttribute mat = new MA.CustomAttribute ();
			mat.TypeName = att.Constructor.DeclaringType.FullName;
			
			if (att.ConstructorArguments.Count > 0) {
				var arguments = att.ConstructorArguments;
				
				MethodReference constructor = FindConstructor (att);
				if (constructor == null)
					throw new InvalidOperationException ("Custom attribute constructor not found");

				for (int n=0; n<arguments.Count; n++) {
					ParameterDefinition par = constructor.Parameters[n];
					object val = arguments [n].Value;
					if (val != null) {
						string name = par.Name;
						NodeAttributeAttribute bat = (NodeAttributeAttribute) GetCustomAttribute (par, typeof(NodeAttributeAttribute), false);
						if (bat != null)
							name = bat.Name;
						mat.Add (name, Convert.ToString (val, System.Globalization.CultureInfo.InvariantCulture));
					}
				}
			}
			
			foreach (Mono.Cecil.CustomAttributeNamedArgument namedArgument in att.Properties) {
				string pname = namedArgument.Name;
				object val = namedArgument.Argument.Value;
				if (val == null)
					continue;

				foreach (TypeDefinition td in GetInheritanceChain (attType)) {
					PropertyDefinition prop = GetMember (td.Properties, pname);
					if (prop == null)
						continue;

					NodeAttributeAttribute bat = (NodeAttributeAttribute) GetCustomAttribute (prop, typeof(NodeAttributeAttribute), false);
					if (bat != null) {
						string name = string.IsNullOrEmpty (bat.Name) ? prop.Name : bat.Name;
						mat.Add (name, Convert.ToString (val, System.Globalization.CultureInfo.InvariantCulture));
					}
				}
			}
			
			foreach (Mono.Cecil.CustomAttributeNamedArgument namedArgument in att.Fields) {
				string pname = namedArgument.Name;
				object val = namedArgument.Argument.Value;
				if (val == null)
					continue;

				foreach (TypeDefinition td in GetInheritanceChain (attType)) {
					FieldDefinition field = GetMember (td.Fields, pname);
					if (field != null) {
						NodeAttributeAttribute bat = (NodeAttributeAttribute) GetCustomAttribute (field, typeof(NodeAttributeAttribute), false);
						if (bat != null) {
							string name = string.IsNullOrEmpty (bat.Name) ? field.Name : bat.Name;
							mat.Add (name, Convert.ToString (val, System.Globalization.CultureInfo.InvariantCulture));
						}
					}
				}
			}

			return mat;
		}

		static TMember GetMember<TMember> (ICollection<TMember> members, string name) where TMember : class, IMemberDefinition
		{
			foreach (var member in members)
				if (member.Name == name)
					return member;

			return null;
		}
		
		IEnumerable<TypeDefinition> GetInheritanceChain (TypeDefinition td)
		{
			yield return td;
			while (td != null && td.BaseType != null && td.BaseType.FullName != "System.Object") {
				td = FindTypeDefinition (td.Module.Assembly, td.BaseType);
				if (td != null)
					yield return td;
			}
		}

		MethodReference FindConstructor (CustomAttribute att)
		{
			// The constructor provided by CustomAttribute.Constructor is lacking some information, such as the parameter
			// name and custom attributes. Since we need the full info, we have to look it up in the declaring type.
			
			TypeDefinition atd = FindTypeDefinition (att.Constructor.DeclaringType.Module.Assembly, att.Constructor.DeclaringType);
			foreach (MethodReference met in atd.Methods) {
				if (met.Name != ".ctor")
					continue;

				if (met.Parameters.Count == att.Constructor.Parameters.Count) {
					for (int n = met.Parameters.Count - 1; n >= 0; n--) {
						if (met.Parameters[n].ParameterType.FullName != att.Constructor.Parameters[n].ParameterType.FullName)
							break;
						if (n == 0)
							return met;
					}
				}
			}
			return null;
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
			var rp = new ReaderParameters (ReadingMode.Deferred);
			rp.AssemblyResolver = defaultAssemblyResolver;
			adef = AssemblyDefinition.ReadAssembly (file, rp);
			if (adef != null && cache)
				cachedAssemblies [file] = adef;
			return adef;
		}
		
		public string[] GetResourceNames (object asm)
		{
			AssemblyDefinition adef = (AssemblyDefinition) asm;
			List<string> names = new List<string> (adef.MainModule.Resources.Count);
			foreach (Resource res in adef.MainModule.Resources) {
				if (res is EmbeddedResource)
					names.Add (res.Name);
			}
			return names.ToArray ();
		}
		
		public System.IO.Stream GetResourceStream (object asm, string resourceName)
		{
			AssemblyDefinition adef = (AssemblyDefinition) asm;
			foreach (Resource res in adef.MainModule.Resources) {
				EmbeddedResource r = res as EmbeddedResource;
				if (r != null && r.Name == resourceName)
					return r.GetResourceStream ();
			}
			throw new InvalidOperationException ("Resource not found: " + resourceName);
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
			return ((AssemblyDefinition)asm).MainModule.Types;
		}

		public System.Collections.IEnumerable GetAssemblyReferences (object asm)
		{
			return ((AssemblyDefinition)asm).MainModule.AssemblyReferences;
		}

		public object GetType (object asm, string typeName)
		{
			if (typeName.IndexOf ('`') != -1) {
				foreach (TypeDefinition td in ((AssemblyDefinition)asm).MainModule.Types) {
					if (td.FullName == typeName) {
						return td;
					}
				}
			}
			TypeDefinition t = ((AssemblyDefinition)asm).MainModule.GetType (typeName);
			if (t != null) {
				return t;
			} else {
				return null;
			}
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
			return t.Module.Assembly;
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
			int i = name.IndexOf ('<');
			if (i != -1) {
				name = name.Substring (0, i);
				td = GetType (referencer, name) as TypeDefinition;
				if (td != null)
					return td;
			}
			
			foreach (AssemblyNameReference aref in referencer.MainModule.AssemblyReferences) {
				string loc = locator.GetAssemblyLocation (aref.FullName);
				if (loc == null)
					continue;
				try {
					AssemblyDefinition asm = LoadAssembly (loc, true);
					td = GetType (asm, name) as TypeDefinition;
					if (td != null)
						return td;
				} catch {
					Console.WriteLine ("Could not scan dependency '{0}'. Ignoring for now.", aref.FullName);
				}
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

		public bool TypeIsAssignableFrom (string baseTypeName, object type)
		{
			foreach (string bt in GetBaseTypeFullNameList (type))
				if (bt == baseTypeName)
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
