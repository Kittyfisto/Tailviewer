using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	///     A <see cref="IPluginLoader" /> implementation which aggregates plugins from multiple other
	///     <see cref="IPluginLoader" /> objects.
	/// </summary>
	public sealed class AggregatedPluginLoader
		: IPluginLoader
	{
		private readonly IReadOnlyList<IPluginLoader> _pluginLoaders;

		public AggregatedPluginLoader(params IPluginLoader[] pluginLoaders)
		{
			_pluginLoaders = pluginLoaders;
		}

		#region Implementation of IPluginLoader

		public IEnumerable<IPluginDescription> Plugins => _pluginLoaders.SelectMany(x => x.Plugins).ToList();

		public IPluginStatus GetStatus(IPluginDescription description)
		{
			foreach (var pluginLoader in _pluginLoaders)
			{
				var status = pluginLoader.GetStatus(description);
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

		public IReadOnlyList<T> LoadAllOfType<T>() where T : class, IPlugin
		{
			return _pluginLoaders.SelectMany(x => x.LoadAllOfType<T>()).ToList();
		}

		#endregion
	}
}