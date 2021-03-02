using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Sources
{
	/// <summary>
	/// 
	/// </summary>
	public class PluginLogSourceFactory
		: ILogSourceFactoryEx
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IReadOnlyList<IPluginWithDescription<ICustomDataSourcePlugin>> _dataSourcePlugins;
		private readonly IServiceContainer _services;

		public PluginLogSourceFactory(IServiceContainer services,
		                            IPluginWithDescription<ICustomDataSourcePlugin>[] dataSourcePlugins)
		{
			_services = services ?? throw new ArgumentNullException(nameof(services));
			_dataSourcePlugins = new List<IPluginWithDescription<ICustomDataSourcePlugin>>(dataSourcePlugins ?? new IPluginWithDescription<ICustomDataSourcePlugin>[0]);
		}

		public PluginLogSourceFactory(IServiceContainer services,
		                            IEnumerable<IPluginWithDescription<ICustomDataSourcePlugin>> dataSourcePlugins)
			: this(services,
			       dataSourcePlugins?.ToArray() ?? new IPluginWithDescription<ICustomDataSourcePlugin>[0])
		{}

		/// <inheritdoc />
		public ILogSource Open(string filePath)
		{
			return new FileLogSource(_services, filePath);
		}

		public IReadOnlyList<ICustomDataSourcePlugin> CustomDataSources
		{
			get { return _dataSourcePlugins.Select(x => x.Plugin).ToList(); }
		}

		public ILogSource CreateCustom(CustomDataSourceId id, ICustomDataSourceConfiguration configuration)
		{
			return CreateCustom(id, configuration, out _);
		}

		public ILogSource CreateCustom(CustomDataSourceId id, ICustomDataSourceConfiguration configuration,
		                               out IPluginDescription pluginDescription)
		{
			var pair = _dataSourcePlugins.First(x => x.Plugin.Id == id);
			pluginDescription = pair.Description;
			var logFile = TryCreateCustomWith(pair.Plugin, configuration);
			return new NoThrowLogSource(logFile, pluginDescription.Name);
		}

		/// <inheritdoc />
		public ILogSource CreateEventLogFile(string fileName)
		{
			return new EventLogSource(_services.Retrieve<ITaskScheduler>(), fileName);
		}

		/// <inheritdoc />
		public ILogSource CreateFilteredLogFile(TimeSpan maximumWaitTime, ILogSource source, ILogEntryFilter filter)
		{
			return new FilteredLogSource(_services.Retrieve<ITaskScheduler>(), maximumWaitTime, source,
			                             null,
			                             filter);
		}

		/// <inheritdoc />
		public ILogSourceProxy CreateLogFileProxy(TimeSpan maximumWaitTime, ILogSource source)
		{
			return new LogSourceProxy(_services.Retrieve<ITaskScheduler>(), maximumWaitTime, source);
		}

		/// <inheritdoc />
		public IMergedLogFile CreateMergedLogFile(TimeSpan maximumWaitTime, IEnumerable<ILogSource> sources)
		{
			return new MergedLogSource(_services.Retrieve<ITaskScheduler>(),
			                           maximumWaitTime,
			                           sources);
		}

		/// <inheritdoc />
		public ILogSource CreateMultiLineLogFile(TimeSpan maximumWaitTime, ILogSource source)
		{
			return new MultiLineLogSource(_services.Retrieve<ITaskScheduler>(), source, maximumWaitTime);
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