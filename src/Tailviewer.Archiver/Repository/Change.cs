using System.Runtime.Serialization;

namespace Tailviewer.Archiver.Repository
{
	/// <summary>
	///    Describes a singular change made to a plugin.
	/// </summary>
	/// <remarks>
	///     Used during transport between repository and tailviewer client.
	/// </remarks>
	/// <remarks>
	///     DO NOT MAKE CHANGES TO THIS CLASS ONCE FINALISED.
	///     Doing so will break the communication between tailviewer client
	///     and the repository.
	/// </remarks>
	public sealed class Change
	{
		/// <summary>
		///    A short (one sentence) summary of the change, mandatory.
		/// </summary>
		[DataMember]
		public string Summary { get; set; }

		/// <summary>
		///    An optional (detailed) description of the change.
		/// </summary>
		[DataMember]
		public string Description { get; set; }
	}
}
