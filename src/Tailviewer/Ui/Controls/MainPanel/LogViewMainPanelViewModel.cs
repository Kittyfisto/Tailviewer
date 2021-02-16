using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.Bookmarks;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.Highlighters;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.DataSourceTree;
using Tailviewer.Ui.Controls.MainPanel.Raw.GoToLine;
using Tailviewer.Ui.Controls.MainPanel.Raw.QuickNavigation;
using Tailviewer.Ui.Controls.QuickFilter;
using Tailviewer.Ui.Controls.SidePanel;
using Tailviewer.Ui.Controls.SidePanel.Bookmarks;
using Tailviewer.Ui.Controls.SidePanel.DataSources;
using Tailviewer.Ui.Controls.SidePanel.Highlighters;
using Tailviewer.Ui.Controls.SidePanel.Issues;
using Tailviewer.Ui.Controls.SidePanel.Outline;
using Tailviewer.Ui.Controls.SidePanel.QuickFilters;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public sealed class LogViewMainPanelViewModel
		: AbstractMainPanelViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ISidePanelViewModel[] _sidePanels;
		private readonly BookmarksViewModel _bookmarks;
		private readonly OutlineSidePanelViewModel _outline;
		private readonly IssuesSidePanelViewModel _issues;

		private readonly DataSourcesViewModel _dataSources;
		private readonly QuickFiltersSidePanelViewModel _quickFilters;
		private HighlightersSidePanelViewModel _highlighters;

		private readonly IActionCenter _actionCenter;
		private readonly IApplicationSettings _applicationSettings;
		private readonly GoToLineViewModel _goToLine;
		private readonly QuickNavigationViewModel _quickNavigation;

		private LogViewerViewModel _currentDataSourceLogView;
		private string _windowTitle;
		private string _windowTitleSuffix;
		private bool _showQuickNavigation;

		public LogViewMainPanelViewModel(IServiceContainer services,
		                                 IActionCenter actionCenter,
		                                 IDataSources dataSources,
		                                 IQuickFilters quickFilters,
		                                 IHighlighters highlighters,
		                                 IApplicationSettings applicationSettings)
			: base(applicationSettings)
		{
			if (actionCenter == null)
				throw new ArgumentNullException(nameof(actionCenter));

			_applicationSettings = applicationSettings;
			_actionCenter = actionCenter;

			_dataSources = new DataSourcesViewModel(applicationSettings, dataSources, _actionCenter);
			_dataSources.PropertyChanged += DataSourcesOnPropertyChanged;
			_quickFilters = new QuickFiltersSidePanelViewModel(applicationSettings, quickFilters);
			_quickFilters.OnFiltersChanged += OnFiltersChanged;

			_highlighters = new HighlightersSidePanelViewModel(highlighters);

			_goToLine = new GoToLineViewModel();
			_goToLine.LineNumberChosen += GoToLineOnLineNumberChosen;

			_quickNavigation = new QuickNavigationViewModel(dataSources);
			_quickNavigation.DataSourceChosen += QuickNavigationOnDataSourceChosen;

			_bookmarks = new BookmarksViewModel(dataSources, OnNavigateToBookmark);
			_outline = new OutlineSidePanelViewModel(services);
			_issues = new IssuesSidePanelViewModel(services);

			_sidePanels = new ISidePanelViewModel[]
			{
				_dataSources,
				_quickFilters,
				//_highlighters,
				_bookmarks,
				_outline,
				_issues
			};

			SelectedSidePanel = _sidePanels.FirstOrDefault(x => x.Id == _applicationSettings.MainWindow?.SelectedSidePanel);

			PropertyChanged += OnPropertyChanged;
			ChangeDataSource(CurrentDataSource);
		}

		private void GoToLineOnLineNumberChosen(LogLineIndex logLineIndex)
		{
			Log.DebugFormat("Going to line {0}", logLineIndex);
			var dataSourceViewModel = _currentDataSourceLogView.DataSource;
			var dataSource = dataSourceViewModel.DataSource;
			var logFile = dataSource.FilteredLogSource;
			var originalIndex = logFile.GetLogLineIndexOfOriginalLineIndex(logLineIndex);

			dataSourceViewModel.SelectedLogLines = new HashSet<LogLineIndex> { originalIndex };
			dataSourceViewModel.RequestBringIntoView(originalIndex);
		}

		private void QuickNavigationOnDataSourceChosen(IDataSource dataSource)
		{
			var viewModel = _dataSources.DataSources.FirstOrDefault(x => x.DataSource == dataSource);
			if (viewModel != null)
			{
				Log.DebugFormat("Navigating to '{0}'", viewModel);
				_dataSources.SelectedItem = viewModel;
			}
			else
			{
				Log.WarnFormat("Unable to navigate to data source '{0}': Can't find it!", dataSource);
			}
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
			_outline.CurrentDataSource = value?.DataSource;
			_issues.CurrentDataSource = value?.DataSource;
			OpenFile(value);
		}

		private void OnNavigateToBookmark(Bookmark bookmark)
		{
			var dataSourceViewModel = _dataSources.DataSources.FirstOrDefault(x => x.DataSource == bookmark.DataSource);
			if (dataSourceViewModel != null)
			{
				CurrentDataSource = dataSourceViewModel;

				var index = bookmark.Index;
				var logFile = dataSourceViewModel.DataSource.FilteredLogSource;
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

		public bool RequestBringIntoView(LogLineIndex index)
		{
			return RequestBringIntoView(CurrentDataSource, index);
		}

		public bool RequestBringIntoView(DataSourceId dataSource, LogLineIndex index)
		{
			var dataSourceViewModel = _dataSources.DataSources.FirstOrDefault(x => x.DataSource.Id == dataSource);
			return RequestBringIntoView(dataSourceViewModel, index);
		}

		private static bool RequestBringIntoView(IDataSourceViewModel dataSourceViewModel, LogLineIndex index)
		{
			if (dataSourceViewModel == null)
				return false;

			var logFile = dataSourceViewModel.DataSource.FilteredLogSource;
			if (logFile != null)
			{
				var actualIndex = logFile.GetLogLineIndexOfOriginalLineIndex(index);
				if (actualIndex == LogLineIndex.Invalid)
				{
					Log.WarnFormat("Unable to find index '{0}' of {1}", index, dataSourceViewModel);
					return false;
				}

				index = actualIndex;
			}
			dataSourceViewModel.SelectedLogLines = new HashSet<LogLineIndex> {index};
			dataSourceViewModel.RequestBringIntoView(index);
			return true;
		}

		public GoToLineViewModel GoToLine => _goToLine;
		public QuickNavigationViewModel QuickNavigation => _quickNavigation;

		public bool ShowQuickNavigation
		{
			get { return _showQuickNavigation; }
			set
			{
				if (value == _showQuickNavigation)
					return;

				_showQuickNavigation = value;
				EmitPropertyChanged();

				if (!value)
				{
					_quickNavigation.SearchTerm = null;
				}
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
				var old = _dataSources.SelectedItem;
				_dataSources.SelectedItem = value;

				if (old != null)
				{
					old.PropertyChanged -= CurrentDataSourceOnPropertyChanged;
				}

				if (value != null)
				{
					value.PropertyChanged += CurrentDataSourceOnPropertyChanged;
				}

				if (old != value)
					EmitPropertyChanged();

				UpdateWindowTitle(value);
			}
		}

		private void CurrentDataSourceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			UpdateWindowTitle(CurrentDataSource);
		}

		private void UpdateWindowTitle(IDataSourceViewModel value)
		{
			if (value != null)
			{
				WindowTitle = string.Format("{0} - {1}", Constants.MainWindowTitle, value.DisplayName);

				var child = value as ISingleDataSourceViewModel;
				var parentId = value.DataSource.ParentId;
				var parent = _dataSources.TryGet(parentId);
				if (child != null && parent != null)
				{
					WindowTitleSuffix = string.Format("{0} -> [{1}] {2}", parent.DisplayName,
					                                  child.CharacterCode,
					                                  child.DataSourceOrigin);
				}
				else
				{
					WindowTitleSuffix = value.DataSourceOrigin;
				}
			}
			else
			{
				WindowTitle = Constants.MainWindowTitle;
				WindowTitleSuffix = null;
			}
		}

		public IDataSourceViewModel GetOrAddPath(string path)
		{
			IDataSourceViewModel dataSource;
			if (Directory.Exists(path))
			{
				dataSource = _dataSources.GetOrAddFolder(path);
			}
			else
			{
				dataSource = _dataSources.GetOrAddFile(path);
			}

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

		public ILogViewerSettings Settings => _applicationSettings.LogViewer;

		public override void Update()
		{
			CurrentDataSourceLogView?.Update();
			_dataSources.Update();
			_bookmarks.Update();
			_outline.Update();
			_issues.Update();
		}

		public QuickFilterViewModel AddQuickFilter()
		{
			return _quickFilters.AddQuickFilter();
		}

		public void ShowQuickFilters()
		{
			SelectedSidePanel = _quickFilters;
		}

		public bool AddBookmark()
		{
			if (!_bookmarks.AddBookmarkCommand.CanExecute(null))
				return false;

			_bookmarks.AddBookmarkCommand.Execute(null);
			return true;
		}

		public void ShowBookmarks()
		{
			SelectedSidePanel = _bookmarks;
		}

		public void GoToPreviousDataSource()
		{
			var dataSources = _dataSources.DataSources;
			if (dataSources.Count == 0)
				return;

			var idx = dataSources.IndexOf(_dataSources.SelectedItem);
			if (idx == -1)
				return;

			var nextIndex = idx - 1;
			if (nextIndex < 0)
				nextIndex = dataSources.Count - 1;

			var next = dataSources[nextIndex];
			_dataSources.SelectedItem = next;
		}

		public void GoToNextDataSource()
		{
			var dataSources = _dataSources.DataSources;
			if (dataSources.Count == 0)
				return;

			var idx = dataSources.IndexOf(_dataSources.SelectedItem);
			if (idx == -1)
				return;

			var nextIndex = idx + 1;
			if (nextIndex >= dataSources.Count)
				nextIndex = 0;

			var next = dataSources[nextIndex];
			_dataSources.SelectedItem = next;
		}
	}
}