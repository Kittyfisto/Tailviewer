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
		/// 
		/// </summary>
		string Author { get; }

		/// <summary>
		/// 
		/// </summary>
		string Description { get; }

		/// <summary>
		/// 
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
	}
}