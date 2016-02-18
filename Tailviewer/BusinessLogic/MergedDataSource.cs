using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic
{
	internal sealed class MergedDataSource
		: AbstractDataSource
	{
		private readonly HashSet<IDataSource> _dataSources;
		private readonly TimeSpan _maximumWaitTime;
		private MergedLogFile _logFile;

		public MergedDataSource(DataSource settings)
			: this(settings, TimeSpan.FromMilliseconds(100))
		{}

		public MergedDataSource(DataSource settings, TimeSpan maximumWaitTime)
			: base(settings, maximumWaitTime)
		{
			_maximumWaitTime = maximumWaitTime;
			_dataSources = new HashSet<IDataSource>();
			UpdateLogFile();
		}

		public int Count
		{
			get { return _dataSources.Count; }
		}

		public IEnumerable<IDataSource> DataSources
		{
			get { return _dataSources; }
		}

		public override ILogFile LogFile
		{
			get { return _logFile; }
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
				throw new ArgumentException("This data source belongs to a different parent and thus cannot be removed from this one");

			if (!_dataSources.Remove(dataSource))
				throw new ArgumentException("dataSource");

			dataSource.Settings.ParentId = Guid.Empty;
			UpdateLogFile();
		}

		private void UpdateLogFile()
		{
			if (_logFile != null)
			{
				_logFile.Dispose();
			}

			_logFile = new MergedLogFile(_dataSources.Select(x => x.LogFile));
			_logFile.Start(_maximumWaitTime);
			CreateFilteredLogFile();
		}
	}
}