using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins.Description;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	///     A <see cref="IPluginLoader" /> implementation which aggregates plugins from multiple other
	///     <see cref="IPluginLoader" /> objects.
	/// </summary>
	public sealed class AggregatedPluginLoader
		: IPluginLoader
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IReadOnlyList<IPluginLoader> _pluginLoaders;

		public AggregatedPluginLoader(IEnumerable<IPluginLoader> pluginLoaders)
			: this(pluginLoaders.ToArray())
		{}

		public AggregatedPluginLoader(params IPluginLoader[] pluginLoaders)
		{
			_pluginLoaders = pluginLoaders;
		}

		#region Implementation of IPluginLoader

		public IEnumerable<IPluginDescription> Plugins => _pluginLoaders.SelectMany(x => x.Plugins).ToList();

		public IPluginStatus GetStatus(PluginId id)
		{
			foreach (var pluginLoader in _pluginLoaders)
			{
				var status = pluginLoader.GetStatus(id);
				if (status.IsInstalled)
				{
					return status;
				}
			}

			return new PluginStatus
			{
				IsInstalled = false,
				IsLoaded = false
			};
		}

		public IReadOnlyDictionary<string, Type> ResolveSerializableTypes()
		{
			var types = new Dictionary<string, Type>();
			foreach (var pluginLoader in _pluginLoaders)
			{
				foreach (var pair in pluginLoader.ResolveSerializableTypes())
				{
					if (types.TryGetValue(pair.Key, out var existingType))
					{
						Log.WarnFormat("There are at least two types ({0} and {1}) which have the same name '{2}', ignoring the latter!",
						               existingType, pair.Value, pair.Key);
					}
					else
					{
						types.Add(pair.Key, pair.Value);
					}
				}
			}
			return types;
		}

		public IReadOnlyList<T> LoadAllOfType<T>() where T : class, IPlugin
		{
			return _pluginLoaders.SelectMany(x => x.LoadAllOfType<T>()).ToList();
		}

		public IReadOnlyList<IPluginWithDescription<T>> LoadAllOfTypeWithDescription<T>() where T : class, IPlugin
		{
			return _pluginLoaders.SelectMany(x => x.LoadAllOfTypeWithDescription<T>()).ToList();
		}

		#endregion
	}
}