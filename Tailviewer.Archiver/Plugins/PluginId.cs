using System;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	/// Uniquely identifies a plugin.
	/// </summary>
	/// <remarks>
	///     The same plugin should always have the same id over its development cycle (i.e. the id should NOT
	///     contain any versions information).
	/// </remarks>
	/// <remarks>
	///     Plugin ids should be 
	/// </remarks>
	/// <example>
	///     Suppose you're working for a company named "Strawberry" in a team named "Rocketeers" and
	///     you're developing a plugin to read your custom log format.
	///
	///     A good id would be:
	///         new PluginId("Strawberry.Rocketeers", "Log")
	/// </example>
	public sealed class PluginId : IEquatable<PluginId>
	{
		private readonly string _value;

		public PluginId(string value)
		{
			_value = value;
		}

		public string Value => _value;

		public bool Equals(PluginId other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(_value, other._value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is PluginId other && Equals(other);
		}

		public override int GetHashCode()
		{
			return (_value != null ? _value.GetHashCode() : 0);
		}

		public static bool operator ==(PluginId left, PluginId right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(PluginId left, PluginId right)
		{
			return !Equals(left, right);
		}

		public override string ToString()
		{
			return _value;
		}
	}
}
