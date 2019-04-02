using System;

namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	/// </summary>
	public struct PluginInterfaceVersion
		: IEquatable<PluginInterfaceVersion>
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

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return _version;
		}

		#endregion
	}
}