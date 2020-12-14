using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tailviewer.Archiver.Plugins.Description;
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
		private readonly HashSet<IDataSource> _excludedDataSources;
		private MergedLogFile _unfilteredLogFile;

		public MergedDataSource(ITaskScheduler taskScheduler, DataSource settings)
			: this(taskScheduler, settings, TimeSpan.FromMilliseconds(value: 10))
		{
		}

		public MergedDataSource(ITaskScheduler taskScheduler, DataSource settings, TimeSpan maximumWaitTime)
			: base(taskScheduler, settings, maximumWaitTime)
		{
			_dataSources = new HashSet<IDataSource>();
			_excludedDataSources = new HashSet<IDataSource>();
			OriginalSources = new IDataSource[0];
			UpdateUnfilteredLogFile();
		}

		public int DataSourceCount => _dataSources.Count;

		public IReadOnlyList<IDataSource> OriginalSources { get; private set; }
		public void SetExcluded(IDataSource dataSource, bool isExcluded)
		{
			if (isExcluded)
			{
				_excludedDataSources.Add(dataSource);
			}
			else
			{
				_excludedDataSources.Remove(dataSource);
			}

			UpdateUnfilteredLogFile();
		}

		public override IPluginDescription TranslationPlugin => null;

		public override ILogFile OriginalLogFile
		{
			get { throw new NotImplementedException(); }
		}

		public override ILogFile UnfilteredLogFile => _unfilteredLogFile;

		public bool IsExpanded
		{
			get { return Settings.IsExpanded; }
			set { Settings.IsExpanded = value; }
		}

		public DataSourceDisplayMode DisplayMode
		{
			get { return Settings.MergedDataSourceDisplayMode; }
			set { Settings.MergedDataSourceDisplayMode = value; }
		}

		public string DisplayName
		{
			get { return Settings.DisplayName; }
			set { Settings.DisplayName = value; }
		}

		public void Add(IDataSource dataSource)
		{
			if (dataSource.ParentId != DataSourceId.Empty && dataSource.ParentId != Id)
				throw new ArgumentException("This data source already belongs to a different parent");

			if (_dataSources.Add(dataSource))
			{
				dataSource.Settings.ParentId = Settings.Id;
				UpdateUnfilteredLogFile();
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
			UpdateUnfilteredLogFile();
		}

		public void SetDataSources(IReadOnlyList<IDataSource> dataSources)
		{
			foreach (var dataSource in dataSources)
			{
				if (dataSource.ParentId != DataSourceId.Empty && dataSource.ParentId != Id)
					throw new ArgumentException("This data source already belongs to a different parent");
			}

			foreach (var dataSource in dataSources)
			{
				_dataSources.Add(dataSource);
				dataSource.Settings.ParentId = Settings.Id;
			}

			foreach (var dataSource in _dataSources.ToList())
			{
				if (!dataSources.Contains(dataSource))
				{
					_dataSources.Remove(dataSource);
					dataSource.Settings.ParentId = DataSourceId.Empty;
				}
			}

			UpdateUnfilteredLogFile();
		}

		protected override void DisposeAdditional()
		{
			_unfilteredLogFile?.Dispose();
		}

		protected override void OnSingleLineChanged()
		{
			UpdateUnfilteredLogFile();
		}

		private void UpdateUnfilteredLogFile()
		{
			_unfilteredLogFile?.Dispose();

			OriginalSources = _dataSources.ToList();

			var logFiles = OriginalSources.Select(dataSource =>
			                              {
				                              // Unfortunately, due to a hack, the attribution to the original data source
				                              // will be lost if we were to not forward anything to the merged data source.
				                              if (_excludedDataSources.Contains(dataSource))
					                              return new EmptyLogFile();

				                              if (IsSingleLine)
					                              return dataSource.OriginalLogFile;

				                              return dataSource.UnfilteredLogFile;
			                              })
			                              .ToList();

			_unfilteredLogFile = new MergedLogFile(TaskScheduler,
			                                       MaximumWaitTime,
			                                       logFiles);
			OnUnfilteredLogFileChanged();
		}
	}
}