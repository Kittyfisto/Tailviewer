using System.Xml.Serialization;

namespace Tailviewer.PluginRepository.Configuration
{
	[XmlRoot(ElementName = "configuration", IsNullable=false)]
	public sealed class ServerConfiguration
	{
		public ServerConfiguration()
		{
			Publishing = new Publishing();
		}

		[XmlElement(ElementName = "publishing")]
		public Publishing Publishing { get; set; }
	}
}
