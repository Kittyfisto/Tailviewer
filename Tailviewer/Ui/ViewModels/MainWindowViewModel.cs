using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using Metrolib;
using Microsoft.Win32;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.AutoUpdates;
using Tailviewer.Ui.Controls.ActionCenter;
using Tailviewer.Ui.Controls.DataSourceTree;
using ApplicationSettings = Tailviewer.Settings.ApplicationSettings;
using DataSources = Tailviewer.BusinessLogic.DataSources.DataSources;
using QuickFilters = Tailviewer.BusinessLogic.Filters.QuickFilters;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class MainWindowViewModel
		: INotifyPropertyChanged
	{
		#region Dispatching

		private readonly IDispatcher _dispatcher;
		private readonly DispatcherTimer _timer;

		#endregion

		#region ViewModels

		private readonly ActionCenterViewModel _actionCenter;
		private readonly AutoUpdateViewModel _autoUpdater;
		private readonly DataSourcesViewModel _dataSourcesViewModel;
		private readonly QuickFiltersViewModel _quickFilters;
		private readonly SettingsViewModel _settings;

		#endregion

		#region Commands

		private readonly ICommand _addDataSourceCommand;
		private readonly ICommand _selectNextDataSourceCommand;
		private readonly ICommand _selectPreviousDataSourceCommand;

		#endregion

		private LogViewerViewModel _currentDataSourceLogView;
		private bool _isLogFileOpen;
		private string _windowTitle;

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

			_dataSourcesViewModel = new DataSourcesViewModel(settings, dataSources);
			_dataSourcesViewModel.PropertyChanged += DataSourcesViewModelOnPropertyChanged;
			_quickFilters = new QuickFiltersViewModel(settings, quickFilters);
			_quickFilters.OnFiltersChanged += OnQuickFiltersChanged;
			_settings = new SettingsViewModel(settings);
			_actionCenter = new ActionCenterViewModel(dispatcher, actionCenter);

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
			_addDataSourceCommand = new DelegateCommand(AddDataSource);

			ChangeDataSource(CurrentDataSource);
		}

		public ActionCenterViewModel ActionCenter
		{
			get { return _actionCenter; }
		}

		public AutoUpdateViewModel AutoUpdater
		{
			get { return _autoUpdater; }
		}

		public ICommand SelectPreviousDataSourceCommand
		{
			get { return _selectPreviousDataSourceCommand; }
		}

		public ICommand SelectNextDataSourceCommand
		{
			get { return _selectNextDataSourceCommand; }
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

		public event PropertyChangedEventHandler PropertyChanged;

		private void AddDataSource()
		{
			// Create OpenFileDialog 
			var dlg = new OpenFileDialog
				{
					DefaultExt = ".log",
					Filter = "Log Files (*.log)|*.log|Txt Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
					Multiselect = true
				};

			// Display OpenFileDialog by calling ShowDialog method 
			if (dlg.ShowDialog() == true)
			{
				string[] selectedFiles = dlg.FileNames;
				foreach (string fileName in selectedFiles)
				{
					OpenFile(fileName);
				}
			}
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
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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