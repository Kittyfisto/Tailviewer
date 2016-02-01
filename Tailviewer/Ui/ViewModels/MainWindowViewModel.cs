using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using Tailviewer.Settings;
using DataSources = Tailviewer.BusinessLogic.DataSources;
using QuickFilters = Tailviewer.BusinessLogic.QuickFilters;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class MainWindowViewModel
		: INotifyPropertyChanged
	{
		private readonly DataSourcesViewModel _dataSourcesViewModel;
		private readonly IDispatcher _dispatcher;
		private readonly QuickFiltersViewModel _quickFilters;
		private readonly ICommand _selectNextDataSourceCommand;
		private readonly ICommand _selectPreviousDataSourceCommand;
		private readonly DispatcherTimer _timer;

		private DataSourceViewModel _currentDataSource;
		private LogViewerViewModel _currentDataSourceLogView;
		private string _errorMessage;
		private bool _hasError;
		private bool _isLogFileOpen;
		private string _windowTitle;

		public MainWindowViewModel(ApplicationSettings settings, DataSources dataSources, QuickFilters quickFilters,
		                           IDispatcher dispatcher)
		{
			if (dataSources == null) throw new ArgumentNullException("dataSources");
			if (dispatcher == null) throw new ArgumentNullException("dispatcher");

			_dataSourcesViewModel = new DataSourcesViewModel(settings, dataSources);
			_quickFilters = new QuickFiltersViewModel(settings, quickFilters);
			_quickFilters.OnFiltersChanged += OnQuickFiltersChanged;
			_timer = new DispatcherTimer
				{
					Interval = TimeSpan.FromMilliseconds(100)
				};
			_timer.Tick += TimerOnTick;
			_timer.Start();

			_dispatcher = dispatcher;
			WindowTitle = Constants.MainWindowTitle;

			_selectNextDataSourceCommand = new DelegateCommand(SelectNextDataSource);
			_selectPreviousDataSourceCommand = new DelegateCommand(SelectPreviousDataSource);
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

		public string ErrorMessage
		{
			get { return _errorMessage; }
			set
			{
				if (value == _errorMessage)
					return;

				_errorMessage = value;
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

		public LogViewerViewModel CurrentDataSourceLogView
		{
			get { return _currentDataSourceLogView; }
			private set
			{
				if (_currentDataSourceLogView == value)
					return;

				if (_currentDataSourceLogView != null)
					_currentDataSourceLogView.Dispose();

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
			get
			{
				return _quickFilters.Observable;
			}
		}

		public ICommand AddQuickFilterCommand
		{
			get { return _quickFilters.AddCommand; }
		}

		public IEnumerable<DataSourceViewModel> RecentFiles
		{
			get { return _dataSourcesViewModel.Observable; }
		}

		public DataSourceViewModel CurrentDataSource
		{
			get { return _currentDataSource; }
			set
			{
				if (value == _currentDataSource)
					return;

				_currentDataSource = value;
				_quickFilters.CurrentDataSource = value;

				OpenFile(value);
				EmitPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void SelectNextDataSource()
		{
			if (CurrentDataSource == null)
			{
				CurrentDataSource = _dataSourcesViewModel.Observable.FirstOrDefault();
			}
			else
			{
				var dataSources = _dataSourcesViewModel.Observable;
				var index = dataSources.IndexOf(CurrentDataSource);
				var nextIndex = (index + 1) % dataSources.Count;
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
				var dataSources = _dataSourcesViewModel.Observable;
				var index = dataSources.IndexOf(CurrentDataSource);
				var nextIndex = index - 1;
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

		public DataSourceViewModel OpenFile(string file)
		{
			DataSourceViewModel dataSource = _dataSourcesViewModel.GetOrAdd(file);
			OpenFile(dataSource);
			return dataSource;
		}

		private void OpenFile(DataSourceViewModel dataSource)
		{
			if (dataSource != null)
			{
				CurrentDataSource = dataSource;
				CurrentDataSourceLogView = new LogViewerViewModel(
					dataSource,
					_dispatcher);
				WindowTitle = string.Format("{0} - {1}", Constants.MainWindowTitle, dataSource.FileName);
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
	}
}