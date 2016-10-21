using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.AutoUpdates;
using Tailviewer.Settings;
using log4net;

namespace Tailviewer.Ui.ViewModels
{
	public sealed class AutoUpdateViewModel
		: INotifyPropertyChanged
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly ICommand _checkForUpdatesCommand;

		private readonly IDispatcher _dispatcher;
		private readonly ICommand _gotItCommand;
		private readonly DelegateCommand _installCommand;
		private readonly AutoUpdateSettings _settings;
		private readonly IAutoUpdater _updater;
		private bool _isUpdateAvailable;
		private VersionInfo _latest;
		private Version _latestVersion;
		private Uri _latestVersionUri;
		private bool _showUpdateAvailable;
		private string _installTitle;
		private Task _installationTask;

		public AutoUpdateViewModel(IAutoUpdater updater, AutoUpdateSettings settings, IDispatcher dispatcher)
		{
			if (updater == null)
				throw new ArgumentNullException("updater");
			if (settings == null)
				throw new ArgumentNullException("settings");
			if (dispatcher == null)
				throw new ArgumentNullException("dispatcher");

			_updater = updater;
			_settings = settings;
			_dispatcher = dispatcher;
			_installCommand = new DelegateCommand(Install, CanInstall);
			_gotItCommand = new DelegateCommand(GotIt);
			_checkForUpdatesCommand = new DelegateCommand(CheckForUpdates);
			_installTitle = "Install";

			_updater.LatestVersionChanged += UpdaterOnLatestVersionChanged;
		}

		public Version LatestVersion
		{
			get { return _latestVersion; }
			private set
			{
				if (value == _latestVersion)
					return;

				_latestVersion = value;
				EmitPropertyChanged();
			}
		}

		public string InstallTitle
		{
			get { return _installTitle; }
			private set
			{
				if (value == _installTitle)
					return;

				_installTitle = value;
				EmitPropertyChanged();
			}
		}

		public ICommand CheckForUpdatesCommand
		{
			get { return _checkForUpdatesCommand; }
		}

		public bool ShowUpdateAvailable
		{
			get { return _showUpdateAvailable; }
			private set
			{
				if (value == _showUpdateAvailable)
					return;

				_showUpdateAvailable = value;
				EmitPropertyChanged();
			}
		}

		public Uri LatestVersionUri
		{
			get { return _latestVersionUri; }
			private set
			{
				if (value == _latestVersionUri)
					return;

				_latestVersionUri = value;
				EmitPropertyChanged();
			}
		}

		public bool IsUpdateAvailable
		{
			get { return _isUpdateAvailable; }
			private set
			{
				if (value == _isUpdateAvailable)
					return;

				_isUpdateAvailable = value;
				EmitPropertyChanged();
			}
		}

		public ICommand InstallCommand
		{
			get { return _installCommand; }
		}

		public ICommand GotItCommand
		{
			get { return _gotItCommand; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void CheckForUpdates()
		{
			_updater.CheckForUpdates(addNotificationWhenUpToDate: true);
		}

		private bool CanInstall()
		{
			return _installationTask == null;
		}

		private void Install()
		{
			_installationTask = _updater.Install(_latest);
			_installationTask.ContinueWith(Shutdown);

			InstallTitle = "Downloading...";
			_installCommand.RaiseCanExecuteChanged();
		}

		private void Shutdown(Task unused)
		{
			Log.InfoFormat("Shutting down to perform the update");
			_dispatcher.BeginInvoke(() => Application.Current.Shutdown());
		}

		private void GotIt()
		{
			ShowUpdateAvailable = false;
		}

		private void UpdaterOnLatestVersionChanged(VersionInfo versionInfo)
		{
			_dispatcher.BeginInvoke(() =>
				{
					_latest = versionInfo;
					Version latest = versionInfo.Stable;
					Version current = _updater.AppVersion;
					if (current != null && latest != null && latest > current)
					{
						IsUpdateAvailable = true;
						ShowUpdateAvailable = true;
						LatestVersion = latest;
						LatestVersionUri = versionInfo.StableAddress;

						Log.InfoFormat("A newer version ({0}) is available to be downloaded", latest);
					}
					else
					{
						Log.InfoFormat("You are running the latest version!");
					}
				});
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}