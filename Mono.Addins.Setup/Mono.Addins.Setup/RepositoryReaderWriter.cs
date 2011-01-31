// It is automatically generated
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;
using System.Collections;
using System.Globalization;

namespace Mono.Addins.Setup
{
	internal class RepositoryReader : XmlSerializationReader
	{
		static readonly System.Reflection.MethodInfo fromBinHexStringMethod = typeof (XmlConvert).GetMethod ("FromBinHexString", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, new Type [] {typeof (string)}, null);
		static byte [] FromBinHexString (string input)
		{
			return input == null ? null : (byte []) fromBinHexStringMethod.Invoke (null, new object [] {input});
		}
		public object ReadRoot_Repository ()
		{
			Reader.MoveToContent();
			if (Reader.LocalName != "Repository" || Reader.NamespaceURI != "")
				throw CreateUnknownNodeException();
			return ReadObject_Repository (true, true);
		}

		public Mono.Addins.Setup.Repository ReadObject_Repository (bool isNullable, bool checkType)
		{
			Mono.Addins.Setup.Repository ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Repository" || t.Namespace != "")
					throw CreateUnknownTypeException(t);
			}

			ob = (Mono.Addins.Setup.Repository) Activator.CreateInstance(typeof(Mono.Addins.Setup.Repository), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b0=false, b1=false, b2=false, b3=false;

			Mono.Addins.Setup.RepositoryEntryCollection o5;
			o5 = ob.@Repositories;
			Mono.Addins.Setup.RepositoryEntryCollection o7;
			o7 = ob.@Addins;
			int n4=0, n6=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "Addin" && Reader.NamespaceURI == "" && !b3) {
						if (((object)o7) == null)
							throw CreateReadOnlyCollectionException ("Mono.Addins.Setup.RepositoryEntryCollection");
						o7.Add (ReadObject_PackageRepositoryEntry (false, true));
						n6++;
					}
					else if (Reader.LocalName == "Repository" && Reader.NamespaceURI == "" && !b2) {
						if (((object)o5) == null)
							throw CreateReadOnlyCollectionException ("Mono.Addins.Setup.RepositoryEntryCollection");
						o5.Add (ReadObject_ReferenceRepositoryEntry (false, true));
						n4++;
					}
					else if (Reader.LocalName == "Name" && Reader.NamespaceURI == "" && !b0) {
						b0 = true;
						string s8 = Reader.ReadElementString ();
						ob.@Name = s8;
					}
					else if (Reader.LocalName == "Url" && Reader.NamespaceURI == "" && !b1) {
						b1 = true;
						string s9 = Reader.ReadElementString ();
						ob.@Url = s9;
					}
					else {
						UnknownNode (ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}


			ReadEndElement();

			return ob;
		}

		public Mono.Addins.Setup.PackageRepositoryEntry ReadObject_PackageRepositoryEntry (bool isNullable, bool checkType)
		{
			Mono.Addins.Setup.PackageRepositoryEntry ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "PackageRepositoryEntry" || t.Namespace != "")
					throw CreateUnknownTypeException(t);
			}

