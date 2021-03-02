using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.Core;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	public sealed class MergedDataSource
		: AbstractDataSource
		, IMergedDataSource
	{
		/// <summary>
		///    The list of data sources.
		/// </summary>
		/// <remarks>
		///    Preserving the order of data sources is incredibly important here and therefore we use a list
		///    rather than a HashSet so that we don't have rely on undocumented behavior.
		/// </remarks>
		private readonly List<IDataSource> _dataSources;
		private readonly LogSourceProxy _unfilteredLogSource;
		private MergedLogSource _logSource;

		public MergedDataSource(ITaskScheduler taskScheduler, DataSource settings)
			: this(taskScheduler, settings, TimeSpan.FromMilliseconds(value: 10))
		{
		}

		public MergedDataSource(ITaskScheduler taskScheduler, DataSource settings, TimeSpan maximumWaitTime)
			: base(taskScheduler, settings, maximumWaitTime)
		{
			_dataSources = new List<IDataSource>();
			_unfilteredLogSource = new LogSourceProxy(taskScheduler, TimeSpan.Zero);
			OriginalSources = new IDataSource[0];
			UpdateUnfilteredLogFile();
			OnUnfilteredLogFileChanged();
		}

		public int DataSourceCount => _dataSources.Count;

		public IReadOnlyList<IDataSource> OriginalSources { get; private set; }

		public void SetExcluded(IDataSource dataSource, bool isExcluded)
		{
			if (isExcluded)
			{
				Settings.ExcludedDataSources.Add(dataSource.Id);
			}
			else
			{
				Settings.ExcludedDataSources.Remove(dataSource.Id);
			}

			UpdateUnfilteredLogFile();
		}

		public bool IsExcluded(IDataSource dataSource)
		{
			return Settings.ExcludedDataSources.Contains(dataSource.Id);
		}

		public override IPluginDescription TranslationPlugin => null;

		public override ILogSource OriginalLogSource
		{
			get { throw new NotImplementedException(); }
		}

		public override ILogSource UnfilteredLogSource => _unfilteredLogSource;

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

			if (!_dataSources.Contains(dataSource))
			{
				_dataSources.Add(dataSource);
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

			Settings.ExcludedDataSources.Remove(dataSource.Id);
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
				if (!_dataSources.Contains(dataSource))
				{
					_dataSources.Add(dataSource);
					dataSource.Settings.ParentId = Settings.Id;
				}
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
			_unfilteredLogSource.Dispose();
			_logSource?.Dispose();
		}

		protected override void OnSingleLineChanged()
		{
			UpdateUnfilteredLogFile();
		}

		private void UpdateUnfilteredLogFile()
		{
			_logSource?.Dispose();

			OriginalSources = _dataSources.ToList();

			var logFiles = OriginalSources.Select(dataSource =>
			                              {
				                              // Unfortunately, due to a hack, the attribution to the original data source
				                              // will be lost if we were to not forward anything to the merged data source.
				                              if (Settings.ExcludedDataSources.Contains(dataSource.Id))
					                              return new EmptyLogSource();

				                              if (IsSingleLine)
					                              return dataSource.OriginalLogSource;

				                              return dataSource.UnfilteredLogSource;
			                              })
			                              .ToList();

			_logSource = new MergedLogSource(TaskScheduler,
			                             MaximumWaitTime,
			                             logFiles);
			_unfilteredLogSource.InnerLogSource = _logSource;
		}
	}
}