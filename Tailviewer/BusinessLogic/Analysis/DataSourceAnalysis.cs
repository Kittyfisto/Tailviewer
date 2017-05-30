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
		private readonly DataSourceAnalysisConfiguration _configuration;

		private readonly IDataSource _dataSource;
		private readonly ITaskScheduler _scheduler;
		private readonly IPeriodicTask _task;

		public DataSourceAnalysis(ITaskScheduler scheduler,
			IDataSource dataSource,
			DataSourceAnalysisConfiguration configuration)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));
			if (dataSource == null)
				throw new ArgumentNullException(nameof(dataSource));
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			_scheduler = scheduler;
			_dataSource = dataSource;
			_configuration = configuration;
			_task = _scheduler.StartPeriodic(OnUpdate, TimeSpan.FromSeconds(0.5), "");
			_analyser = TryCreateAnalyser();
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
				var obj = Activator.CreateInstance(_configuration.LogAnalyserType, BindingFlags.Default, null, new object[]
				{
					_dataSource.UnfilteredLogFile,
					_configuration.Configuration
				});
				var analyser = (ILogAnalyser) obj;
				return analyser;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while trying to create analyser '{0}': {1}",
					_configuration.LogAnalyserType,
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
					_configuration.LogAnalyserType,
					e);
			}
		}
	}
}