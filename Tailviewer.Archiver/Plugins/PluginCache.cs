using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	/// An <see cref="IPluginLoader"/> implementation which keeps plugins alive once they've been loaded.
	/// </summary>
	public sealed class PluginCache
		: IPluginLoader
	{
		private readonly IPluginLoader _pluginLoader;
		private readonly Dictionary<Type, IReadOnlyList<IPlugin>> _loadedPlugins;

		public PluginCache(IPluginLoader pluginLoader)
		{
			_pluginLoader = pluginLoader ?? throw new ArgumentNullException(nameof(pluginLoader));
			_loadedPlugins = new Dictionary<Type, IReadOnlyList<IPlugin>>();
		}

		#region Implementation of IPluginLoader

		public IEnumerable<IPluginDescription> Plugins => _pluginLoader.Plugins;

		public IPluginStatus GetStatus(IPluginDescription description)
		{
			return _pluginLoader.GetStatus(description);
		}

		public IReadOnlyList<T> LoadAllOfType<T>() where T : class, IPlugin
		{
			if (!_loadedPlugins.TryGetValue(typeof(T), out var plugins))
			{
				plugins = _pluginLoader.LoadAllOfType<T>()?.ToList() ?? new List<T>();
				_loadedPlugins.Add(typeof(T), plugins);
			}

			return plugins.OfType<T>().ToList();
		}

		#endregion
	}
}
