using System;
using System.Collections.Generic;
using System.Threading;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Responsible for creating and maintaining and scheduling analyses of data sources.
	/// </summary>
	public sealed class AnalysisEngine
		: IAnalysisEngine
			, IDisposable
	{
		private readonly List<DataSourceAnalysis> _analyses;
		private readonly ITaskScheduler _scheduler;
		private readonly object _syncRoot;

		public AnalysisEngine(ITaskScheduler scheduler)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));

			_scheduler = scheduler;
			_analyses = new List<DataSourceAnalysis>();
			_syncRoot = new object();
		}

		public IDataSourceAnalysis CreateAnalysis(IDataSource dataSource, DataSourceAnalysisConfiguration configuration)
		{
			var analysis = new DataSourceAnalysis(_scheduler, dataSource, configuration);
			lock (_syncRoot)
			{
				_analyses.Add(analysis);
			}
			return analysis;
		}

		public void RemoveAnalysis(IDataSourceAnalysis analysis)
		{
			lock (_syncRoot)
			{
				var tmp = analysis as DataSourceAnalysis;
				if (tmp != null && _analyses.Remove(tmp))
					tmp.Dispose();
			}
		}

		public void Dispose()
		{
		}
	}
}