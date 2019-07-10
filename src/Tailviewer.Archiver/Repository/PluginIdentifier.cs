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
			return string.Equals(Id, other.Id) && Equals(Version, other.Version);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is PluginIdentifier && Equals((PluginIdentifier) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (Version != null ? Version.GetHashCode() : 0);
			}
		}

		public static bool operator ==(PluginIdentifier left, PluginIdentifier right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PluginIdentifier left, PluginIdentifier right)
		{
			return !left.Equals(right);
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