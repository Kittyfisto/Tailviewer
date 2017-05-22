using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.AutoUpdates;
using Tailviewer.BusinessLogic.Bookmarks;
using Tailviewer.Ui.Controls.ActionCenter;
using Tailviewer.Ui.Controls.DataSourceTree;
using Tailviewer.Ui.Controls.MainPanel;
using Tailviewer.Ui.Controls.SidePanel;
using ApplicationSettings = Tailviewer.Settings.ApplicationSettings;
using DataSources = Tailviewer.BusinessLogic.DataSources.DataSources;
using QuickFilters = Tailviewer.BusinessLogic.Filters.QuickFilters;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class MainWindowViewModel
		: INotifyPropertyChanged
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Dispatching

		private readonly IDispatcher _dispatcher;
		private readonly DispatcherTimer _timer;

		#endregion

		private readonly ApplicationSettings _applicationSettings;
		private readonly IActionCenter _actionCenter;

		#region ViewModels

		private readonly ActionCenterViewModel _actionCenterViewModel;
		private readonly AutoUpdateViewModel _autoUpdater;
		private readonly DataSourcesViewModel _dataSources;
		private readonly QuickFiltersViewModel _quickFilters;
		private readonly BookmarksViewModel _bookmarks;
		private readonly SettingsViewModel _settings;

		#endregion

		#region Commands

		private readonly ICommand _selectNextDataSourceCommand;
		private readonly ICommand _selectPreviousDataSourceCommand;

		#endregion

		#region Main Panel

		private readonly AnalyseMainPanelEntry _analyseEntry;
		private readonly LogViewMainPanelEntry _rawEntry;
		private readonly IMainPanelEntry[] _entries;
		private IMainPanelEntry _selectedEntry;

		#endregion

		#region Side Panel

		private readonly ISidePanelViewModel[] _sidePanels;
		private ISidePanelViewModel _selectedPanel;

		#endregion

		private bool _isLogFileOpen;
		private string _windowTitle;
		private string _windowTitleSuffix;
		private IMainPanelViewModel _selectedMainPanel;

		public MainWindowViewModel(ApplicationSettings settings,
		                           DataSources dataSources,
		                           QuickFilters quickFilters,
		                           IActionCenter actionCenter,
		                           IAutoUpdater updater,
		                           IDispatcher dispatcher)
		{
			if (dataSources == null) throw new ArgumentNullException(nameof(dataSources));
			if (quickFilters == null) throw new ArgumentNullException(nameof(quickFilters));
			if (updater == null) throw new ArgumentNullException(nameof(updater));
			if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));

			_applicationSettings = settings;
			_dataSources = new DataSourcesViewModel(settings, dataSources);
			_dataSources.PropertyChanged += DataSourcesOnPropertyChanged;
			_quickFilters = new QuickFiltersViewModel(settings, quickFilters);
			_quickFilters.OnFiltersChanged += OnQuickFiltersChanged;
			_bookmarks = new BookmarksViewModel(dataSources, OnNavigateToBookmark);
			_settings = new SettingsViewModel(settings);
			_actionCenter = actionCenter;
			_actionCenterViewModel = new ActionCenterViewModel(dispatcher, actionCenter);

			_timer = new DispatcherTimer
				{
					Interval = TimeSpan.FromMilliseconds(100)
				};
			_timer.Tick += TimerOnTick;
			_timer.Start();

			_autoUpdater = new AutoUpdateViewModel(updater, settings.AutoUpdate, dispatcher);

			_dispatcher = dispatcher;
			WindowTitle = Constants.MainWindowTitle;

			_selectNextDataSourceCommand = new DelegateCommand(SelectNextDataSource);
			_selectPreviousDataSourceCommand = new DelegateCommand(SelectPreviousDataSource);

			_analyseEntry = new AnalyseMainPanelEntry();
			_rawEntry = new LogViewMainPanelEntry();
			_entries = new IMainPanelEntry[]
			{
				//_analyseEntry,
				_rawEntry
			};
			SelectedEntry = _entries.FirstOrDefault(x => x.Id == _applicationSettings.MainWindow.SelectedMainPanel)
			                ?? _rawEntry;

			_sidePanels = new ISidePanelViewModel[]
			{
				_dataSources,
				_quickFilters,
				_bookmarks
			};
			SelectedPanel = _sidePanels.FirstOrDefault(x => x.Id == _applicationSettings.MainWindow.SelectedSidePanel);

			ChangeDataSource(CurrentDataSource);
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

		public ActionCenterViewModel ActionCenter => _actionCenterViewModel;

		public AutoUpdateViewModel AutoUpdater => _autoUpdater;

		public ICommand SelectPreviousDataSourceCommand => _selectPreviousDataSourceCommand;

		public ICommand SelectNextDataSourceCommand => _selectNextDataSourceCommand;

		public string WindowTitle
		{
			get { return _windowTitle; }
			private set
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
			private set
			{
				if (value == _windowTitleSuffix)
					return;

				_windowTitleSuffix = value;
				EmitPropertyChanged();
			}
		}

		public bool IsLogFileOpen
		{
			get { return _isLogFileOpen; }
			private set
			{
				if (value == _isLogFileOpen)
					return;

				_isLogFileOpen = value;
				EmitPropertyChanged();
			}
		}

		public SettingsViewModel Settings => _settings;
		
		#region Main Panel

		public IMainPanelViewModel SelectedMainPanel
		{
			get { return _selectedMainPanel; }
			private set
			{
				if (value == _selectedMainPanel)
					return;

				_selectedMainPanel = value;
				EmitPropertyChanged();
			}
		}

		public IEnumerable<IMainPanelEntry> Entries => _entries;

		public IMainPanelEntry SelectedEntry
		{
			get { return _selectedEntry; }
			set
			{
				if (value == _selectedEntry)
					return;

				_selectedEntry = value;
				EmitPropertyChanged();

				if (value == _analyseEntry)
				{
					SelectedMainPanel = new AnalyseMainPanelViewModel();
				}
				else if (value == _rawEntry)
				{
					SelectedMainPanel = new LogViewMainPanelViewModel(_actionCenter, _applicationSettings)
					{
						CurrentDataSource = CurrentDataSource
					};
				}

				_applicationSettings.MainWindow.SelectedMainPanel = _selectedEntry?.Id;
			}
		}

		#endregion

		public DataSourcesViewModel DataSources => _dataSources;

		public IEnumerable<IDataSourceViewModel> RecentFiles => _dataSources.Observable;

		public IDataSourceViewModel CurrentDataSource
		{
			get { return _dataSources.SelectedItem; }
			set
			{
				if (value == _dataSources.SelectedItem)
					return;

				ChangeDataSource(value);
				EmitPropertyChanged();
			}
		}

		public ISidePanelViewModel SelectedPanel
		{
			get { return _selectedPanel; }
			set
			{
				if (value == _selectedPanel)
					return;

				_selectedPanel = value;
				EmitPropertyChanged();

				_applicationSettings.MainWindow.SelectedSidePanel = _selectedPanel?.Id;
				_applicationSettings.SaveAsync();
			}
		}

		public IEnumerable<ISidePanelViewModel> SidePanels => _sidePanels;

		public event PropertyChangedEventHandler PropertyChanged;
		
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
			_dataSources.SelectedItem = value;
			_quickFilters.CurrentDataSource = value;
			_bookmarks.CurrentDataSource = value?.DataSource;
			OpenFile(value);
		}

		private void SelectNextDataSource()
		{
			if (CurrentDataSource == null)
			{
				CurrentDataSource = _dataSources.Observable.FirstOrDefault();
			}
			else
			{
				ObservableCollection<IDataSourceViewModel> dataSources = _dataSources.Observable;
				int index = dataSources.IndexOf(CurrentDataSource);
				int nextIndex = (index + 1)%dataSources.Count;
				CurrentDataSource = dataSources[nextIndex];
			}
		}

		private void SelectPreviousDataSource()
		{
			if (CurrentDataSource == null)
			{
				CurrentDataSource = _dataSources.Observable.LastOrDefault();
			}
			else
			{
				ObservableCollection<IDataSourceViewModel> dataSources = _dataSources.Observable;
				int index = dataSources.IndexOf(CurrentDataSource);
				int nextIndex = index - 1;
				if (nextIndex < 0)
					nextIndex = dataSources.Count - 1;
				CurrentDataSource = dataSources[nextIndex];
			}
		}

		private void OnQuickFiltersChanged()
		{
			var panel = _selectedMainPanel;
			if (panel != null)
				panel.QuickFilterChain = _quickFilters.CreateFilterChain();
		}

		private void TimerOnTick(object sender, EventArgs eventArgs)
		{
			_dataSources.Update();
			_selectedMainPanel?.Update();
			_actionCenterViewModel.Update();
			_bookmarks.Update();
		}

		public void OpenFiles(string[] files)
		{
			foreach (string file in files)
			{
				OpenFile(file);
			}
		}

		public IDataSourceViewModel OpenFile(string file)
		{
			IDataSourceViewModel dataSource = _dataSources.GetOrAdd(file);
			OpenFile(dataSource);
			return dataSource;
		}

		private void OpenFile(IDataSourceViewModel dataSource)
		{
			if (dataSource != null)
			{
				CurrentDataSource = dataSource;
				if (SelectedMainPanel != null)
					SelectedMainPanel.CurrentDataSource = CurrentDataSource;

				WindowTitle = string.Format("{0} - {1}", Constants.MainWindowTitle, dataSource.DisplayName);
				WindowTitleSuffix = dataSource.DataSourceOrigin;
			}
			else
			{
				CurrentDataSource = null;
				if (SelectedMainPanel != null)
					SelectedMainPanel.CurrentDataSource = null;
				WindowTitle = Constants.MainWindowTitle;
				WindowTitleSuffix = null;
			}
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public QuickFilterViewModel AddQuickFilter()
		{
			return _quickFilters.AddQuickFilter();
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
	}
}