using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Responsible for creating and maintaining and scheduling analyses of data sources.
	/// </summary>
	public sealed class AnalysisEngine
		: IAnalysisEngine
			, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly List<IDataSourceAnalysisHandle> _analyses;
		private readonly Dictionary<LogAnalyserFactoryId, ILogAnalyserFactory> _factoriesById;
		private readonly ITaskScheduler _scheduler;
		private readonly object _syncRoot;

		public AnalysisEngine(ITaskScheduler scheduler)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));

			_scheduler = scheduler;
			_analyses = new List<IDataSourceAnalysisHandle>();
			_syncRoot = new object();
			_factoriesById = new Dictionary<LogAnalyserFactoryId, ILogAnalyserFactory>();
		}

		public IDataSourceAnalysisHandle CreateAnalysis(ILogFile logFile, DataSourceAnalysisConfiguration configuration, IDataSourceAnalysisListener listener)
		{
			if (logFile == null)
				throw new ArgumentNullException(nameof(logFile));
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));
			if (listener == null)
				throw new ArgumentNullException(nameof(listener));

			lock (_syncRoot)
			{
				var analysis = CreatAnalysisFor(configuration.AnalyserId, logFile, configuration.Configuration, listener);
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

		public void RegisterFactory(ILogAnalyserFactory factory)
		{
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			lock (_syncRoot)
			{
				var id = factory.Id;
				if (_factoriesById.ContainsKey(id))
					throw new ArgumentException(string.Format("There already exists a factory of id '{0}'", id));

				_factoriesById.Add(id, factory);
			}
		}

		private IDataSourceAnalysisHandle CreatAnalysisFor(LogAnalyserFactoryId id,
			ILogFile logFile,
			ILogAnalyserConfiguration configuration,
			IDataSourceAnalysisListener listener)
		{
			IDataSourceAnalysisHandle analysis;
			ILogAnalyserFactory factory;
			if (_factoriesById.TryGetValue(id, out factory))
			{
				analysis = new DataSourceAnalysis(_scheduler, logFile, factory, configuration, listener);
			}
			else
			{
				Log.ErrorFormat("Unable to find factory '{0}', analysis will be skipped", id);
				analysis = new DummyAnalysis();
			}

			return analysis;
		}

		private sealed class DummyAnalysis
			: IDataSourceAnalysisHandle
		{
			public void Start()
			{}
		}
	}
}