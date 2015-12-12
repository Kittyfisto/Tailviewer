using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using SharpTail.BusinessLogic;
using SharpTail.Ui;
using SharpTail.Ui.ViewModels;

namespace SharpTail
{
	public sealed class MainWindowViewModel
		: INotifyPropertyChanged
	{
		private LogViewerViewModel _currentDataSourceLogView;
		private readonly UiDispatcher _dispatcher;
		private bool _isLogFileOpen;
		private string _windowTitle;
		private bool _hasError;
		private string _errorMessage;
		private readonly DataSourceCollection _dataSources;
		private DataSourceViewModel _currentDataSource;

		public const string ApplicationName = "SharpTail";

		public MainWindowViewModel()
		{}

		public MainWindowViewModel(Dispatcher dispatcher)
		{
			_dataSources = new DataSourceCollection();

			_dispatcher = new UiDispatcher(dispatcher);
			WindowTitle = ApplicationName;
			dispatcher.UnhandledException += DispatcherOnUnhandledException;
		}

		private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
		{
			var exception = args.Exception;
			if (exception != null)
			{
				HasError = true;
				ErrorMessage = exception.Message;
			}
			args.Handled = true;
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
				_currentDataSourceLogView = value;
				IsLogFileOpen = value != null;
				EmitPropertyChanged();
			}
		}

		public IEnumerable<DataSourceViewModel> RecentFiles
		{
			get { return _dataSources.Observable; }
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

		public void OpenFiles(string[] files)
		{
			var file = files[0];
			OpenFile(file);
		}

		private void OpenFile(string file)
		{
			var dataSource = _dataSources.GetOrAdd(file);
			OpenFile(dataSource);
		}

		private void OpenFile(DataSourceViewModel dataSource)
		{
			LogFile logFile;
			if (TryOpenFile(dataSource, out logFile))
			{
				CurrentDataSource = dataSource;
				CurrentDataSourceLogView = new LogViewerViewModel(
					dataSource,
					_dispatcher,
					logFile);

				WindowTitle = string.Format("{0} - {1}", ApplicationName, dataSource.FileName);
			}
			else
			{
				CurrentDataSourceLogView = null;
				WindowTitle = ApplicationName;
			}
		}

		private bool TryOpenFile(DataSourceViewModel source, out LogFile logFile)
		{
			if (source != null)
			{
				try
				{
					logFile = LogFile.FromFile(source.DataSource);
					source.LastOpened = DateTime.Now;
					return true;
				}
				catch (Exception e)
				{
					// TODO: Log & display this problem
				}
			}

			logFile = null;
			return false;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}