using System;
using System.Runtime.Serialization;

namespace Tailviewer.Archiver.Registry
{
	/// <summary>
	/// </summary>
	[DataContract]
	public struct PluginRegistryId : IEquatable<PluginRegistryId>
	{
		public PluginRegistryId(string id, Version version)
		{
			Id = id;
			Version = version;
		}

		#region Equality members

		public bool Equals(PluginRegistryId other)
		{
			return string.Equals(Id, other.Id) && Equals(Version, other.Version);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is PluginRegistryId && Equals((PluginRegistryId) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (Version != null ? Version.GetHashCode() : 0);
			}
		}

		public static bool operator ==(PluginRegistryId left, PluginRegistryId right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PluginRegistryId left, PluginRegistryId right)
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