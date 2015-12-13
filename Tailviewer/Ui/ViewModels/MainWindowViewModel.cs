using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class MainWindowViewModel
		: INotifyPropertyChanged
	{
		private readonly DataSources _dataSources;
		private readonly DataSourcesViewModel _dataSourcesViewModel;
		private readonly UiDispatcher _dispatcher;
		private readonly DispatcherTimer _timer;

		private DataSourceViewModel _currentDataSource;
		private LogViewerViewModel _currentDataSourceLogView;
		private string _errorMessage;
		private bool _hasError;
		private bool _isLogFileOpen;
		private string _windowTitle;

		public MainWindowViewModel()
		{
		}

		public MainWindowViewModel(DataSources dataSources, Dispatcher dispatcher)
		{
			if (dataSources == null) throw new ArgumentNullException("dataSources");
			if (dispatcher == null) throw new ArgumentNullException("dispatcher");

			_dataSources = dataSources;
			_dataSourcesViewModel = new DataSourcesViewModel(_dataSources);
			_timer = new DispatcherTimer
				{
					Interval = TimeSpan.FromMilliseconds(100)
				};
			_timer.Tick += TimerOnTick;
			_timer.Start();

			_dispatcher = new UiDispatcher(dispatcher);
			WindowTitle = Constants.MainWindowTitle;
			dispatcher.UnhandledException += DispatcherOnUnhandledException;
		}

		private void TimerOnTick(object sender, EventArgs eventArgs)
		{
			_dataSourcesViewModel.Update();
		}

		public bool HasError
		{
			get { return _hasError; }
			private set
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
			private set
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
				IsLogFileOpen = value != null;
				EmitPropertyChanged();
			}
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
				OpenFile(value);
				EmitPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
		{
			Exception exception = args.Exception;
			if (exception != null)
			{
				HasError = true;
				ErrorMessage = exception.Message;
			}
			args.Handled = true;
		}

		public void OpenFiles(string[] files)
		{
			foreach (var file in files)
			{
				OpenFile(file);
			}
		}

		private void OpenFile(string file)
		{
			DataSourceViewModel dataSource = _dataSourcesViewModel.GetOrAdd(file);
			OpenFile(dataSource);
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
	}
}