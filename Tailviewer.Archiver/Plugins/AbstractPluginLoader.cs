using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Plugins
{
	public abstract class AbstractPluginLoader
		: IPluginLoader
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly List<IPluginDescription> _plugins;
		private readonly object _syncRoot;

		protected AbstractPluginLoader()
		{
			_plugins = new List<IPluginDescription>();
			_syncRoot = new object();
		}

		/// <inheritdoc />
		public IEnumerable<IPluginDescription> Plugins
		{
			get
			{
				lock (_syncRoot)
				{
					return _plugins.ToList();
				}
			}
		}

		/// <inheritdoc />
		public abstract T Load<T>(IPluginDescription description) where T : class, IPlugin;

		/// <inheritdoc />
		public IEnumerable<T> LoadAllOfType<T>(IEnumerable<IPluginDescription> pluginDescriptions) where T : class, IPlugin
		{
			var ret = new List<T>();
			foreach (var pluginDescription in pluginDescriptions)
			{
				if (pluginDescription.Plugins.ContainsKey(typeof(T)))
					try
					{
						var plugin = Load<T>(pluginDescription);
						ret.Add(plugin);
					}
					catch (Exception e)
					{
						Log.ErrorFormat("Unable to load plugin of interface '{0}' from '{1}': {2}",
							typeof(T),
							pluginDescription,
							e);
					}
			}
			return ret;
		}

		protected void Add(params IPluginDescription[] plugins)
		{
			lock (_syncRoot)
			{
				_plugins.AddRange(plugins);
			}
		}
	}
}