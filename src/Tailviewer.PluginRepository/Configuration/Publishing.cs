using System.Xml;
using System.Xml.Serialization;

namespace Tailviewer.PluginRepository.Configuration
{
	public sealed class Publishing
	{
		public Publishing()
		{
			Email = "admin@root";
			AllowRemotePublish = false;
		}

		[XmlAnyElement("AllowRemoteComment")]
		public XmlComment AllowRemoteComment
		{
			get { return new XmlDocument().CreateComment("When set to true, plugins can be published to this repository remotely via archver.exe, \r\n        otherwise plugins can only be published via the repository.exe commandline on this machine"); }
			set { }
		}

		[XmlElement(ElementName = "allow-remote-publish")]
		public bool AllowRemotePublish { get; set; }

		[XmlAnyElement("PublishComment")]
		public XmlComment PublishComment
		{
			get { return new XmlDocument().CreateComment("Controls if and when emails are sent in case new plugins are published"); }
			set { }
		}

		[XmlElement(ElementName = "notify")]
		public bool SendMail { get; set; }

		[XmlElement(ElementName = "email")]
		public string Email { get; set; }
	}
}