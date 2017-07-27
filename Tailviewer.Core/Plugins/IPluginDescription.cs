using System;
using System.Collections.Generic;

namespace Tailviewer.Core.Plugins
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
		///     The website of the plugin, if available.
		/// </summary>
		/// <remarks>
		///     This value is simply retrieved from the plugin and not verified in any way.
		/// </remarks>
		Uri Website { get; }

		/// <summary>
		///     A map from the plugin interface to its actual implementation.
		/// </summary>
		IReadOnlyDictionary<Type, string> Plugins { get; }
	}
}