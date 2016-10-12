using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using Metrolib;
using Tailviewer.BusinessLogic.AutoUpdates;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.DataSourceTree;
using log4net;
using DataSources = Tailviewer.BusinessLogic.DataSources.DataSources;
using QuickFilters = Tailviewer.BusinessLogic.Filters.QuickFilters;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class MainWindowViewModel
		: INotifyPropertyChanged
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICommand _closeErrorDialogCommand;
		private readonly DataSourcesViewModel _dataSourcesViewModel;
		private readonly SettingsViewModel _settings;
		private readonly IDispatcher _dispatcher;
		private readonly ICommand _gotItCommand;
		private readonly ICommand _addDataSourceCommand;
		private readonly QuickFiltersViewModel _quickFilters;
		private readonly ICommand _selectNextDataSourceCommand;
		private readonly ICommand _selectPreviousDataSourceCommand;
		private readonly ICommand _checkForUpdatesCommand;

		private readonly DispatcherTimer _timer;
		private readonly IAutoUpdater _updater;

		private LogViewerViewModel _currentDataSourceLogView;
		private Exception _exception;
		private bool _hasError;
		private bool _isLogFileOpen;
		private bool _isUpdateAvailable;
		private bool _showUpdateAvailable;
		private string _windowTitle;
		private Version _latestVersion;
		private Uri _latestVersionUri;

		public MainWindowViewModel(ApplicationSettings settings,
		                           DataSources dataSources,
		                           QuickFilters quickFilters,
		                           IAutoUpdater updater,
		                           IDispatcher dispatcher)
		{
			if (dataSources == null) throw new ArgumentNullException("dataSources");
			if (quickFilters == null) throw new ArgumentNullException("quickFilters");
			if (updater == null) throw new ArgumentNullException("updater");
			if (dispatcher == null) throw new ArgumentNullException("dispatcher");

			_dataSourcesViewModel = new DataSourcesViewModel(settings, dataSources);
			_dataSourcesViewModel.PropertyChanged += DataSourcesViewModelOnPropertyChanged;
			_quickFilters = new QuickFiltersViewModel(settings, quickFilters);
			_quickFilters.OnFiltersChanged += OnQuickFiltersChanged;
			_settings = new SettingsViewModel(settings);

			_timer = new DispatcherTimer
				{
					Interval = TimeSpan.FromMilliseconds(100)
				};
			_timer.Tick += TimerOnTick;
			_timer.Start();

			_updater = updater;

			_dispatcher = dispatcher;
			WindowTitle = Constants.MainWindowTitle;

			_selectNextDataSourceCommand = new DelegateCommand(SelectNextDataSource);
			_selectPreviousDataSourceCommand = new DelegateCommand(SelectPreviousDataSource);
			_closeErrorDialogCommand = new DelegateCommand(CloseErrorDialog);
			_gotItCommand = new DelegateCommand(GotIt);

			_addDataSourceCommand = new DelegateCommand(AddDataSource);

			_checkForUpdatesCommand = new DelegateCommand(CheckForUpdates);

			ChangeDataSource(CurrentDataSource);

			_updater.LatestVersionChanged += UpdaterOnLatestVersionChanged;
			if (_settings.CheckForUpdates)
			{
				_updater.CheckForUpdatesAsync();
			}
		}

		private void CheckForUpdates()
		{
			_updater.CheckForUpdatesAsync();
		}

		private void AddDataSource()
		{
			// Create OpenFileDialog 
			var dlg = new Microsoft.Win32.OpenFileDialog
				{
					DefaultExt = ".log",
					Filter = "Log Files (*.log)|*.log|Txt Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
					Multiselect = true
				};

			// Display OpenFileDialog by calling ShowDialog method 
			if (dlg.ShowDialog() == true)
			{
				var selectedFiles = dlg.FileNames;
				foreach (var fileName in selectedFiles)
				{
					OpenFile(fileName);
				}
			}
		}

		public ICommand GotItCommand
		{
			get { return _gotItCommand; }
		}

		public bool ShowUpdateAvailable
		{
			get { return _showUpdateAvailable; }
			private set
			{
				if (value == _showUpdateAvailable)
					return;

				_showUpdateAvailable = value;
				EmitPropertyChanged();
			}
		}

		public Version LatestVersion
		{
			get { return _latestVersion; }
			private set
			{
				if (value == _latestVersion)
					return;

				_latestVersion = value;
				EmitPropertyChanged();
			}
		}

		public Uri LatestVersionUri
		{
			get { return _latestVersionUri; }
			private set
			{
				if (value == _latestVersionUri)
					return;

				_latestVersionUri = value;
				EmitPropertyChanged();
			}
		}

		public bool IsUpdateAvailable
		{
			get { return _isUpdateAvailable; }
			private set
			{
				if (value == _isUpdateAvailable)
					return;

				_isUpdateAvailable = value;
				EmitPropertyChanged();
			}
		}

		public ICommand SelectPreviousDataSourceCommand
		{
			get { return _selectPreviousDataSourceCommand; }
		}

		public ICommand SelectNextDataSourceCommand
		{
			get { return _selectNextDataSourceCommand; }
		}

		public bool HasError
		{
			get { return _hasError; }
			set
			{
				if (value == _hasError)
					return;

				_hasError = value;
				EmitPropertyChanged();
			}
		}

		public ICommand CloseErrorDialogCommand
		{
			get { return _closeErrorDialogCommand; }
		}

		public Exception Exception
		{
			get { return _exception; }
			set
			{
				if (value == _exception)
					return;

				_exception = value;
				EmitPropertyChanged();
			}
		}

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

		public SettingsViewModel Settings
		{
			get { return _settings; }
		}

		public LogViewerViewModel CurrentDataSourceLogView
		{
			get { return _currentDataSourceLogView; }
			private set
			{
				if (_currentDataSourceLogView == value)
					return;

				_currentDataSourceLogView = value;
				if (value != null)
				{
					value.QuickFilterChain = _quickFilters.CreateFilterChain();
					IsLogFileOpen = true;
				}
				else
				{
					IsLogFileOpen = false;
				}
				EmitPropertyChanged();
			}
		}

		public IEnumerable<QuickFilterViewModel> QuickFilters
		{
			get { return _quickFilters.Observable; }
		}

		public ICommand AddQuickFilterCommand
		{
			get { return _quickFilters.AddCommand; }
		}

		public IEnumerable<IDataSourceViewModel> RecentFiles
		{
			get { return _dataSourcesViewModel.Observable; }
		}

		public IDataSourceViewModel CurrentDataSource
		{
			get { return _dataSourcesViewModel.SelectedItem; }
			set
			{
				if (value == _dataSourcesViewModel.SelectedItem)
					return;

				ChangeDataSource(value);
				EmitPropertyChanged();
			}
		}

		public ICommand AddDataSourceCommand
		{
			get { return _addDataSourceCommand; }
		}

		public ICommand CheckForUpdatesCommand
		{
			get { return _checkForUpdatesCommand; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void GotIt()
		{
			ShowUpdateAvailable = false;
		}

		private void UpdaterOnLatestVersionChanged(VersionInfo versionInfo)
		{
			_dispatcher.BeginInvoke(() =>
				{
					Version latest = versionInfo.Stable;
					Version current = _updater.AppVersion;
					if (current != null && latest != null && latest > current)
					{
						IsUpdateAvailable = true;
						ShowUpdateAvailable = true;
						LatestVersion = latest;
						LatestVersionUri = versionInfo.StableAddress;

						Log.InfoFormat("A newer version ({0}) is available to be downloaded", latest);
					}
					else
					{
						Log.InfoFormat("You are running the latest version!");
					}
				});
		}

		private void DataSourcesViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "SelectedItem":
					ChangeDataSource(_dataSourcesViewModel.SelectedItem);
					EmitPropertyChanged("CurrentDataSource");
					break;
			}
		}

		private void ChangeDataSource(IDataSourceViewModel value)
		{
			_dataSourcesViewModel.SelectedItem = value;
			_quickFilters.CurrentDataSource = value;
			OpenFile(value);
		}

		private void CloseErrorDialog()
		{
			Exception = null;
			HasError = false;
		}

		private void SelectNextDataSource()
		{
			if (CurrentDataSource == null)
			{
				CurrentDataSource = _dataSourcesViewModel.Observable.FirstOrDefault();
			}
			else
			{
				ObservableCollection<IDataSourceViewModel> dataSources = _dataSourcesViewModel.Observable;
				int index = dataSources.IndexOf(CurrentDataSource);
				int nextIndex = (index + 1)%dataSources.Count;
				CurrentDataSource = dataSources[nextIndex];
			}
		}

		private void SelectPreviousDataSource()
		{
			if (CurrentDataSource == null)
			{
				CurrentDataSource = _dataSourcesViewModel.Observable.LastOrDefault();
			}
			else
			{
				ObservableCollection<IDataSourceViewModel> dataSources = _dataSourcesViewModel.Observable;
				int index = dataSources.IndexOf(CurrentDataSource);
				int nextIndex = index - 1;
				if (nextIndex < 0)
					nextIndex = dataSources.Count - 1;
				CurrentDataSource = dataSources[nextIndex];
			}
		}

		private void OnQuickFiltersChanged()
		{
			LogViewerViewModel view = _currentDataSourceLogView;
			if (view != null)
				view.QuickFilterChain = _quickFilters.CreateFilterChain();
		}

		private void TimerOnTick(object sender, EventArgs eventArgs)
		{
			_dataSourcesViewModel.Update();
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
			IDataSourceViewModel dataSource = _dataSourcesViewModel.GetOrAdd(file);
			OpenFile(dataSource);
			return dataSource;
		}

		private void OpenFile(IDataSourceViewModel dataSource)
		{
			if (dataSource != null)
			{
				CurrentDataSource = dataSource;
				CurrentDataSourceLogView = new LogViewerViewModel(
					dataSource,
					_dispatcher);
				WindowTitle = string.Format("{0} - {1}", Constants.MainWindowTitle, dataSource.DisplayName);
			}
			else
			{
				CurrentDataSource = null;
				CurrentDataSourceLogView = null;
				WindowTitle = Constants.MainWindowTitle;
			}
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public QuickFilterViewModel AddQuickFilter()
		{
			return _quickFilters.AddQuickFilter();
		}

		public bool CanBeDragged(IDataSourceViewModel source)
		{
			return _dataSourcesViewModel.CanBeDragged(source);
		}

		public bool CanBeDropped(IDataSourceViewModel source,
		                         IDataSourceViewModel dest,
		                         DataSourceDropType dropType,
		                         out IDataSourceViewModel finalDest)
		{
			return _dataSourcesViewModel.CanBeDropped(source, dest, dropType, out finalDest);
		}

		public void OnDropped(IDataSourceViewModel source,
		                      IDataSourceViewModel dest,
		                      DataSourceDropType dropType)
		{
			_dataSourcesViewModel.OnDropped(source, dest, dropType);
		}
	}
}