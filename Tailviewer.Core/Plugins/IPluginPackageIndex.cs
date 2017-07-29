using System;
using System.Collections.Generic;

namespace Tailviewer.Core.Plugins
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
		string PluginAuthor { get; }

		/// <summary>
		/// 
		/// </summary>
		string PluginDescription { get; }

		/// <summary>
		/// 
		/// </summary>
		string PluginWebsite { get; }

		/// <summary>
		///     The list of assemblies contained in the plugin package.
		/// </summary>
		IReadOnlyList<IAssemblyDescription> Assemblies { get; }
	}
}