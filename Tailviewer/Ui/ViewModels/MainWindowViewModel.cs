using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.DataSourceTree;
using DataSources = Tailviewer.BusinessLogic.DataSources;
using QuickFilters = Tailviewer.BusinessLogic.QuickFilters;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class MainWindowViewModel
		: INotifyPropertyChanged
	{
		private readonly ICommand _closeErrorDialogCommand;
		private readonly DataSourcesViewModel _dataSourcesViewModel;
		private readonly IDispatcher _dispatcher;
		private readonly QuickFiltersViewModel _quickFilters;
		private readonly ICommand _selectNextDataSourceCommand;
		private readonly ICommand _selectPreviousDataSourceCommand;
		private readonly DispatcherTimer _timer;

		private IDataSourceViewModel _currentDataSource;
		private LogViewerViewModel _currentDataSourceLogView;
		private Exception _exception;
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
			_closeErrorDialogCommand = new DelegateCommand(CloseErrorDialog);
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