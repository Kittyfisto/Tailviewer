using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.Core.Sources;
using Tailviewer.Plugins;

namespace Tailviewer.BusinessLogic.Sources
{
	/// <summary>
	/// 
	/// </summary>
	public class PluginLogFileFactory
		: ILogFileFactory
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IReadOnlyList<IPluginWithDescription<ICustomDataSourcePlugin>> _dataSourcePlugins;
		private readonly IServiceContainer _services;

		public PluginLogFileFactory(IServiceContainer services,
		                            IPluginWithDescription<ICustomDataSourcePlugin>[] dataSourcePlugins)
		{
			if (services == null)
				throw new ArgumentNullException(nameof(services));

			_services = services;
			_dataSourcePlugins = new List<IPluginWithDescription<ICustomDataSourcePlugin>>(dataSourcePlugins);
		}

		public PluginLogFileFactory(IServiceContainer services,
		                            IEnumerable<IPluginWithDescription<ICustomDataSourcePlugin>> dataSourcePlugins)
			: this(services,
			       dataSourcePlugins?.ToArray() ?? new IPluginWithDescription<ICustomDataSourcePlugin>[0])
		{}

		/// <inheritdoc />
		public ILogSource Open(string filePath, out IPluginDescription pluginDescription)
		{
			pluginDescription = null;
			return _services.CreateTextLogFile(filePath);
		}

		public IReadOnlyList<ICustomDataSourcePlugin> CustomDataSources
		{
			get { return _dataSourcePlugins.Select(x => x.Plugin).ToList(); }
		}

		public ILogSource CreateCustom(CustomDataSourceId id, ICustomDataSourceConfiguration configuration,
		                               out IPluginDescription pluginDescription)
		{
			var pair = _dataSourcePlugins.First(x => x.Plugin.Id == id);
			pluginDescription = pair.Description;
			var logFile = TryCreateCustomWith(pair.Plugin, configuration);
			return new NoThrowLogSource(logFile, pluginDescription.Name);
		}

		private ILogSource TryCreateCustomWith(ICustomDataSourcePlugin plugin, ICustomDataSourceConfiguration configuration)
		{
			try
			{
				var logFile = plugin.CreateLogSource(_services, configuration);
				return logFile;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught exception while trying to create custom log file: {0}", e);
				return null;
			}
		}
	}
}