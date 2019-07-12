using System.Xml.Serialization;

namespace Tailviewer.PluginRepository.Configuration
{
	[XmlRoot(ElementName = "configuration", IsNullable=false)]
	public sealed class ServerConfiguration
	{
		public ServerConfiguration()
		{
			Address = "0.0.0.0:1234";
			Publishing = new Publishing();
		}

		[XmlElement(ElementName = "address")]
		public string Address { get; set; }

		[XmlElement(ElementName = "publishing")]
		public Publishing Publishing { get; set; }
	}
}
