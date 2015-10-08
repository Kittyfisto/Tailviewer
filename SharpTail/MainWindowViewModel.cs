using System;
using System.ComponentModel;
using System.IO;
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
		private LogViewerViewModel _logViewModel;
		private readonly UiDispatcher _dispatcher;
		private bool _isLogFileOpen;
		private string _windowTitle;
		private bool _hasError;
		private string _errorMessage;

		public const string ApplicationName = "SharpTail";

		public MainWindowViewModel()
		{}

		public MainWindowViewModel(Dispatcher dispatcher)
		{
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

		public LogViewerViewModel LogViewModel
		{
			get { return _logViewModel; }
			private set
			{
				_logViewModel = value;
				IsLogFileOpen = value != null;
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
			LogViewModel = new LogViewerViewModel(
				_dispatcher,
				LogFile.FromFile(file));

			var fileName = Path.GetFileName(file);
			WindowTitle = string.Format("{0} - {1}", ApplicationName, fileName);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}