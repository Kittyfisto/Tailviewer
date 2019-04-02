using System;

namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	///     Describes the version of an <see cref="IPlugin" /> specialization.
	/// </summary>
	/// <remarks>
	///     Whenever a breaking change is made to a plugin interface, or to a type used by a plugin interface,
	///     the interfaces version is incremented by the tailviewer developer(s). This in turn allows tailviewer
	///     to quickly determine if a particular plugin is compatible with the current version, or not without having
	///     to load the entire assembly.
	/// </remarks>
	public struct PluginInterfaceVersion
		: IEquatable<PluginInterfaceVersion>, IComparable<PluginInterfaceVersion>, IComparable
	{
		/// <summary>
		/// </summary>
		public static readonly PluginInterfaceVersion First;

		static PluginInterfaceVersion()
		{
			First = new PluginInterfaceVersion(version: 1);
		}

		private readonly int _version;

		/// <summary>
		/// </summary>
		/// <param name="version"></param>
		public PluginInterfaceVersion(int version)
		{
			_version = version;
		}

		/// <summary>
		///     The numerical value of this version.
		/// </summary>
		public int Value
		{
			get { return _version; }
		}

		#region Equality members

		/// <inheritdoc />
		public bool Equals(PluginInterfaceVersion other)
		{
			return _version == other._version;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is PluginInterfaceVersion && Equals((PluginInterfaceVersion) obj);
		}

		#region Relational members

		/// <inheritdoc />
		public int CompareTo(PluginInterfaceVersion other)
		{
			return _version.CompareTo(other._version);
		}

		/// <inheritdoc />
		public int CompareTo(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return 1;
			if (!(obj is PluginInterfaceVersion))
				throw new ArgumentException($"Object must be of type {nameof(PluginInterfaceVersion)}");
			return CompareTo((PluginInterfaceVersion) obj);
		}

		/// <summary>
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator <(PluginInterfaceVersion left, PluginInterfaceVersion right)
		{
			return left.CompareTo(right) < 0;
		}

		/// <summary>
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator >(PluginInterfaceVersion left, PluginInterfaceVersion right)
		{
			return left.CompareTo(right) > 0;
		}

		/// <summary>
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator <=(PluginInterfaceVersion left, PluginInterfaceVersion right)
		{
			return left.CompareTo(right) <= 0;
		}

		/// <summary>
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator >=(PluginInterfaceVersion left, PluginInterfaceVersion right)
		{
			return left.CompareTo(right) >= 0;
		}

		#endregion

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return _version;
		}

		/// <summary>
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(PluginInterfaceVersion left, PluginInterfaceVersion right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(PluginInterfaceVersion left, PluginInterfaceVersion right)
		{
			return !left.Equals(right);
		}

		#endregion
	}
}