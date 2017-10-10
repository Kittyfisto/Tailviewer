using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using log4net;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.Bookmarks;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.DataSourceTree;
using Tailviewer.Ui.Controls.QuickFilter;
using Tailviewer.Ui.Controls.SidePanel;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public sealed class LogViewMainPanelViewModel
		: AbstractMainPanelViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ISidePanelViewModel[] _sidePanels;
		private readonly BookmarksViewModel _bookmarks;

		private readonly DataSourcesViewModel _dataSources;
		private readonly QuickFiltersSidePanelViewModel _quickFilters;

		private LogViewerViewModel _currentDataSourceLogView;
		private readonly IActionCenter _actionCenter;
		private readonly IApplicationSettings _applicationSettings;
		private string _windowTitle;
		private string _windowTitleSuffix;

		public LogViewMainPanelViewModel(IActionCenter actionCenter,
			IDataSources dataSources,
			IQuickFilters quickFilters,
			IApplicationSettings applicationSettings)
			: base(applicationSettings)
		{
			if (actionCenter == null)
				throw new ArgumentNullException(nameof(actionCenter));

			_actionCenter = actionCenter;
			_applicationSettings = applicationSettings;

			_dataSources = new DataSourcesViewModel(applicationSettings, dataSources);
			_dataSources.PropertyChanged += DataSourcesOnPropertyChanged;
			_quickFilters = new QuickFiltersSidePanelViewModel(applicationSettings, quickFilters);
			_quickFilters.OnFiltersChanged += OnFiltersChanged;

			_bookmarks = new BookmarksViewModel(dataSources, OnNavigateToBookmark);

			_sidePanels = new ISidePanelViewModel[]
			{
				_dataSources,
				_quickFilters,
				_bookmarks
			};

			SelectedSidePanel = _sidePanels.FirstOrDefault(x => x.Id == _applicationSettings.MainWindow?.SelectedSidePanel);

			PropertyChanged += OnPropertyChanged;
			ChangeDataSource(CurrentDataSource);
		}

		private void OnFiltersChanged()
		{
			var source = CurrentDataSource;
			if (source != null)
				source.QuickFilterChain = _quickFilters.CreateFilterChain();
		}

		public bool CanBeDragged(IDataSourceViewModel source)
		{
			return _dataSources.CanBeDragged(source);
		}

		public bool CanBeDropped(IDataSourceViewModel source,
			IDataSourceViewModel dest,
			DataSourceDropType dropType,
			out IDataSourceViewModel finalDest)
		{
			return _dataSources.CanBeDropped(source, dest, dropType, out finalDest);
		}

		public void OnDropped(IDataSourceViewModel source,
			IDataSourceViewModel dest,
			DataSourceDropType dropType)
		{
			_dataSources.OnDropped(source, dest, dropType);
		}

		private void DataSourcesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(DataSourcesViewModel.SelectedItem):
					ChangeDataSource(_dataSources.SelectedItem);
					EmitPropertyChanged(nameof(CurrentDataSource));
					break;
			}
		}

		private void ChangeDataSource(IDataSourceViewModel value)
		{
			if (value != null)
				value.QuickFilterChain = _quickFilters.CreateFilterChain();

			_dataSources.SelectedItem = value;
			_quickFilters.CurrentDataSource = value;
			_bookmarks.CurrentDataSource = value?.DataSource;
			OpenFile(value);
		}

		private void OnNavigateToBookmark(Bookmark bookmark)
		{
			var dataSourceViewModel = _dataSources.DataSources.FirstOrDefault(x => x.DataSource == bookmark.DataSource);
			if (dataSourceViewModel != null)
			{
				CurrentDataSource = dataSourceViewModel;

				var index = bookmark.Index;
				var logFile = dataSourceViewModel.DataSource.FilteredLogFile;
				if (logFile != null)
				{
					var actualIndex = logFile.GetLogLineIndexOfOriginalLineIndex(index);
					if (actualIndex == LogLineIndex.Invalid)
					{
						Log.WarnFormat("Unable to find index '{0}' of {1}", index, dataSourceViewModel);
						return;
					}

					index = actualIndex;
				}

				dataSourceViewModel.SelectedLogLines = new HashSet<LogLineIndex> { index };
				dataSourceViewModel.RequestBringIntoView(index);
			}
		}

		public string WindowTitle
		{
			get { return _windowTitle; }
			set
			{
				if (value == _windowTitle)
					return;

				_windowTitle = value;
				EmitPropertyChanged();
			}
		}

		public string WindowTitleSuffix
		{
			get { return _windowTitleSuffix; }
			set
			{
				if (value == _windowTitleSuffix)
					return;

				_windowTitleSuffix = value;
				EmitPropertyChanged();
			}
		}

		public IDataSourceViewModel CurrentDataSource
		{
			get { return _dataSources.SelectedItem; }
			set
			{
				var before = _dataSources.SelectedItem;
				_dataSources.SelectedItem = value;

				if (before != value)
					EmitPropertyChanged();

				if (value != null)
				{
					WindowTitle = string.Format("{0} - {1}", Constants.MainWindowTitle, value.DisplayName);
					WindowTitleSuffix = value.DataSourceOrigin;
				}
				else
				{
					WindowTitle = Constants.MainWindowTitle;
					WindowTitleSuffix = null;
				}
			}
		}

		public IDataSourceViewModel OpenFile(string file)
		{
			IDataSourceViewModel dataSource = _dataSources.GetOrAdd(file);
			OpenFile(dataSource);
			return dataSource;
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case nameof(CurrentDataSource):
					OpenFile(CurrentDataSource);
					break;
			}
		}

		private void OpenFile(IDataSourceViewModel dataSource)
		{
			if (dataSource != null)
			{
				CurrentDataSource = dataSource;
				CurrentDataSourceLogView = new LogViewerViewModel(
					dataSource,
					_actionCenter,
					_applicationSettings);
			}
			else
			{
				CurrentDataSource = null;
				CurrentDataSourceLogView = null;
			}
		}

		public LogViewerViewModel CurrentDataSourceLogView
		{
			get { return _currentDataSourceLogView; }
			set
			{
				if (_currentDataSourceLogView == value)
					return;

				_currentDataSourceLogView = value;
				EmitPropertyChanged();
			}
		}

		public override IEnumerable<ISidePanelViewModel> SidePanels => _sidePanels;

		public IEnumerable<IDataSourceViewModel> RecentFiles => _dataSources.Observable;

		public override void Update()
		{
			CurrentDataSourceLogView?.Update();
			_dataSources.Update();
			_bookmarks.Update();
		}

		public QuickFilterViewModel AddQuickFilter()
		{
			return _quickFilters.AddQuickFilter();
		}

		public void ShowQuickFilters()
		{
			SelectedSidePanel = _quickFilters;
		}
	}
}