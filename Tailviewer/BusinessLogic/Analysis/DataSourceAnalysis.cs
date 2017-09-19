using System;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Represents a (user defined) analysis of a data source.
	///     Encapsules an underlying <see cref="ILogAnalyser" />, forwards its result
	///     and hides all of its (possible) failures.
	/// </summary>
	public sealed class DataSourceAnalysis
		: IDataSourceAnalysis
		, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly ILogAnalyser _analyser;
		private readonly ILogAnalyserConfiguration _configuration;

		private readonly IDataSource _dataSource;
		private readonly ITaskScheduler _scheduler;
		private readonly IPeriodicTask _task;
		private readonly ILogAnalyserFactory _factory;

		public DataSourceAnalysis(ITaskScheduler scheduler,
			IDataSource dataSource,
			ILogAnalyserFactory factory,
			ILogAnalyserConfiguration configuration)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));
			if (dataSource == null)
				throw new ArgumentNullException(nameof(dataSource));
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			_scheduler = scheduler;
			_dataSource = dataSource;
			_factory = factory;
			_configuration = configuration;
			_analyser = TryCreateAnalyser();
			_task = _scheduler.StartPeriodic(OnUpdate, TimeSpan.FromSeconds(0.5), "");
		}

		public ILogAnalysisResult Result { get; private set; }

		public void Dispose()
		{
			_scheduler.StopPeriodic(_task);
			_analyser?.Dispose();
		}

		private ILogAnalyser TryCreateAnalyser()
		{
			try
			{
				var analyser = _factory.Create(_scheduler, _dataSource.UnfilteredLogFile, _configuration);
				return analyser;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while trying to create analyser '{0}': {1}",
					_factory.Id,
					e);
				return null;
			}
		}

		private void OnUpdate()
		{
			try
			{
				Result = _analyser.Result;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while trying to fetch analysis result '{0}': {1}",
					_factory.Id,
					e);
			}
		}
	}
}