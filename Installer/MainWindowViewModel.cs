using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using Metrolib;

namespace Installer
{
	public sealed class MainWindowViewModel
		: INotifyPropertyChanged
	{
		private readonly string _appVersion;
		private readonly ICommand _browseCommand;
		private readonly Dispatcher _dispatcher;

		private bool _agreeToLicense;
		private string _installationPath;
		private Size _installationSize;

		public MainWindowViewModel(Dispatcher dispatcher)
		{
			_dispatcher = dispatcher;
			_installationPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
		}

		public string AppVersion
		{
			get { return _appVersion; }
		}

		public Size InstallationSize
		{
			get { return _installationSize; }
		}

		public string InstallationPath
		{
			get { return _installationPath; }
			set
			{
				if (value == _installationPath)
					return;

				_installationPath = value;
				EmitPropertyChanged();
			}
		}

		public ICommand BrowseCommand
		{
			get { return _browseCommand; }
		}

		public bool AgreeToLicense
		{
			get { return _agreeToLicense; }
			set
			{
				if (value == _agreeToLicense)
					return;

				_agreeToLicense = value;
				EmitPropertyChanged();
			}
		}

		public string AppTitle
		{
			get { return Constants.ApplicationTitle; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}