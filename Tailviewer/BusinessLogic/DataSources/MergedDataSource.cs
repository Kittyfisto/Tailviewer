using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	internal sealed class MergedDataSource
		: AbstractDataSource
	{
		private readonly HashSet<IDataSource> _dataSources;
		private readonly TimeSpan _maximumWaitTime;
		private readonly ITaskScheduler _taskScheduler;
		private MergedLogFile _unfilteredLogFile;

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
			UpdateLogFile();
		}

		public int DataSourceCount
		{
			get { return _dataSources.Count; }
		}

		public IEnumerable<IDataSource> DataSources
		{
			get { return _dataSources; }
		}

		public override ILogFile UnfilteredLogFile
		{
			get { return _unfilteredLogFile; }
		}

		public void Add(IDataSource dataSource)
		{
			if (dataSource.ParentId != Guid.Empty && dataSource.ParentId != Id)
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

			dataSource.Settings.ParentId = Guid.Empty;
			UpdateLogFile();
		}

		private void UpdateLogFile()
		{
			_unfilteredLogFile?.Dispose();

			_unfilteredLogFile = new MergedLogFile(_taskScheduler,
			                                       _maximumWaitTime,
			                                       _dataSources.Select(x => x.UnfilteredLogFile));
			OnUnfilteredLogFileChanged();
		}
	}
}