using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins.Description;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	/// An <see cref="IPluginLoader"/> implementation which keeps plugins alive once they've been loaded.
	/// </summary>
	public sealed class PluginCache
		: IPluginLoader
	{
		private readonly object _syncRoot;
		private readonly IPluginLoader _pluginLoader;
		private readonly Dictionary<Type, IReadOnlyList<IPluginWithDescription<IPlugin>>> _loadedPlugins;

		public PluginCache(IPluginLoader pluginLoader)
		{
			_syncRoot = new object();
			_pluginLoader = pluginLoader ?? throw new ArgumentNullException(nameof(pluginLoader));
			_loadedPlugins = new Dictionary<Type, IReadOnlyList<IPluginWithDescription<IPlugin>>>();
		}

		#region Implementation of IPluginLoader

		public IEnumerable<IPluginDescription> Plugins => _pluginLoader.Plugins;

		public IPluginStatus GetStatus(PluginId id)
		{
			return _pluginLoader.GetStatus(id);
		}

		public IReadOnlyDictionary<string, Type> ResolveSerializableTypes()
		{
			return _pluginLoader.ResolveSerializableTypes();
		}

		public IReadOnlyList<T> LoadAllOfType<T>() where T : class, IPlugin
		{
			var plugins = GetOrLoad<T>();
			return plugins.Select(x => x.Plugin).OfType<T>().ToList();
		}

		public IReadOnlyList<IPluginWithDescription<T>> LoadAllOfTypeWithDescription<T>() where T : class, IPlugin
		{
			var plugins = GetOrLoad<T>();
			return plugins.Cast<IPluginWithDescription<T>>().ToList();
		}

		#endregion

		private IReadOnlyList<IPluginWithDescription<IPlugin>> GetOrLoad<T>() where T : class, IPlugin
		{
			lock (_syncRoot)
			{
				if (!_loadedPlugins.TryGetValue(typeof(T), out var plugins))
				{
					var tmp = _pluginLoader.LoadAllOfTypeWithDescription<T>();
					if (tmp != null)
					{
						plugins = new List<IPluginWithDescription<IPlugin>>(tmp);
					}
					else
					{
						plugins = new List<IPluginWithDescription<IPlugin>>();
					}

					_loadedPlugins.Add(typeof(T), plugins);
				}

				return plugins;
			}
		}
	}
}
