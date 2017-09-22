using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.BusinessLogic.Analysis
{
	public sealed class AnalyserGroup
		: IAnalyserGroup
			, IDisposable
	{
		private readonly List<DataSourceAnalyser> _analysers;
		private readonly IAnalysisEngine _analysisEngine;
		private readonly LogFileProxy _logFile;
		private readonly List<ILogFile> _logFiles;
		private readonly TimeSpan _maximumWaitTime;
		private readonly object _syncRoot;
		private readonly ITaskScheduler _taskScheduler;
		private readonly AnalysisId _id;

		public AnalyserGroup(ITaskScheduler taskScheduler,
			IAnalysisEngine analysisEngine,
			TimeSpan maximumWaitTime)
		{
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));
			if (analysisEngine == null)
				throw new ArgumentNullException(nameof(analysisEngine));

			_id = AnalysisId.CreateNew();
			_taskScheduler = taskScheduler;
			_maximumWaitTime = maximumWaitTime;
			_logFiles = new List<ILogFile>();
			_logFile = new LogFileProxy(taskScheduler, maximumWaitTime);
			_analysisEngine = analysisEngine;
			_analysers = new List<DataSourceAnalyser>();
			_syncRoot = new object();
		}

		public IEnumerable<IDataSourceAnalyser> Analysers
		{
			get
			{
				lock (_syncRoot)
				{
					return _analysers.ToList();
				}
			}
		}

		public IEnumerable<ILogFile> LogFiles
		{
			get
			{
				lock (_syncRoot)
				{
					return _logFiles.ToList();
				}
			}
		}

		public Percentage Progress
		{
			get
			{
				// TODO: Move this portion into a separate timer that updates at 10Hz or so...
				var progress = Percentage.Zero;
				lock (_syncRoot)
				{
					if (_analysers.Count == 0)
						return Percentage.HundredPercent;

					foreach (var analyser in _analysers)
					{
						var tmp = analyser.Progress;
						if (!Percentage.IsNan(tmp))
							progress += tmp;
					}

					return progress;
				}
			}
		}

		public bool IsFrozen => false;

		public AnalysisId Id => _id;

		/// <summary>
		/// </summary>
		/// <param name="analyserId"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		public IDataSourceAnalyser Add(LogAnalyserFactoryId analyserId, ILogAnalyserConfiguration configuration)
		{
			var analyser = new DataSourceAnalyser(_logFile, _analysisEngine, analyserId);
			try
			{
				analyser.Configuration = configuration;
				lock (_syncRoot)
				{
					_analysers.Add(analyser);
				}

				return analyser;
			}
			catch (Exception)
			{
				analyser.Dispose();
				throw;
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="analyser"></param>
		public void Remove(IDataSourceAnalyser analyser)
		{
			var tmp = analyser as DataSourceAnalyser;
			lock (_syncRoot)
			{
				if (_analysers.Remove(tmp))
					tmp?.Dispose();
			}
		}

		public void Dispose()
		{
			lock (_syncRoot)
			{
				foreach (var analyser in _analysers)
					analyser.Dispose();

				_logFile.Dispose();
			}
		}

		public void Add(ILogFile logFile)
		{
			lock (_syncRoot)
			{
				_logFiles.Add(logFile);
				UpdateProxy();
			}
		}

		public void Remove(ILogFile logFile)
		{
			lock (_syncRoot)
			{
				_logFiles.Remove(logFile);
				UpdateProxy();
			}
		}

		private void UpdateProxy()
		{
			var merged = new MergedLogFile(_taskScheduler,
				_maximumWaitTime,
				_logFiles);
			_logFile.InnerLogFile = merged;
		}

		/// <summary>
		///     Creates a snapshot of this group's analysers.
		/// </summary>
		/// <returns></returns>
		public AnalyserGroupSnapshot CreateSnapshot()
		{
			lock (_syncRoot)
			{
				var analysers = new List<DataSourceAnalyserSnapshot>(_analysers.Count);
				foreach (var analyser in _analysers)
					analysers.Add(analyser.CreateSnapshot());
				return new AnalyserGroupSnapshot(Progress, analysers);
			}
		}
	}
}