using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins.Description;

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

		public IPluginStatus GetStatus(PluginId id)
		{
			throw new NotImplementedException();
		}

		public abstract IReadOnlyDictionary<string, Type> ResolveSerializableTypes();

		public abstract T Load<T>(IPluginDescription plugin,
		                          IPluginImplementationDescription implementation) where T : class, IPlugin;

		/// <inheritdoc />
		public IReadOnlyList<T> LoadAllOfType<T>() where T : class, IPlugin
		{
			var ret = new List<T>();
			foreach (var pluginDescription in Plugins)
			{
				foreach (var description in pluginDescription.PluginImplementations)
				{
					if (description.InterfaceType == typeof(T))
					{
						try
						{
							var plugin = Load<T>(pluginDescription, description);
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
				}
			}
			return ret;
		}

		public IReadOnlyList<IPluginWithDescription<T>> LoadAllOfTypeWithDescription<T>() where T : class, IPlugin
		{
			throw new NotImplementedException();
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