using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Metrolib;

namespace Installer
{
	public sealed class MainWindowViewModel
		: INotifyPropertyChanged
	{
		private readonly Version _appVersion;
		private readonly IDispatcher _dispatcher;
		private readonly ICommand _browseCommand;
		private readonly DelegateCommand _installationCommand;
		private readonly DelegateCommand _launchCommand;

		private bool _agreeToLicense;
		private string _installationPath;
		private double _installationProgress;
		private Size _installationSize;

		private Task _installationTask;
		private bool _isInstalling;
		private bool _isPostInstallation;
		private bool _isPreInstallation;
		private string _installationResult;
		private bool _success;

		public MainWindowViewModel(IDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
			_appVersion = Constants.ApplicationVersion;
			_installationPath =
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Constants.ApplicationTitle);
			_browseCommand = new DelegateCommand(DoBrowse);
			_installationCommand = new DelegateCommand(DoInstall, CanClickInstall);
			_launchCommand = new DelegateCommand(DoLaunch, CanLaunch);

			IsPreInstallation = true;
		}

		private bool CanLaunch()
		{
			return _success;
		}

		private void DoLaunch()
		{
			var app = Path.Combine(_installationPath, "Tailviewer.exe");
			Process.Start(app);
			System.Windows.Application.Current.Shutdown();
		}

		public DelegateCommand LaunchCommand
		{
			get { return _launchCommand; }
		}

		public Version AppVersion
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
				UpdateCanInstall();
				EmitPropertyChanged();
			}
		}

		public string AppTitle
		{
			get { return Constants.ApplicationTitle; }
		}

		public ICommand InstallationCommand
		{
			get { return _installationCommand; }
		}

		public bool IsPreInstallation
		{
			get { return _isPreInstallation; }
			private set
			{
				if (value == _isPreInstallation)
					return;

				_isPreInstallation = value;
				EmitPropertyChanged();
			}
		}

		public bool IsInstalling
		{
			get { return _isInstalling; }
			private set
			{
				if (value == _isInstalling)
					return;

				_isInstalling = value;
				EmitPropertyChanged();
			}
		}

		public bool IsPostInstallation
		{
			get { return _isPostInstallation; }
			private set
			{
				if (value == _isPostInstallation)
					return;

				_isPostInstallation = value;
				EmitPropertyChanged();
			}
		}

		public double InstallationProgress
		{
			get { return _installationProgress; }
			private set
			{
				if (value == _installationProgress)
					return;

				_installationProgress = value;
				EmitPropertyChanged();
			}
		}

		public string InstallationResult
		{
			get { return _installationResult; }
			private set
			{
				_installationResult = value;
				EmitPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[Pure]
		private bool CanClickInstall()
		{
			if (!_agreeToLicense)
			{
				return false;
			}

			if (_installationTask != null)
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(_installationPath))
			{
				return false;
			}

			return true;
		}

		private void DoInstall()
		{
			IsPreInstallation = false;
			IsInstalling = true;
			UpdateCanInstall();

			_installationTask = Task.Factory.StartNew(Installation, TaskCreationOptions.LongRunning);
			_installationTask.ContinueWith(OnInstallationFinished);
		}

		private void OnInstallationFinished(Task task)
		{
			string result;
			if (task.IsFaulted)
			{
				result = "failed";
			}
			else
			{
				_success = true;
				result = "succeeded";
			}

			_dispatcher.BeginInvoke(() =>
				{
					_installationTask = null;

					InstallationResult = result;

					IsInstalling = false;
					IsPostInstallation = true;

					_launchCommand.RaiseCanExecuteChanged();
				});
		}

		private void Installation()
		{
			using (var installer = new Installer(_installationPath))
			{
				installer.Run();
			}
		}

		private void DoBrowse()
		{
			var folderBrowser = new FolderBrowserDialog();
			folderBrowser.Description = "Select Chase 6 Installation Folder";
			folderBrowser.RootFolder = Environment.SpecialFolder.MyComputer;
			folderBrowser.ShowNewFolderButton = false;

			if (Directory.Exists(_installationPath))
			{
				folderBrowser.SelectedPath = _installationPath;
			}

			if (folderBrowser.ShowDialog() == DialogResult.OK)
			{
				_installationPath = folderBrowser.SelectedPath;
			}
		}

		private void UpdateCanInstall()
		{
			_installationCommand.RaiseCanExecuteChanged();
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}