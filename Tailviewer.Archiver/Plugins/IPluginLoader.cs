using System;
using System.Collections.Generic;
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
		///     Actually loads and instantiates an implementation of the given <typeparamref name="T" /> plugin-interface
		///     that is part of the given plugin <paramref name="description"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="description"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">When the given plugin doesn't offer an implementation of the given interface</exception>
		T Load<T>(IPluginDescription description) where T : class, IPlugin;

		/// <summary>
		///     Loads all plugins in the given list that implement the given interface <typeparamref name="T" />.
		///     All plugins that cannot be loaded are ignored (an error is logged, but the caller is not notified).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IEnumerable<T> LoadAllOfType<T>() where T : class, IPlugin;
	}
}