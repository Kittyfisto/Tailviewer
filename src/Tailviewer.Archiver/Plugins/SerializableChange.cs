using System.Runtime.Serialization;

namespace Tailviewer.Archiver.Plugins
{
	[DataContract]
	public struct SerializableChange
	{
		/// <summary>
		///     A required short one sentence summary of the change.
		/// </summary>
		[DataMember]
		public string Summary { get; set; }

		/// <summary>
		///     An optional (possibly longer) description of the change.
		/// </summary>
		[DataMember]
		public string Description { get; set; }
	}
}