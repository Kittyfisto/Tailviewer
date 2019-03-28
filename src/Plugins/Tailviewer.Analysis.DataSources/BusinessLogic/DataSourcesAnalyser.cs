using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Analysis.DataSources.BusinessLogic
{
	public sealed class DataSourcesAnalyser
		: IDataSourceAnalyser
		, ILogFileListener
	{
		private readonly object _syncRoot;
		private readonly AnalyserId _id;
		private readonly DataSourcesResult _result;
		private readonly Dictionary<ILogFile, DataSourceResult> _logFiles;
		private readonly TimeSpan _maximumWaitTime;

		public DataSourcesAnalyser(AnalyserId id, TimeSpan maximumWaitTime)
		{
			_syncRoot = new object();
			_id = id;
			_maximumWaitTime = maximumWaitTime;
			_result = new DataSourcesResult();
			_logFiles = new Dictionary<ILogFile, DataSourceResult>();
		}

		public ILogAnalysisResult Result => _result;

		public bool IsFrozen => false;

		public ILogAnalyserConfiguration Configuration { get; set; }

		public void OnLogFileAdded(DataSourceId id, ILogFile logFile)
		{
			lock (_syncRoot)
			{
				var dataSource = new DataSourceResult
				{
					Id = id,
					Name = logFile.GetValue(LogFileProperties.Name),
					SizeInBytes = logFile.GetValue(LogFileProperties.Size)?.Bytes
				};
				_logFiles.Add(logFile, dataSource);
				_result.DataSources.Add(dataSource);
			}

			// We do not control what IlogFile.AddListener does so it's outside of the lock
			logFile.AddListener(this, _maximumWaitTime, 10000);
		}

		public void OnLogFileRemoved(DataSourceId id, ILogFile logFile)
		{
			lock (_syncRoot)
			{
				if (!_logFiles.TryGetValue(logFile, out var dataSource))
					return;

				_logFiles.Remove(logFile);
				_result.DataSources.Remove(dataSource);
			}

			// We do not control what IlogFile.AddListener does so it's outside of the lock
			logFile.RemoveListener(this);
		}

		public AnalyserId Id => _id;

		public AnalyserPluginId AnalyserPluginId => DataSourcesAnalyserPlugin.Id;

		public Percentage Progress => Percentage.HundredPercent;

		#region Implementation of IDisposable

		public void Dispose()
		{
			List<ILogFile> logFiles;
			lock (_syncRoot)
			{
				logFiles = _logFiles.Keys.ToList();
				_logFiles.Clear();
			}

			foreach (var logFile in logFiles)
			{
				logFile.RemoveListener(this);
			}
		}

		#endregion

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			// we don't know what ILogFile.GetValue() does so it stays outside the lock!
			var created = logFile.GetValue(LogFileProperties.Created);
			var lastModified = logFile.GetValue(LogFileProperties.LastModified);
			var sizeInBytes = logFile.GetValue(LogFileProperties.Size)?.Bytes;

			lock (_syncRoot)
			{
				if (_logFiles.TryGetValue(logFile, out var result))
				{
					result.Created = created;
					result.LastModified = lastModified;
					result.SizeInBytes = sizeInBytes;
				}
			}
		}
	}
}
