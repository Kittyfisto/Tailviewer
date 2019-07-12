using System;
using System.Runtime.Serialization;

namespace Tailviewer.Archiver.Repository
{
	/// <summary>
	///     Describes a plugin as part of plugin repository.
	/// </summary>
	/// <remarks>
	///     Used during transport between repository and tailviewer client.
	/// </remarks>
	/// <remarks>
	///     DO NOT MAKE CHANGES TO THIS CLASS ONCE FINALISED.
	///     Doing so will break the communication between tailviewer client
	///     and the repository.
	/// </remarks>
	[DataContract]
	public sealed class PublishedPluginDescription
	{
		[DataMember]
		public PluginIdentifier Identifier { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string Author { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public string Website { get; set; }

		[DataMember]
		public string Publisher { get; set; }

		[DataMember]
		public DateTime PublishTimestamp { get; set; }

		[DataMember]
		public long SizeInBytes { get; set; }
	}
}