using System.Collections.Generic;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Core.Plugins
{
	public interface IPluginLoader
	{
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