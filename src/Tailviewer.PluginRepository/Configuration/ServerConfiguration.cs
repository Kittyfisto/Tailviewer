using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using log4net;

namespace Tailviewer.PluginRepository.Configuration
{
	[XmlRoot(ElementName = "configuration", IsNullable=false)]
	public sealed class ServerConfiguration
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public ServerConfiguration()
		{
			Address = "0.0.0.0:1234";
			Publishing = new Publishing();
		}

		[XmlElement(ElementName = "address")]
		public string Address { get; set; }

		[XmlElement(ElementName = "publishing")]
		public Publishing Publishing { get; set; }

		public void WriteTo(string fileName)
		{
			var serializer = new XmlSerializer(typeof(ServerConfiguration));
			using (var fileStream = File.Create(fileName))
			{
				XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
				namespaces.Add("", "");
				serializer.Serialize(fileStream, new ServerConfiguration(), namespaces);
			}
		}

		public static ServerConfiguration ReadOrCreate(string fileName)
		{
			if (!File.Exists(fileName))
			{
				var config = new ServerConfiguration();

				TryWriteTo(fileName, config);

				return config;
			}

			return TryRead(fileName);
		}

		private static void TryWriteTo(string fileName, ServerConfiguration config)
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(fileName));
				config.WriteTo(fileName);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Unable to write configuration to disk: {0}", e);
			}
		}

		[Pure]
		public static ServerConfiguration TryRead(string fileName)
		{
			var serializer = new XmlSerializer(typeof(ServerConfiguration));
			serializer.UnknownNode += UnknownNode;
			serializer.UnknownAttribute += UnknownAttribute;

			using (var filestream = File.OpenRead(fileName))
			{
				try
				{
					return (ServerConfiguration)serializer.Deserialize(filestream);
				}
				catch (Exception e)
				{
					Log.WarnFormat("Unable to read configuration '{0}': {1}", fileName, e.Message);
					return new ServerConfiguration();
				}
			}
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
