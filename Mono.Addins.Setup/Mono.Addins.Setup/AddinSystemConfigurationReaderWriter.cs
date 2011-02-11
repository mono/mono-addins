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
	internal class AddinSystemConfigurationReader : XmlSerializationReader
	{
		static readonly System.Reflection.MethodInfo fromBinHexStringMethod = typeof (XmlConvert).GetMethod ("FromBinHexString", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, new Type [] {typeof (string)}, null);
		static byte [] FromBinHexString (string input)
		{
			return input == null ? null : (byte []) fromBinHexStringMethod.Invoke (null, new object [] {input});
		}
		public object ReadRoot_AddinSystemConfiguration ()
		{
			Reader.MoveToContent();
			if (Reader.LocalName != "AddinSystemConfiguration" || Reader.NamespaceURI != "")
				throw CreateUnknownNodeException();
			return ReadObject_AddinSystemConfiguration (true, true);
		}

		public Mono.Addins.Setup.AddinSystemConfiguration ReadObject_AddinSystemConfiguration (bool isNullable, bool checkType)
		{
			Mono.Addins.Setup.AddinSystemConfiguration ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "AddinSystemConfiguration" || t.Namespace != "")
					throw CreateUnknownTypeException(t);
			}

			ob = (Mono.Addins.Setup.AddinSystemConfiguration) Activator.CreateInstance(typeof(Mono.Addins.Setup.AddinSystemConfiguration), true);

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

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "AddinPaths" && Reader.NamespaceURI == "" && !b3) {
						if (((object)ob.@AddinPaths) == null)
							throw CreateReadOnlyCollectionException ("System.Collections.Specialized.StringCollection");
						if (Reader.IsEmptyElement) {
							Reader.Skip();
						} else {
							int n4 = 0;
							Reader.ReadStartElement();
							Reader.MoveToContent();

							while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
							{
								if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
								{
									if (Reader.LocalName == "Addin" && Reader.NamespaceURI == "") {
										string s5 = Reader.ReadElementString ();
										if (((object)ob.@AddinPaths) == null)
											throw CreateReadOnlyCollectionException ("System.Collections.Specialized.StringCollection");
										ob.@AddinPaths.Add (s5);
										n4++;
									}
									else UnknownNode (null);
								}
								else UnknownNode (null);

								Reader.MoveToContent();
							}
							ReadEndElement();
						}
						b3 = true;
					}
					else if (Reader.LocalName == "RepositoryIdCount" && Reader.NamespaceURI == "" && !b1) {
						b1 = true;
						string s6 = Reader.ReadElementString ();
						ob.@RepositoryIdCount = Int32.Parse (s6, CultureInfo.InvariantCulture);
					}
					else if (Reader.LocalName == "DisabledAddins" && Reader.NamespaceURI == "" && !b2) {
						if (((object)ob.@DisabledAddins) == null)
							throw CreateReadOnlyCollectionException ("System.Collections.Specialized.StringCollection");
						if (Reader.IsEmptyElement) {
							Reader.Skip();
						} else {
							int n7 = 0;
							Reader.ReadStartElement();
							Reader.MoveToContent();

							while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
							{
								if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
								{
									if (Reader.LocalName == "Addin" && Reader.NamespaceURI == "") {
										string s8 = Reader.ReadElementString ();
										if (((object)ob.@DisabledAddins) == null)
											throw CreateReadOnlyCollectionException ("System.Collections.Specialized.StringCollection");
										ob.@DisabledAddins.Add (s8);
										n7++;
									}
									else UnknownNode (null);
								}
								else UnknownNode (null);

								Reader.MoveToContent();
							}
							ReadEndElement();
						}
						b2 = true;
					}
					else if (Reader.LocalName == "Repositories" && Reader.NamespaceURI == "" && !b0) {
						if (((object)ob.@Repositories) == null)
							throw CreateReadOnlyCollectionException ("System.Collections.ArrayList");
						if (Reader.IsEmptyElement) {
							Reader.Skip();
						} else {
							int n9 = 0;
							Reader.ReadStartElement();
							Reader.MoveToContent();

							while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
							{
								if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
								{
									if (Reader.LocalName == "Repository" && Reader.NamespaceURI == "") {
										if (((object)ob.@Repositories) == null)
											throw CreateReadOnlyCollectionException ("System.Collections.ArrayList");
										ob.@Repositories.Add (ReadObject_RepositoryRecord (false, true));
										n9++;
									}
									else UnknownNode (null);
								}
								else UnknownNode (null);

								Reader.MoveToContent();
							}
							ReadEndElement();
						}
						b0 = true;
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

		public Mono.Addins.Setup.RepositoryRecord ReadObject_RepositoryRecord (bool isNullable, bool checkType)
		{
			Mono.Addins.Setup.RepositoryRecord ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "RepositoryRecord" || t.Namespace != "")
					throw CreateUnknownTypeException(t);
			}

			ob = (Mono.Addins.Setup.RepositoryRecord) Activator.CreateInstance(typeof(Mono.Addins.Setup.RepositoryRecord), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
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

			bool b10=false, b11=false, b12=false, b13=false, b14=false, b15=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "File" && Reader.NamespaceURI == "" && !b11) {
						b11 = true;
						string s16 = Reader.ReadElementString ();
						ob.@File = s16;
					}
					else if (Reader.LocalName == "Enabled" && Reader.NamespaceURI == "" && !b15) {
						b15 = true;
						string s17 = Reader.ReadElementString ();
						ob.@Enabled = XmlConvert.ToBoolean (s17);
					}
					else if (Reader.LocalName == "IsReference" && Reader.NamespaceURI == "" && !b10) {
						b10 = true;
						string s18 = Reader.ReadElementString ();
						ob.@IsReference = XmlConvert.ToBoolean (s18);
					}
					else if (Reader.LocalName == "Name" && Reader.NamespaceURI == "" && !b13) {
						b13 = true;
						string s19 = Reader.ReadElementString ();
						ob.@Name = s19;
					}
					else if (Reader.LocalName == "Url" && Reader.NamespaceURI == "" && !b12) {
						b12 = true;
						string s20 = Reader.ReadElementString ();
						ob.@Url = s20;
					}
					else if (Reader.LocalName == "LastModified" && Reader.NamespaceURI == "" && !b14) {
						b14 = true;
						string s21 = Reader.ReadElementString ();
						ob.@LastModified = XmlConvert.ToDateTime (s21, XmlDateTimeSerializationMode.RoundtripKind);
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

		protected override void InitCallbacks ()
		{
		}

		protected override void InitIDs ()
		{
		}

	}

	internal class AddinSystemConfigurationWriter : XmlSerializationWriter
	{
		const string xmlNamespace = "http://www.w3.org/2000/xmlns/";
		static readonly System.Reflection.MethodInfo toBinHexStringMethod = typeof (XmlConvert).GetMethod ("ToBinHexString", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, new Type [] {typeof (byte [])}, null);
		static string ToBinHexString (byte [] input)
		{
			return input == null ? null : (string) toBinHexStringMethod.Invoke (null, new object [] {input});
		}
		public void WriteRoot_AddinSystemConfiguration (object o)
		{
			WriteStartDocument ();
			Mono.Addins.Setup.AddinSystemConfiguration ob = (Mono.Addins.Setup.AddinSystemConfiguration) o;
			TopLevelElement ();
			WriteObject_AddinSystemConfiguration (ob, "AddinSystemConfiguration", "", true, false, true);
		}

		void WriteObject_AddinSystemConfiguration (Mono.Addins.Setup.AddinSystemConfiguration ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(Mono.Addins.Setup.AddinSystemConfiguration))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("AddinSystemConfiguration", "");

			if (ob.@Repositories != null) {
				WriteStartElement ("Repositories", "", ob.@Repositories);
				for (int n22 = 0; n22 < ob.@Repositories.Count; n22++) {
					WriteObject_RepositoryRecord (((Mono.Addins.Setup.RepositoryRecord) ob.@Repositories[n22]), "Repository", "", false, false, true);
				}
				WriteEndElement (ob.@Repositories);
			}
			WriteElementString ("RepositoryIdCount", "", ob.@RepositoryIdCount.ToString(CultureInfo.InvariantCulture));
			if (ob.@DisabledAddins != null) {
				WriteStartElement ("DisabledAddins", "", ob.@DisabledAddins);
				for (int n23 = 0; n23 < ob.@DisabledAddins.Count; n23++) {
					WriteElementString ("Addin", "", ob.@DisabledAddins[n23]);
				}
				WriteEndElement (ob.@DisabledAddins);
			}
			if (ob.@AddinPaths != null) {
				WriteStartElement ("AddinPaths", "", ob.@AddinPaths);
				for (int n24 = 0; n24 < ob.@AddinPaths.Count; n24++) {
					WriteElementString ("Addin", "", ob.@AddinPaths[n24]);
				}
				WriteEndElement (ob.@AddinPaths);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_RepositoryRecord (Mono.Addins.Setup.RepositoryRecord ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(Mono.Addins.Setup.RepositoryRecord))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("RepositoryRecord", "");

			WriteAttribute ("id", "", ob.@Id);

			WriteElementString ("IsReference", "", (ob.@IsReference?"true":"false"));
			WriteElementString ("File", "", ob.@File);
			WriteElementString ("Url", "", ob.@Url);
			WriteElementString ("Name", "", ob.@Name);
			WriteElementString ("LastModified", "", XmlConvert.ToString (ob.@LastModified, XmlDateTimeSerializationMode.RoundtripKind));
			if (ob.@Enabled != true) {
				WriteElementString ("Enabled", "", (ob.@Enabled?"true":"false"));
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		protected override void InitCallbacks ()
		{
		}
	}
}

