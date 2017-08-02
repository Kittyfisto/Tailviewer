using System.Runtime.Serialization;

namespace Tailviewer.Archiver.Plugins
{
	[DataContract]
	public sealed class NativeImageDescription
		: INativeImageDescription
	{
		/// <inheritdoc />
		[DataMember]
		public string EntryName { get; set; }

		/// <inheritdoc />
		[DataMember]
		public string ImageName { get; set; }
	}
}