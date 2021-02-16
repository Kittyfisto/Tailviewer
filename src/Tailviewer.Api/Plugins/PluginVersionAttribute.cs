using System;

namespace Tailviewer.Plugins
{
	/// <summary>
	///     This attribute should be placed alongside other attributes in your
	///     AssemblyInfo.cs file. Describes the version of a tailviewer plugin.
	///     Will be displayed to the user.
	/// </summary>
	/// <remarks>
	///     A version number consists of up to 4 numbers: major, minor, build and revision.
	///     Numbers on the left are more important than those to the right.
	/// </remarks>
	/// <example>
	///     [assembly: PluginVersion(1, 0)]
	///     [assembly: PluginVersion(1, 1, 2)]
	///     [assembly: PluginVersion(0, 1, 3, 24)]
	/// </example>
	[AttributeUsage(AttributeTargets.Assembly)]
	public sealed class PluginVersionAttribute
		: Attribute
	{
		/// <summary>
		///     Initializes this attribute.
		/// </summary>
		/// <param name="version"></param>
		public PluginVersionAttribute(Version version)
		{
			Version = version;
		}

		/// <summary>
		///     Initializes this attribute.
		/// </summary>
		/// <param name="majorVersion"></param>
		/// <param name="minorVersion"></param>
		public PluginVersionAttribute(int majorVersion, int minorVersion)
		{
			Version = new Version(majorVersion, minorVersion);
		}

		/// <summary>
		///     Initializes this attribute.
		/// </summary>
		/// <param name="majorVersion"></param>
		/// <param name="minorVersion"></param>
		/// <param name="build"></param>
		public PluginVersionAttribute(int majorVersion, int minorVersion, int build)
		{
			Version = new Version(majorVersion, minorVersion, build);
		}

		/// <summary>
		///     Initializes this attribute.
		/// </summary>
		/// <param name="majorVersion"></param>
		/// <param name="minorVersion"></param>
		/// <param name="build"></param>
		/// <param name="revision"></param>
		public PluginVersionAttribute(int majorVersion, int minorVersion, int build, int revision)
		{
			Version = new Version(majorVersion, minorVersion, build, revision);
		}

		/// <summary>
		///     The version of this plugin.
		/// </summary>
		public Version Version { get; }
	}
}