using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Tailviewer.Archiver.Plugins.Description
{
	/// <summary>
	///     Describes a tailviewer plugin.
	/// </summary>
	public interface IPluginDescription
	{
		/// <summary>
		///     The full filepath of the plugin.
		/// </summary>
		string FilePath { get; }

		/// <summary>
		///     A unique id for this plugin.
		/// </summary>
		PluginId Id { get; }

		/// <summary>
		///     The (human-readable) name of the plugin.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The author of the plugin, if available.
		/// </summary>
		/// <remarks>
		///     This value is simply retrieved from the plugin and not verified in any way.
		/// </remarks>
		string Author { get; }

		/// <summary>
		///     The description of the plugin, if available.
		/// </summary>
		/// <remarks>
		///     This value is simply retrieved from the plugin and not verified in any way.
		/// </remarks>
		string Description { get; }

		/// <summary>
		///     The version of the plugin, as visible to the user.
		/// </summary>
		Version Version { get; }

		/// <summary>
		///     The website of the plugin, if available.
		/// </summary>
		/// <remarks>
		///     This value is simply retrieved from the plugin and not verified in any way.
		/// </remarks>
		Uri Website { get; }

		/// <summary>
		/// 
		/// </summary>
		ImageSource Icon { get; }

		/// <summary>
		///     A human readable error that explains why a plugin couldn't be loaded.
		/// </summary>
		/// <remarks>
		///     When null, then the plugin could be loaded.
		/// </remarks>
		string Error { get; }

		/// <summary>
		///     A map of all <see cref="BusinessLogic.Plugins.IPlugin"/> implementations offered by the plugin.
		/// </summary>
		IReadOnlyList<IPluginImplementationDescription> PluginImplementations { get; }

		/// <summary>
		///     A map from an immutable name to a .NET assembly qualified typename of a serializable type.
		/// </summary>
		IReadOnlyDictionary<string, string> SerializableTypes { get; }

		/// <summary>
		///     The list of changes made to this plugin compared to the last version.
		/// </summary>
		IReadOnlyList<IChange> Changes { get; }

		/// <summary>
		///     The version of the Tailviewer API this plugins has been compiled against.
		/// </summary>
		/// <remarks>
		///     May not be available for older plugins compiled against v0.9 and older.
		/// </remarks>
		Version TailviewerApiVersion { get; }
	}
}