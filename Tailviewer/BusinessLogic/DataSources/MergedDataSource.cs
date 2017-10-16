using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	public sealed class MergedDataSource
		: AbstractDataSource
		, IMergedDataSource
	{
		private readonly HashSet<IDataSource> _dataSources;
		private readonly TimeSpan _maximumWaitTime;
		private readonly ITaskScheduler _taskScheduler;
		private MergedLogFile _unfilteredLogFile;
		private IReadOnlyList<IDataSource> _orderedDataSources;

		public MergedDataSource(ITaskScheduler taskScheduler, DataSource settings)
			: this(taskScheduler, settings, TimeSpan.FromMilliseconds(10))
		{
		}

		public MergedDataSource(ITaskScheduler taskScheduler, DataSource settings, TimeSpan maximumWaitTime)
			: base(taskScheduler, settings, maximumWaitTime)
		{
			_taskScheduler = taskScheduler;
			_maximumWaitTime = maximumWaitTime;
			_dataSources = new HashSet<IDataSource>();
			_orderedDataSources = new IDataSource[0];
			UpdateLogFile();
		}

		public int DataSourceCount => _dataSources.Count;

		public IReadOnlyList<IDataSource> OriginalSources => _orderedDataSources;

		public override ILogFile UnfilteredLogFile => _unfilteredLogFile;

		public bool IsExpanded
		{
			get { return Settings.IsExpanded; }
			set { Settings.IsExpanded = value; }
		}

		public void Add(IDataSource dataSource)
		{
			if (dataSource.ParentId != DataSourceId.Empty && dataSource.ParentId != Id)
				throw new ArgumentException("This data source already belongs to a different parent");

			if (_dataSources.Add(dataSource))
			{
				dataSource.Settings.ParentId = Settings.Id;
				UpdateLogFile();
			}
		}

		public void Remove(IDataSource dataSource)
		{
			if (dataSource.ParentId != Id)
				throw new ArgumentException(
					"This data source belongs to a different parent and thus cannot be removed from this one");

			if (!_dataSources.Remove(dataSource))
				throw new ArgumentException("dataSource");

			dataSource.Settings.ParentId = DataSourceId.Empty;
			UpdateLogFile();
		}

		private void UpdateLogFile()
		{
			_unfilteredLogFile?.Dispose();

			_orderedDataSources = _dataSources.ToList();
			var logFiles = _orderedDataSources.Select(x => x.UnfilteredLogFile); //< We want to make sure that we match the order we're providing to the outside world is the same order we're forwarding to the actual MergedLogFile!!!!
			_unfilteredLogFile = new MergedLogFile(_taskScheduler,
			                                       _maximumWaitTime,
			                                       logFiles);
			OnUnfilteredLogFileChanged();
		}
	}
}