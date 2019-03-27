using System;
using System.Threading.Tasks;
using System.Windows;
using Metrolib;

namespace Installer.Applications.Update
{
	public sealed class UpdateWindowViewModel
		: AbstractViewModel
	{
		private readonly UiDispatcher _dispatcher;
		private readonly string _installationPath;
		private readonly Task _installationTask;
		private readonly Installer _installer;

		private string _errorMessage;
		private bool _hasFailed;
		private double _installationProgress;
		private string _installationResult;
		private bool _isPostInstallation;

		public UpdateWindowViewModel(UiDispatcher dispatcher, string installationPath)
		{
			if (dispatcher == null)
				throw new ArgumentNullException("dispatcher");
			if (installationPath == null)
				throw new ArgumentNullException("installationPath");

			_dispatcher = dispatcher;
			_installationPath = installationPath;
			_installer = new Installer();

			_installationTask = Task.Factory.StartNew(Installation, TaskCreationOptions.LongRunning);
			_installationTask.ContinueWith(OnInstallationFinished);
		}

		public void Update()
		{
			InstallationProgress = _installer.Progress;
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

		private void Installation()
		{
			_installer.Run(_installationPath);
		}

		private void OnInstallationFinished(Task task)
		{
			_dispatcher.BeginInvoke(() =>
				{
					if (task.IsFaulted)
					{
						InstallationResult = "failed";
						HasFailed = true;
						ErrorMessage = FormatErrorMessage(task.Exception);
						IsPostInstallation = true;
					}
					else
					{
						_installer.Launch();
						Application.Current.Shutdown();
					}
				});
		}
	}
}