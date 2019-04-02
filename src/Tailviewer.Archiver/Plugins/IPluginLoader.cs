using System;
using System.Collections.Generic;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	/// 
	/// </summary>
	public interface IPluginLoader
	{
		/// <summary>
		/// The list of installed plugins.
		/// </summary>
		IEnumerable<IPluginDescription> Plugins { get; }

		/// <summary>
		/// Obtains status information for that particular plugin.
		/// Includes potential errors, etc...
		/// </summary>
		/// <param name="description"></param>
		/// <returns></returns>
		IPluginStatus GetStatus(IPluginDescription description);

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		IReadOnlyDictionary<string, Type> ResolveSerializableTypes();

		/// <summary>
		///     Loads all plugins in the given list that implement the given interface <typeparamref name="T" />.
		///     All plugins that cannot be loaded are ignored (an error is logged, but the caller is not notified).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IReadOnlyList<T> LoadAllOfType<T>() where T : class, IPlugin;
	}
}