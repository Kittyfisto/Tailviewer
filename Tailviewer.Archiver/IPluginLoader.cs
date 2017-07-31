using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Core.Plugins
{
	/// <summary>
	/// 
	/// </summary>
	public interface IPluginLoader
	{
		/// <summary>
		/// 
		/// </summary>
		IEnumerable<IPluginDescription> Plugins { get; }

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
		/// <param name="pluginDescriptions"></param>
		/// <returns></returns>
		IEnumerable<T> LoadAllOfType<T>(IEnumerable<IPluginDescription> pluginDescriptions) where T : class, IPlugin;
	}
}