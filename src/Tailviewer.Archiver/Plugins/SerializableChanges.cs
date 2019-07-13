using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using log4net;

namespace Tailviewer.Archiver.Plugins
{
	[XmlRoot(ElementName = "changelist", IsNullable=false)]
	public sealed class SerializableChanges
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[XmlArray("changes")]
		[XmlArrayItem("change")]
		public List<SerializableChange> Changes { get; }

		public SerializableChanges()
		{
			Changes = new List<SerializableChange>();
		}

		public void Serialize(Stream stream)
		{
			XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
			namespaces.Add("", "");

			var serializer = new XmlSerializer(typeof(SerializableChanges));
			serializer.Serialize(stream, this, namespaces);
		}

		public static SerializableChanges Deserialize(Stream stream)
		{
			XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
			namespaces.Add("", "");

			var serializer = new XmlSerializer(typeof(SerializableChanges));
			serializer.UnknownNode+= UnknownNode;
			serializer.UnknownAttribute+= UnknownAttribute;
			return (SerializableChanges) serializer.Deserialize(stream);
		}

		private static void UnknownNode(object sender, XmlNodeEventArgs e)
		{
			Log.WarnFormat("Unknown Node:{0}\t{1}", e.Name, e.Text);
		}

		private static void UnknownAttribute(object sender, XmlAttributeEventArgs e)
		{
			System.Xml.XmlAttribute attr = e.Attr;
			Log.WarnFormat("Unknown attribute: {0}={1}", attr.Name, attr.Value);
		}
	}
}