using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Tailviewer.Archiver.Plugins
{
	[DataContract]
	public sealed class SerializableChange
	{
		/// <summary>
		///     A required short one sentence summary of the change.
		/// </summary>
		[DataMember]
		[XmlElement(ElementName = "summary")]
		public string Summary { get; set; }

		/// <summary>
		///     An optional (possibly longer) description of the change.
		/// </summary>
		[DataMember]
		[XmlElement(ElementName = "description")]
		public string Description { get; set; }
	}
}