			ob = (Mono.Addins.Setup.PackageRepositoryEntry) Activator.CreateInstance(typeof(Mono.Addins.Setup.PackageRepositoryEntry), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b10=false, b11=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "Addin" && Reader.NamespaceURI == "" && !b11) {
						b11 = true;
						ob.@Addin = ReadObject_AddinInfo (false, true);
					}
					else if (Reader.LocalName == "Url" && Reader.NamespaceURI == "" && !b10) {
						b10 = true;
						string s12 = Reader.ReadElementString ();
						ob.@Url = s12;
					}
					else {
						UnknownNode (ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public Mono.Addins.Setup.ReferenceRepositoryEntry ReadObject_ReferenceRepositoryEntry (bool isNullable, bool checkType)
		{
			Mono.Addins.Setup.ReferenceRepositoryEntry ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "ReferenceRepositoryEntry" || t.Namespace != "")
					throw CreateUnknownTypeException(t);
			}

			ob = (Mono.Addins.Setup.ReferenceRepositoryEntry) Activator.CreateInstance(typeof(Mono.Addins.Setup.ReferenceRepositoryEntry), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b13=false, b14=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "Url" && Reader.NamespaceURI == "" && !b13) {
						b13 = true;
						string s15 = Reader.ReadElementString ();
						ob.@Url = s15;
					}
					else if (Reader.LocalName == "LastModified" && Reader.NamespaceURI == "" && !b14) {
						b14 = true;
						string s16 = Reader.ReadElementString ();
						ob.@LastModified = XmlConvert.ToDateTime (s16, XmlDateTimeSerializationMode.RoundtripKind);
					}
					else {
						UnknownNode (ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public Mono.Addins.Setup.AddinInfo ReadObject_AddinInfo (bool isNullable, bool checkType)
		{
			Mono.Addins.Setup.AddinInfo ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "AddinInfo" || t.Namespace != "")
					throw CreateUnknownTypeException(t);
			}

			ob = (Mono.Addins.Setup.AddinInfo) Activator.CreateInstance(typeof(Mono.Addins.Setup.AddinInfo), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b17=false, b18=false, b19=false, b20=false, b21=false, b22=false, b23=false, b24=false, b25=false, b26=false, b27=false, b28=false, b29=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "Version" && Reader.NamespaceURI == "" && !b20) {
						b20 = true;
						string s30 = Reader.ReadElementString ();
						ob.@Version = s30;
					}
					else if (Reader.LocalName == "Dependencies" && Reader.NamespaceURI == "" && !b27) {
						if (((object)ob.@Dependencies) == null)
							throw CreateReadOnlyCollectionException ("Mono.Addins.Description.DependencyCollection");
						if (Reader.IsEmptyElement) {
							Reader.Skip();
						} else {
							int n31 = 0;
							Reader.ReadStartElement();
							Reader.MoveToContent();

							while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
							{
								if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
								{
									if (Reader.LocalName == "AssemblyDependency" && Reader.NamespaceURI == "") {
										if (((object)ob.@Dependencies) == null)
											throw CreateReadOnlyCollectionException ("Mono.Addins.Description.DependencyCollection");
										ob.@Dependencies.Add (ReadObject_AssemblyDependency (false, true));
										n31++;
									}
									else if (Reader.LocalName == "NativeDependency" && Reader.NamespaceURI == "") {
										if (((object)ob.@Dependencies) == null)
											throw CreateReadOnlyCollectionException ("Mono.Addins.Description.DependencyCollection");
										ob.@Dependencies.Add (ReadObject_NativeReference (false, true));
										n31++;
									}
									else if (Reader.LocalName == "AddinDependency" && Reader.NamespaceURI == "") {
										if (((object)ob.@Dependencies) == null)
											throw CreateReadOnlyCollectionException ("Mono.Addins.Description.DependencyCollection");
										ob.@Dependencies.Add (ReadObject_AddinReference (false, true));
										n31++;
									}
									else UnknownNode (null);
								}
								else UnknownNode (null);

								Reader.MoveToContent();
							}
							ReadEndElement();
						}
						b27 = true;
					}
					else if (Reader.LocalName == "Name" && Reader.NamespaceURI == "" && !b19) {
						b19 = true;
						string s32 = Reader.ReadElementString ();
						ob.@Name = s32;
					}
					else if (Reader.LocalName == "BaseVersion" && Reader.NamespaceURI == "" && !b21) {
						b21 = true;
						string s33 = Reader.ReadElementString ();
						ob.@BaseVersion = s33;
					}
					else if (Reader.LocalName == "Id" && Reader.NamespaceURI == "" && !b17) {
						b17 = true;
						string s34 = Reader.ReadElementString ();
						ob.@LocalId = s34;
					}
					else if (Reader.LocalName == "Url" && Reader.NamespaceURI == "" && !b24) {
						b24 = true;
						string s35 = Reader.ReadElementString ();
						ob.@Url = s35;
					}
					else if (Reader.LocalName == "Copyright" && Reader.NamespaceURI == "" && !b23) {
						b23 = true;
						string s36 = Reader.ReadElementString ();
						ob.@Copyright = s36;
					}
					else if (Reader.LocalName == "Description" && Reader.NamespaceURI == "" && !b25) {
						b25 = true;
						string s37 = Reader.ReadElementString ();
						ob.@Description = s37;
					}
					else if (Reader.LocalName == "Author" && Reader.NamespaceURI == "" && !b22) {
						b22 = true;
						string s38 = Reader.ReadElementString ();
						ob.@Author = s38;
					}
					else if (Reader.LocalName == "OptionalDependencies" && Reader.NamespaceURI == "" && !b28) {
						if (((object)ob.@OptionalDependencies) == null)
							throw CreateReadOnlyCollectionException ("Mono.Addins.Description.DependencyCollection");
						if (Reader.IsEmptyElement) {
							Reader.Skip();
						} else {
							int n39 = 0;
							Reader.ReadStartElement();
							Reader.MoveToContent();

							while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
							{
								if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
								{
									if (Reader.LocalName == "AssemblyDependency" && Reader.NamespaceURI == "") {
										if (((object)ob.@OptionalDependencies) == null)
											throw CreateReadOnlyCollectionException ("Mono.Addins.Description.DependencyCollection");
										ob.@OptionalDependencies.Add (ReadObject_AssemblyDependency (false, true));
										n39++;
									}
									else if (Reader.LocalName == "NativeDependency" && Reader.NamespaceURI == "") {
										if (((object)ob.@OptionalDependencies) == null)
											throw CreateReadOnlyCollectionException ("Mono.Addins.Description.DependencyCollection");
										ob.@OptionalDependencies.Add (ReadObject_NativeReference (false, true));
										n39++;
									}
									else if (Reader.LocalName == "AddinDependency" && Reader.NamespaceURI == "") {
										if (((object)ob.@OptionalDependencies) == null)
											throw CreateReadOnlyCollectionException ("Mono.Addins.Description.DependencyCollection");
										ob.@OptionalDependencies.Add (ReadObject_AddinReference (false, true));
										n39++;
									}
									else UnknownNode (null);
								}
								else UnknownNode (null);

								Reader.MoveToContent();
							}
							ReadEndElement();
						}
						b28 = true;
					}
					else if (Reader.LocalName == "Properties" && Reader.NamespaceURI == "" && !b29) {
						if (((object)ob.@Properties) == null)
							throw CreateReadOnlyCollectionException ("Mono.Addins.Setup.AddinPropertyCollectionImpl");
						if (Reader.IsEmptyElement) {
							Reader.Skip();
						} else {
							int n40 = 0;
							Reader.ReadStartElement();
							Reader.MoveToContent();

							while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
							{
								if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
								{
									if (Reader.LocalName == "Property" && Reader.NamespaceURI == "") {
										if (((object)ob.@Properties) == null)
											throw CreateReadOnlyCollectionException ("Mono.Addins.Setup.AddinPropertyCollectionImpl");
										ob.@Properties.Add (ReadObject_AddinProperty (false, true));
										n40++;
									}
									else UnknownNode (null);
								}
								else UnknownNode (null);

								Reader.MoveToContent();
							}
							ReadEndElement();
						}
						b29 = true;
					}
					else if (Reader.LocalName == "Namespace" && Reader.NamespaceURI == "" && !b18) {
						b18 = true;
						string s41 = Reader.ReadElementString ();
						ob.@Namespace = s41;
					}
					else if (Reader.LocalName == "Category" && Reader.NamespaceURI == "" && !b26) {
						b26 = true;
						string s42 = Reader.ReadElementString ();
						ob.@Category = s42;
					}
					else {
						UnknownNode (ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public Mono.Addins.Description.AssemblyDependency ReadObject_AssemblyDependency (bool isNullable, bool checkType)
		{
			Mono.Addins.Description.AssemblyDependency ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "AssemblyDependency" || t.Namespace != "")
					throw CreateUnknownTypeException(t);
			}

			ob = (Mono.Addins.Description.AssemblyDependency) Activator.CreateInstance(typeof(Mono.Addins.Description.AssemblyDependency), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b43=false, b44=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "Package" && Reader.NamespaceURI == "" && !b44) {
						b44 = true;
						string s45 = Reader.ReadElementString ();
						ob.@Package = s45;
					}
					else if (Reader.LocalName == "FullName" && Reader.NamespaceURI == "" && !b43) {
						b43 = true;
						string s46 = Reader.ReadElementString ();
						ob.@FullName = s46;
					}
					else {
						UnknownNode (ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public Mono.Addins.Description.NativeDependency ReadObject_NativeReference (bool isNullable, bool checkType)
		{
			Mono.Addins.Description.NativeDependency ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "NativeReference" || t.Namespace != "")
					throw CreateUnknownTypeException(t);
			}

			ob = (Mono.Addins.Description.NativeDependency) Activator.CreateInstance(typeof(Mono.Addins.Description.NativeDependency), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					UnknownNode (ob);
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public Mono.Addins.Description.AddinDependency ReadObject_AddinReference (bool isNullable, bool checkType)
		{
			Mono.Addins.Description.AddinDependency ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "AddinReference" || t.Namespace != "")
					throw CreateUnknownTypeException(t);
			}

			ob = (Mono.Addins.Description.AddinDependency) Activator.CreateInstance(typeof(Mono.Addins.Description.AddinDependency), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b47=false, b48=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "Version" && Reader.NamespaceURI == "" && !b48) {
						b48 = true;
						string s49 = Reader.ReadElementString ();
						ob.@Version = s49;
					}
					else if (Reader.LocalName == "AddinId" && Reader.NamespaceURI == "" && !b47) {
						b47 = true;
						string s50 = Reader.ReadElementString ();
						ob.@AddinId = s50;
					}
					else {
						UnknownNode (ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public Mono.Addins.Description.AddinProperty ReadObject_AddinProperty (bool isNullable, bool checkType)
		{
			Mono.Addins.Description.AddinProperty ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "AddinProperty" || t.Namespace != "")
					throw CreateUnknownTypeException(t);
			}

			ob = (Mono.Addins.Description.AddinProperty) Activator.CreateInstance(typeof(Mono.Addins.Description.AddinProperty), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "locale" && Reader.NamespaceURI == "") {
					ob.@Locale = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();


			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					UnknownNode (ob);
				}
				else if (Reader.NodeType == System.Xml.XmlNodeType.Text || Reader.NodeType == System.Xml.XmlNodeType.CDATA)
				{
					ob.@Value = ReadString (ob.@Value);
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		protected override void InitCallbacks ()
		{
		}

		protected override void InitIDs ()
		{
		}

	}

	internal class RepositoryWriter : XmlSerializationWriter
	{
		const string xmlNamespace = "http://www.w3.org/2000/xmlns/";
		static readonly System.Reflection.MethodInfo toBinHexStringMethod = typeof (XmlConvert).GetMethod ("ToBinHexString", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, new Type [] {typeof (byte [])}, null);
		static string ToBinHexString (byte [] input)
		{
			return input == null ? null : (string) toBinHexStringMethod.Invoke (null, new object [] {input});
		}
		public void WriteRoot_Repository (object o)
		{
			WriteStartDocument ();
			Mono.Addins.Setup.Repository ob = (Mono.Addins.Setup.Repository) o;
			TopLevelElement ();
			WriteObject_Repository (ob, "Repository", "", true, false, true);
		}

		void WriteObject_Repository (Mono.Addins.Setup.Repository ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(Mono.Addins.Setup.Repository))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Repository", "");

			WriteElementString ("Name", "", ob.@Name);
			WriteElementString ("Url", "", ob.@Url);
			if (ob.@Repositories != null) {
				for (int n51 = 0; n51 < ob.@Repositories.Count; n51++) {
					WriteObject_ReferenceRepositoryEntry (((Mono.Addins.Setup.ReferenceRepositoryEntry) ob.@Repositories[n51]), "Repository", "", false, false, true);
				}
			}
			if (ob.@Addins != null) {
				for (int n52 = 0; n52 < ob.@Addins.Count; n52++) {
					WriteObject_PackageRepositoryEntry (((Mono.Addins.Setup.PackageRepositoryEntry) ob.@Addins[n52]), "Addin", "", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_ReferenceRepositoryEntry (Mono.Addins.Setup.ReferenceRepositoryEntry ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(Mono.Addins.Setup.ReferenceRepositoryEntry))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("ReferenceRepositoryEntry", "");

			WriteElementString ("Url", "", ob.@Url);
			WriteElementString ("LastModified", "", XmlConvert.ToString (ob.@LastModified, XmlDateTimeSerializationMode.RoundtripKind));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_PackageRepositoryEntry (Mono.Addins.Setup.PackageRepositoryEntry ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(Mono.Addins.Setup.PackageRepositoryEntry))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("PackageRepositoryEntry", "");

			WriteElementString ("Url", "", ob.@Url);
			WriteObject_AddinInfo (ob.@Addin, "Addin", "", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_AddinInfo (Mono.Addins.Setup.AddinInfo ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(Mono.Addins.Setup.AddinInfo))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("AddinInfo", "");

			WriteElementString ("Id", "", ob.@LocalId);
			WriteElementString ("Namespace", "", ob.@Namespace);
			WriteElementString ("Name", "", ob.@Name);
			WriteElementString ("Version", "", ob.@Version);
			WriteElementString ("BaseVersion", "", ob.@BaseVersion);
			WriteElementString ("Author", "", ob.@Author);
			WriteElementString ("Copyright", "", ob.@Copyright);
			WriteElementString ("Url", "", ob.@Url);
			WriteElementString ("Description", "", ob.@Description);
			WriteElementString ("Category", "", ob.@Category);
			if (ob.@Dependencies != null) {
				WriteStartElement ("Dependencies", "", ob.@Dependencies);
				for (int n53 = 0; n53 < ob.@Dependencies.Count; n53++) {
					if (((object)ob.@Dependencies[n53]) == null) { }
					else if (ob.@Dependencies[n53].GetType() == typeof(Mono.Addins.Description.AssemblyDependency)) {
						WriteObject_AssemblyDependency (((Mono.Addins.Description.AssemblyDependency) ob.@Dependencies[n53]), "AssemblyDependency", "", false, false, true);
					}
					else if (ob.@Dependencies[n53].GetType() == typeof(Mono.Addins.Description.NativeDependency)) {
						WriteObject_NativeReference (((Mono.Addins.Description.NativeDependency) ob.@Dependencies[n53]), "NativeDependency", "", false, false, true);
					}
					else if (ob.@Dependencies[n53].GetType() == typeof(Mono.Addins.Description.AddinDependency)) {
						WriteObject_AddinReference (((Mono.Addins.Description.AddinDependency) ob.@Dependencies[n53]), "AddinDependency", "", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Dependencies[n53]);
				}
				WriteEndElement (ob.@Dependencies);
			}
			if (ob.@OptionalDependencies != null) {
				WriteStartElement ("OptionalDependencies", "", ob.@OptionalDependencies);
				for (int n54 = 0; n54 < ob.@OptionalDependencies.Count; n54++) {
					if (((object)ob.@OptionalDependencies[n54]) == null) { }
					else if (ob.@OptionalDependencies[n54].GetType() == typeof(Mono.Addins.Description.AssemblyDependency)) {
						WriteObject_AssemblyDependency (((Mono.Addins.Description.AssemblyDependency) ob.@OptionalDependencies[n54]), "AssemblyDependency", "", false, false, true);
					}
					else if (ob.@OptionalDependencies[n54].GetType() == typeof(Mono.Addins.Description.NativeDependency)) {
						WriteObject_NativeReference (((Mono.Addins.Description.NativeDependency) ob.@OptionalDependencies[n54]), "NativeDependency", "", false, false, true);
					}
					else if (ob.@OptionalDependencies[n54].GetType() == typeof(Mono.Addins.Description.AddinDependency)) {
						WriteObject_AddinReference (((Mono.Addins.Description.AddinDependency) ob.@OptionalDependencies[n54]), "AddinDependency", "", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@OptionalDependencies[n54]);
				}
				WriteEndElement (ob.@OptionalDependencies);
			}
			if (ob.@Properties != null) {
				WriteStartElement ("Properties", "", ob.@Properties);
				for (int n55 = 0; n55 < ob.@Properties.Count; n55++) {
					WriteObject_AddinProperty (ob.@Properties[n55], "Property", "", false, false, true);
				}
				WriteEndElement (ob.@Properties);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_AssemblyDependency (Mono.Addins.Description.AssemblyDependency ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(Mono.Addins.Description.AssemblyDependency))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("AssemblyDependency", "");

			WriteElementString ("FullName", "", ob.@FullName);
			WriteElementString ("Package", "", ob.@Package);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_NativeReference (Mono.Addins.Description.NativeDependency ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(Mono.Addins.Description.NativeDependency))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("NativeReference", "");

			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_AddinReference (Mono.Addins.Description.AddinDependency ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(Mono.Addins.Description.AddinDependency))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("AddinReference", "");

			WriteElementString ("AddinId", "", ob.@AddinId);
			WriteElementString ("Version", "", ob.@Version);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_AddinProperty (Mono.Addins.Description.AddinProperty ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(Mono.Addins.Description.AddinProperty))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("AddinProperty", "");

			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("locale", "", ob.@Locale);

			WriteValue (ob.@Value);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		protected override void InitCallbacks ()
		{
		}

	}
}

