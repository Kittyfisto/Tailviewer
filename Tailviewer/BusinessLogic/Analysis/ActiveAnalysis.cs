using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Analysis;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.BusinessLogic.Analysis
{
	public sealed class ActiveAnalysis
		: IAnalysis
			, IDisposable
	{
		private readonly AnalysisTemplate _template;
		private readonly Dictionary<DataSourceAnalyser, AnalyserTemplate> _analysers;
		private readonly ILogAnalyserEngine _logAnalyserEngine;
		private readonly LogFileProxy _logFile;
		private readonly List<ILogFile> _logFiles;
		private readonly TimeSpan _maximumWaitTime;
		private readonly object _syncRoot;
		private readonly ITaskScheduler _taskScheduler;
		private readonly AnalysisId _id;

		public ActiveAnalysis(AnalysisTemplate template,
			ITaskScheduler taskScheduler,
			ILogAnalyserEngine logAnalyserEngine,
			TimeSpan maximumWaitTime)
		{
			if (template == null)
				throw new ArgumentNullException(nameof(template));
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));
			if (logAnalyserEngine == null)
				throw new ArgumentNullException(nameof(logAnalyserEngine));

			_id = AnalysisId.CreateNew();
			_template = template;
			_taskScheduler = taskScheduler;
			_maximumWaitTime = maximumWaitTime;
			_logFiles = new List<ILogFile>();
			_logFile = new LogFileProxy(taskScheduler, maximumWaitTime);
			_logAnalyserEngine = logAnalyserEngine;
			_analysers = new Dictionary<DataSourceAnalyser, AnalyserTemplate>();
			_syncRoot = new object();
		}

		public AnalysisTemplate Template => _template;

		public IEnumerable<IDataSourceAnalyser> Analysers
		{
			get
			{
				lock (_syncRoot)
				{
					return _analysers.Keys.ToList();
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

					foreach (var analyser in _analysers.Keys)
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
		/// <param name="factoryId"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		public IDataSourceAnalyser Add(LogAnalyserFactoryId factoryId, ILogAnalyserConfiguration configuration)
		{
			var template = new AnalyserTemplate
			{
				Id = AnalyserId.CreateNew(),
				FactoryId = factoryId,
				Configuration = configuration
			};

			var analyser = new DataSourceAnalyser(template, _logFile, _logAnalyserEngine);
			try
			{
				analyser.Configuration = configuration;
				lock (_syncRoot)
				{
					_analysers.Add(analyser, template);
					_template.Add(template);
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
				AnalyserTemplate template;
				if (tmp != null && _analysers.TryGetValue(tmp, out template))
				{
					_template.Remove(template);
					_analysers.Remove(tmp);

					tmp?.Dispose();
				}
			}
		}

		public void Dispose()
		{
			lock (_syncRoot)
			{
				foreach (var analyser in _analysers.Keys)
					analyser.Dispose();
				_analysers.Clear();

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
		public AnalysisSnapshot CreateSnapshot()
		{
			lock (_syncRoot)
			{
				var analysers = new List<DataSourceAnalyserSnapshot>(_analysers.Count);
				foreach (var analyser in _analysers.Keys)
					analysers.Add(analyser.CreateSnapshot());
				return new AnalysisSnapshot(Progress, analysers);
			}
		}
	}
}