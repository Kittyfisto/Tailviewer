using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Responsible for creating and maintaining and scheduling <see cref="ILogAnalyser" />s.
	///     Each analyser is wrapped in a <see cref="LogAnalyserProxy" /> which is primarily responsible
	///     for handling <see cref="ILogAnalyser" /> faults.
	/// </summary>
	public sealed class LogAnalyserEngine
		: ILogAnalyserEngine
			, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly List<IDataSourceAnalysisHandle> _analyses;
		private readonly Dictionary<LogAnalyserFactoryId, ILogAnalyserPlugin> _factoriesById;
		private readonly ITaskScheduler _scheduler;
		private readonly object _syncRoot;

		public LogAnalyserEngine(ITaskScheduler scheduler)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));

			_scheduler = scheduler;
			_analyses = new List<IDataSourceAnalysisHandle>();
			_syncRoot = new object();
			_factoriesById = new Dictionary<LogAnalyserFactoryId, ILogAnalyserPlugin>();
		}

		public void Dispose()
		{
			lock (_syncRoot)
			{
				foreach (var analysis in _analyses)
				{
					var disposable = analysis as IDisposable;
					disposable?.Dispose();
				}
				_analyses.Clear();
			}
		}

		public IDataSourceAnalysisHandle CreateAnalysis(ILogFile logFile, DataSourceAnalysisConfiguration configuration,
			IDataSourceAnalysisListener listener)
		{
			if (logFile == null)
				throw new ArgumentNullException(nameof(logFile));
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));
			if (listener == null)
				throw new ArgumentNullException(nameof(listener));

			lock (_syncRoot)
			{
				var analysis = CreatAnalysisFor(configuration.FactoryId, logFile, configuration.Configuration, listener);
				_analyses.Add(analysis);
				// DO NOT ANYTHING IN BETWEEN ADD AND RETURN
				return analysis;
			}
		}

		public bool RemoveAnalysis(IDataSourceAnalysisHandle analysis)
		{
			lock (_syncRoot)
			{
				if (_analyses.Remove(analysis))
				{
					var disposable = analysis as IDisposable;
					disposable?.Dispose();
					return true;
				}

				return false;
			}
		}

		public void RegisterFactory(ILogAnalyserPlugin plugin)
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

		private IDataSourceAnalysisHandle CreatAnalysisFor(LogAnalyserFactoryId id,
			ILogFile logFile,
			ILogAnalyserConfiguration configuration,
			IDataSourceAnalysisListener listener)
		{
			IDataSourceAnalysisHandle analysis;
			ILogAnalyserPlugin plugin;
			if (_factoriesById.TryGetValue(id, out plugin))
			{
				analysis = new LogAnalyserProxy(_scheduler, logFile, plugin, configuration, listener);
			}
			else
			{
				Log.ErrorFormat("Unable to find plugin '{0}', analysis will be skipped", id);
				analysis = new DummyAnalysis();
			}

			return analysis;
		}

		private sealed class DummyAnalysis
			: IDataSourceAnalysisHandle
		{
			public void Start()
			{
			}
		}
	}
}