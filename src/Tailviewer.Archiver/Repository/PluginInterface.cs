using System.Runtime.Serialization;

namespace Tailviewer.Archiver.Repository
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	///     Used during transport between repository and tailviewer client.
	/// </remarks>
	/// <remarks>
	///     DO NOT MAKE CHANGES TO THIS CLASS ONCE FINALIZED.
	///     Doing so will break the communication between tailviewer client
	///     and the repository.
	/// </remarks>
	[DataContract]
	public sealed class PluginInterface
	{
		public PluginInterface()
		{ }

		public PluginInterface(string fullName, int version)
		{
			FullName = fullName;
			Version = version;
		}

		[DataMember]
		public string FullName { get; set; }

		[DataMember]
		public int Version { get; set; }

		#region Overrides of ValueType

		public override string ToString()
		{
			return $"{FullName} v{Version}";
		}

		#endregion
	}
}