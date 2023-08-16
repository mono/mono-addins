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

//#define ASSEMBLY_LOAD_STATS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Mono.Addins;
using Mono.Addins.Database;
using Mono.Cecil;
using CustomAttribute = Mono.Cecil.CustomAttribute;
using MA = Mono.Addins.Database;
using System.Linq;

namespace Mono.Addins.CecilReflector
{
	public class Reflector: IAssemblyReflector, IDisposable
	{
		IAssemblyLocator locator;
		Dictionary<string, AssemblyDefinition> cachedAssemblies = new Dictionary<string, AssemblyDefinition> ();
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
					// The base type may be in an assembly that doesn't reference Mono.Addins, even though it may reference
					// other assemblies that do reference Mono.Addins. So the Mono.Addins filter can't be applied here.
					TypeDefinition bt = FindTypeDefinition (td.Module.Assembly, td.BaseType, assembliesReferencingMonoAddinsOnly: false);
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
				List<int> typeParameters = null;

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
							typeParameters = new List<int> ();
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
						int ip = typeParameters [n];
						string propName = ciParams[ip].Name;
						propName = char.ToUpper (propName [0]) + propName.Substring (1);

						SetTypeNameAndAssemblyName (propName, attype, ob, (TypeReference)att.ConstructorArguments[ip].Value);
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
						SetTypeNameAndAssemblyName (pname, attype, ob, (TypeReference)namedArgument.Argument.Value);
					} else
						prop.SetValue (ob, namedArgument.Argument.Value, null);
				}
			}
			return ob;
		}

		static void SetTypeNameAndAssemblyName (string basePropName, Type attype, object ob, TypeReference typeReference)
		{
			// We can't load the type. We have to use the typeName and typeAssemblyName properties instead.
			var typeNameProp = basePropName + "Name";
			var prop = attype.GetProperty (typeNameProp, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						
			if (prop == null)
				throw new InvalidOperationException ("Property '" + typeNameProp + "' not found in type '" + attype + "'.");

			var assemblyName = typeReference.Resolve().Module.Assembly.FullName;
			prop.SetValue (ob, typeReference.FullName + ", " + assemblyName, null);

			prop = attype.GetProperty(basePropName + "FullName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			if (prop != null)
				prop.SetValue(ob, typeReference.FullName, null);
		}
		
		public List<MA.CustomAttribute> GetRawCustomAttributes (object obj, Type type, bool inherit)
		{
			List<MA.CustomAttribute> atts = new List<MA.CustomAttribute> ();
			Mono.Cecil.ICustomAttributeProvider aprov = obj as Mono.Cecil.ICustomAttributeProvider;
			if (aprov == null)
				return atts;
			
			foreach (CustomAttribute att in aprov.CustomAttributes) {
				// The class of the attribute is always a subclass of a Mono.Addins class
				MA.CustomAttribute catt = ConvertToRawAttribute (att, type.FullName, baseIsMonoAddinsType: true);
				if (catt != null)
					atts.Add (catt);
			}
			if (inherit && (obj is TypeDefinition)) {
				TypeDefinition td = (TypeDefinition) obj;
				if (td.BaseType != null && td.BaseType.FullName != "System.Object") {
					// The base type may be in an assembly that doesn't reference Mono.Addins, even though it may reference
					// other assemblies that do reference Mono.Addins. So the Mono.Addins filter can't be applied here.
					TypeDefinition bt = FindTypeDefinition (td.Module.Assembly, td.BaseType, assembliesReferencingMonoAddinsOnly: false);
					if (bt != null)
						atts.AddRange (GetRawCustomAttributes (bt, type, true));
				}
			}
			return atts;
		}

		MA.CustomAttribute ConvertToRawAttribute (CustomAttribute att, string expectedType, bool baseIsMonoAddinsType)
		{
			// If the class of the attribute is a subclass of a Mono.Addins class, then the assembly where this
			// custom attribute type is defined must reference Mono.Addins.
			TypeDefinition attType = FindTypeDefinition (att.Constructor.DeclaringType.Module.Assembly, att.Constructor.DeclaringType, assembliesReferencingMonoAddinsOnly: baseIsMonoAddinsType);

			if (attType == null || !TypeIsAssignableFrom (expectedType, attType, baseIsMonoAddinsType))
				return null;

			MA.CustomAttribute mat = new MA.CustomAttribute ();
			mat.TypeName = att.Constructor.DeclaringType.FullName;
			
			if (att.ConstructorArguments.Count > 0) {
				var arguments = att.ConstructorArguments;
				
				MethodReference constructor = FindConstructor (att, attType);
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

			List<TypeDefinition> inheritanceChain = null;

			foreach (Mono.Cecil.CustomAttributeNamedArgument namedArgument in att.Properties) {
				string pname = namedArgument.Name;
				object val = namedArgument.Argument.Value;
				if (val == null)
					continue;

				if (inheritanceChain == null)
					inheritanceChain = GetInheritanceChain (attType, baseIsMonoAddinsType).ToList ();

				foreach (TypeDefinition td in inheritanceChain) {
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

				if (inheritanceChain == null)
					inheritanceChain = GetInheritanceChain (attType, baseIsMonoAddinsType).ToList ();

				foreach (TypeDefinition td in inheritanceChain) {
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

		IEnumerable<TypeDefinition> GetInheritanceChain (TypeDefinition td, bool baseIsMonoAddinsType)
		{
			yield return td;
			while (td != null && td.BaseType != null && td.BaseType.FullName != "System.Object") {
				// If the class we are looking for is a subclass of a Mono.Addins class, then the assembly where this
				// class is defined must reference Mono.Addins.
				td = FindTypeDefinition (td.Module.Assembly, td.BaseType, assembliesReferencingMonoAddinsOnly: baseIsMonoAddinsType);
				if (td != null)
					yield return td;
			}
		}

		MethodReference FindConstructor (CustomAttribute att, TypeDefinition atd)
		{
			// The constructor provided by CustomAttribute.Constructor is lacking some information, such as the parameter
			// name and custom attributes. Since we need the full info, we have to look it up in the declaring type.

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
			return LoadAssembly (file, true);
		}

		public AssemblyDefinition LoadAssembly (string file, bool cache)
		{
			AssemblyDefinition adef;
			if (cachedAssemblies.TryGetValue (file, out adef))
				return adef;
			var rp = new ReaderParameters (ReadingMode.Deferred);
			rp.AssemblyResolver = defaultAssemblyResolver;
			adef = AssemblyDefinition.ReadAssembly (file, rp);
			if (adef != null) {
				if (cache)
					cachedAssemblies [file] = adef;
				// Since the assembly is loaded, we can quickly check now if it references Mono.Addins.
				// This information may be useful later on.
				if (adef.Name.Name != "Mono.Addins" && !adef.MainModule.AssemblyReferences.Any (r => r.Name == "Mono.Addins"))
					assembliesNotReferencingMonoAddins.Add (adef.FullName);
			}

#if ASSEMBLY_LOAD_STATS
			loadCounter.TryGetValue (file, out int num);
			loadCounter [file] = num + 1;
#endif
			return adef;
		}

#if ASSEMBLY_LOAD_STATS
		static Dictionary<string, int> loadCounter = new Dictionary<string, int> ();
#endif

		public void UnloadAssembly (object assembly)
		{
			var adef = (AssemblyDefinition)assembly;
			cachedAssemblies.Remove (adef.MainModule.FileName);
			adef.Dispose ();
		}

		public string GetAssemblyName (object assembly)
		{
			var adef = (AssemblyDefinition)assembly;
			return adef.Name.Name;
		}

		bool FoundToNotReferenceMonoAddins (AssemblyNameReference aref)
		{
			// Quick check to find out if an assembly references Mono.Addins, based only on cached information.
			return assembliesNotReferencingMonoAddins.Contains (aref.FullName);
		}

		bool CheckHasMonoAddinsReference (AssemblyDefinition adef)
		{
			// Maybe the assembly is already in the disallowed list
			if (assembliesNotReferencingMonoAddins.Contains (adef.FullName))
				return false;

			if (adef.Name.Name != "Mono.Addins" && !adef.MainModule.AssemblyReferences.Any (r => r.Name == "Mono.Addins")) {
				assembliesNotReferencingMonoAddins.Add (adef.FullName);
				return false;
			}
			return true;
		}

		HashSet<string> assembliesNotReferencingMonoAddins = new HashSet<string> (StringComparer.Ordinal);

		public string [] GetResourceNames (object asm)
		{
			AssemblyDefinition adef = (AssemblyDefinition)asm;
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
			// The scanner only uses this method when looking for an extension node type, which is
			// a subclass of a Mono.Addins type, so it must be defined in an assembly that references Mono.Addins.
			return LoadAssemblyFromReference ((AssemblyNameReference)asmReference, assembliesReferencingMonoAddinsOnly: true);
		}

		AssemblyDefinition LoadAssemblyFromReference (AssemblyNameReference aref, bool assembliesReferencingMonoAddinsOnly)
		{
			// Fast check for Mono.Addins reference that sometimes will avoid loading the assembly 
			if (assembliesReferencingMonoAddinsOnly && FoundToNotReferenceMonoAddins (aref))
				return null;

			string loc = locator.GetAssemblyLocation (aref.FullName);
			if (loc == null)
				return null;

			AssemblyDefinition asm = LoadAssembly (loc, true);

			// Check for Mono.Addins references first, that will update the cache.

			if (!CheckHasMonoAddinsReference (asm) && assembliesReferencingMonoAddinsOnly) {
				// We loaded an assembly we are not interested in, so we could unload it now.
				// We already cached the information about whether it has a Mono.Addins reference
				// or not, so the next we try to load the reference, the check can be done
				// without loading. However, empirical tests should that the cache size doesn't
				// increase much, while redundant assembly loads are significanly reduced.
				//UnloadAssembly (asm);
				return null;
			}
			return asm;
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
			// The base type can be any type, so we can apply the Mono.Addins optimization in this case.
			return GetBaseTypeFullNameList ((TypeDefinition)type, baseIsMonoAddinsType: false, includeInterfaces: true);
		}

		public System.Collections.IEnumerable GetBaseTypeFullNameList (TypeDefinition type, bool baseIsMonoAddinsType, bool includeInterfaces)
		{
			AssemblyDefinition asm = GetAssemblyDefinition (type);

			var list = new List<string> ();
			Hashtable visited = new Hashtable ();
			GetBaseTypeFullNameList (visited, list, asm, type, baseIsMonoAddinsType, includeInterfaces);
			list.Remove (type.FullName);
			return list;
		}

		void GetBaseTypeFullNameList (Hashtable visited, List<string> list, AssemblyDefinition asm, TypeReference tr, bool baseIsMonoAddinsType, bool includeInterfaces)
		{
			if (tr.FullName == "System.Object" || visited.Contains (tr.FullName))
				return;

			visited [tr.FullName] = tr;
			list.Add (tr.FullName);

			TypeDefinition type = FindTypeDefinition (asm, tr, assembliesReferencingMonoAddinsOnly: baseIsMonoAddinsType);
			if (type == null)
				return;

			asm = GetAssemblyDefinition (type);

			if (type.BaseType != null)
				GetBaseTypeFullNameList (visited, list, asm, type.BaseType, baseIsMonoAddinsType, includeInterfaces);

			if (includeInterfaces) {
				foreach (InterfaceImplementation ii in type.Interfaces) {
					TypeReference interf = ii.InterfaceType;
					GetBaseTypeFullNameList (visited, list, asm, interf, baseIsMonoAddinsType, includeInterfaces);
				}
			}
		}

		TypeDefinition FindTypeDefinition (AssemblyDefinition referencer, TypeReference rt, bool assembliesReferencingMonoAddinsOnly)
		{
			if (rt is TypeDefinition)
				return (TypeDefinition)rt;

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
				try {
					AssemblyDefinition asm = LoadAssemblyFromReference (aref, assembliesReferencingMonoAddinsOnly);
					if (asm == null)
						continue;

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

		public bool TypeIsAssignableFrom (string baseTypeName, object type, bool baseIsMonoAddinsClass)
		{
			// If the base is a Mono.Addins class then there is no need to include interfaces when getting
			// the base type list (since we are looking for a class, not for an interface).
			foreach (string bt in GetBaseTypeFullNameList ((TypeDefinition)type, baseIsMonoAddinsClass, !baseIsMonoAddinsClass))
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
			return GetTypeAssemblyQualifiedName(((FieldDefinition)field).FieldType.Resolve());
		}

		public void Dispose ()
		{
			foreach (AssemblyDefinition asm in cachedAssemblies.Values)
				asm.Dispose ();

#if ASSEMBLY_LOAD_STATS
			Console.WriteLine ("Total assemblies: " + loadCounter.Count);
			Console.WriteLine ("Assembly cache size: {0} ({1}%)", cachedAssemblies.Count, (cachedAssemblies.Count * 100) / loadCounter.Count);

			Console.WriteLine ("Total assembly loads: " + loadCounter.Values.Sum ());
			var redundant = loadCounter.Where (c => c.Value > 1).Select (c => c.Value - 1).Sum ();
			Console.WriteLine ("Redundant loads: {0} ({1}%)", redundant, ((redundant * 100) / loadCounter.Count));
#endif
		}
	}
}
