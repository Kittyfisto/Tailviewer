using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Tailviewer.Archiver.Plugins
{
	[DataContract]
	public sealed class SerializableChange
	{
		/// <summary>
		///     The unique id of this change. There should never be another change (for the same plugin)
		///     with that id.
		/// </summary>
		/// <remarks>
		///     This field exists to allow tailviewer to easily compare two plugin versions for changes made.
		/// </remarks>
		[DataMember]
		[XmlAttribute(AttributeName = "id")]
		public string Id { get; set; }

		/// <summary>
		///     A required short one sentence summary of the change.
		/// </summary>
		[DataMember]
		[XmlAttribute(AttributeName = "summary")]
		public string Summary { get; set; }

		/// <summary>
		///     An optional (possibly longer) description of the change.
		/// </summary>
		[DataMember]
		[XmlAttribute(AttributeName = "description")]
		public string Description { get; set; }
	}
}