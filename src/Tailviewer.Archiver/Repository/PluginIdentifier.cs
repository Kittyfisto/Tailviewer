using System;
using System.Runtime.Serialization;

namespace Tailviewer.Archiver.Repository
{
	/// <summary>
	///    Identifies a plugin as part of a plugin repository.
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
	public class PluginIdentifier : IEquatable<PluginIdentifier>
	{
		public PluginIdentifier()
		{ }

		public PluginIdentifier(string id, Version version)
		{
			Id = id;
			Version = version;
		}

		#region Equality members

		public bool Equals(PluginIdentifier other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(Id, other.Id) && Equals(Version, other.Version);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((PluginIdentifier) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (Version != null ? Version.GetHashCode() : 0);
			}
		}

		#endregion

		[DataMember]
		public string Id { get; set; }

		[DataMember]
		public Version Version { get; set; }

		#region Overrides of ValueType

		public override string ToString()
		{
			return $"{Id} v{Version}";
		}

		#endregion
	}
}