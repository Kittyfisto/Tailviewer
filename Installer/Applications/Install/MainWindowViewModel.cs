using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Metrolib;
using Application = System.Windows.Application;

namespace Installer.Applications.Install
{
	public sealed class MainWindowViewModel
		: AbstractViewModel
	{
		private readonly Version _appVersion;
		private readonly ICommand _browseCommand;
		private readonly IDispatcher _dispatcher;
		private readonly DelegateCommand _installationCommand;
		private readonly Installer _installer;
		private readonly DelegateCommand _launchCommand;

		private bool _agreeToLicense;
		private string _errorMessage;
		private bool _hasFailed;
		private string _installationPath;
		private double _installationProgress;
		private string _installationResult;

		private Task _installationTask;
		private bool _isInstalling;
		private bool _isPostInstallation;
		private bool _isPreInstallation;
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
			_installer = new Installer();

			IsPreInstallation = true;
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
			get { return _installer.InstallationSize; }
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

		public bool HasFailed
		{
			get { return _hasFailed; }
			private set
			{
				if (value == _hasFailed)
					return;

				_hasFailed = value;
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

		public void Update()
		{
			InstallationProgress = _installer.Progress;
		}

		private bool CanLaunch()
		{
			return _success;
		}

		private void DoLaunch()
		{
			string app = Path.Combine(_installationPath, "Tailviewer.exe");
			Process.Start(app);
			Application.Current.Shutdown();
		}

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
			_dispatcher.BeginInvoke(() =>
				{
					string result;
					if (task.IsFaulted)
					{
						result = "failed";
						HasFailed = true;
						ErrorMessage = FormatErrorMessage(task.Exception);
					}
					else
					{
						_success = true;
						result = "succeeded";
					}

					_installationTask = null;

					InstallationResult = result;

					IsInstalling = false;
					IsPostInstallation = true;

					_launchCommand.RaiseCanExecuteChanged();
				});
		}

		private void Installation()
		{
			_installer.Run(_installationPath);
		}

		private void DoBrowse()
		{
			var folderBrowser = new FolderBrowserDialog();
			folderBrowser.Description = string.Format("Choose folder for {0}", Constants.ApplicationTitle);
			folderBrowser.RootFolder = Environment.SpecialFolder.MyComputer;
			folderBrowser.ShowNewFolderButton = false;

			if (Directory.Exists(_installationPath))
			{
				folderBrowser.SelectedPath = _installationPath;
			}

			if (folderBrowser.ShowDialog() == DialogResult.OK)
			{
				InstallationPath = folderBrowser.SelectedPath;
			}
		}

		private void UpdateCanInstall()
		{
			_installationCommand.RaiseCanExecuteChanged();
		}
	}
}