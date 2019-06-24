using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Analysis;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Responsible for creating and maintaining and scheduling <see cref="IDataSourceAnalyser" />s.
	///     Each analyser is wrapped in a <see cref="DataSourceAnalyserProxy" /> which is primarily responsible
	///     for handling <see cref="IDataSourceAnalyser" /> faults.
	/// </summary>
	/// <remarks>
	///     This class offers a "high-level" interface to analysis compared to <see cref="ILogAnalyserEngine"/> and in
	///     fact uses the latter to achieve this.
	/// </remarks>
	public sealed class DataSourceAnalyserEngine
		: IDataSourceAnalyserEngine
		, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly List<IDataSourceAnalyser> _dataSourceAnalysers;
		private readonly Dictionary<AnalyserPluginId, IDataSourceAnalyserPlugin> _factoriesById;
		private readonly ILogAnalyserEngine _logAnalyserEngine;
		private readonly IServiceContainer _services;

		private readonly object _syncRoot;

		public DataSourceAnalyserEngine(IServiceContainer services, ILogAnalyserEngine logAnalyserEngine)
		{
			_services = services ?? throw new ArgumentNullException(nameof(services));
			_logAnalyserEngine = logAnalyserEngine ?? throw new ArgumentNullException(nameof(logAnalyserEngine));
			_syncRoot = new object();
			_dataSourceAnalysers = new List<IDataSourceAnalyser>();
			_factoriesById = new Dictionary<AnalyserPluginId, IDataSourceAnalyserPlugin>();

			var pluginLoader = services.TryRetrieve<IPluginLoader>();
			if (pluginLoader != null)
				foreach (var plugin in pluginLoader.LoadAllOfType<IDataSourceAnalyserPlugin>())
					RegisterFactory(plugin);
		}

		/// <inheritdoc />
		public IDataSourceAnalyser CreateAnalyser(ILogFile logFile, AnalyserTemplate template)
		{
			if (TryGetPlugin(template.AnalyserPluginId, out var plugin))
			{
				// As usual, we don't trust plugins to behave nice and therefore we wrap them in a proxy
				// which handles all exceptions thrown by the plugin.
				var analyser = new DataSourceAnalyserProxy(plugin, template.Id, _services, logFile, template.Configuration);
				Add(analyser);
				return analyser;
			}
			else
			{
				var analyser = new DataSourceAnalyser(template, logFile, _logAnalyserEngine);
				Add(analyser);
				return analyser;
			}
		}

		public void RemoveAnalyser(IDataSourceAnalyser analyser)
		{
			lock (_syncRoot)
			{
				_dataSourceAnalysers.Remove(analyser);
				analyser.Dispose();
			}
		}

		public void Dispose()
		{
			lock (_syncRoot)
			{
				foreach (var analyser in _dataSourceAnalysers) analyser.Dispose();
			}
		}

		public void RegisterFactory(IDataSourceAnalyserPlugin plugin)
		{
			if (plugin == null)
				throw new ArgumentNullException(nameof(plugin));

			lock (_syncRoot)
			{
				var id = plugin.Id;
				if (_factoriesById.ContainsKey(id))
					throw new ArgumentException(string.Format("There already exists a plugin of id '{0}'", id));

				_factoriesById.Add(id, plugin);
			}
		}

		private void Add(IDataSourceAnalyser analyser)
		{
			lock (_syncRoot)
			{
				_dataSourceAnalysers.Add(analyser);
			}
		}

		private bool TryGetPlugin(AnalyserPluginId id, out IDataSourceAnalyserPlugin plugin)
		{
			lock (_syncRoot)
			{
				return _factoriesById.TryGetValue(id, out plugin);
			}
		}
	}
}