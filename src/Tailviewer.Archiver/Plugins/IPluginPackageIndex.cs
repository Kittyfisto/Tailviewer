using System;
using System.Collections.Generic;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	///     Describes the content of a plugin package to tailviewer.
	/// </summary>
	/// <remarks>
	///     Plugin authors do not need to use this interface.
	/// </remarks>
	public interface IPluginPackageIndex
	{
		/// <summary>
		///     The version of the plugin archive.
		///     Used to find out of a plugin has been created with an older
		///     software and thus shouldn't be supported anymore (or is too new...).
		/// </summary>
		int PluginArchiveVersion { get; }

		/// <summary>
		///     The id of the plugin. Should be long enough to guarantuee to be unique
		///     amongst all other plugins.
		///     Will not be shown the user.
		/// </summary>
		string Id { get; }

		/// <summary>
		///     The name of the plugin, as visible to the user.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The author of the plugin.
		///     Will be displayed to the user.
		/// </summary>
		string Author { get; }

		/// <summary>
		/// </summary>
		string Description { get; }

		/// <summary>
		/// </summary>
		string Website { get; }

		/// <summary>
		/// </summary>
		Version Version { get; }

		/// <summary>
		///     The list of assemblies contained in the plugin package.
		/// </summary>
		IReadOnlyList<IAssemblyDescription> Assemblies { get; }

		/// <summary>
		///     The list of native images contained in the plugin package.
		/// </summary>
		IReadOnlyList<INativeImageDescription> NativeImages { get; }

		/// <summary>
		///     The list of plugin-interface implementations, contained in the plugin assembly.
		/// </summary>
		IEnumerable<PluginInterfaceImplementation> ImplementedPluginInterfaces { get; }

		/// <summary>
		///     The list of serializable types implemented by the plugin.
		/// </summary>
		IEnumerable<SerializableTypeDescription> SerializableTypes { get; }

		/// <summary>
		///     The list of changes made to this plugin compared to the last version.
		/// </summary>
		IReadOnlyList<SerializableChange> Changes { get; }
	}
}