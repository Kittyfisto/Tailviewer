using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Tailviewer.Settings;

namespace Tailviewer.Ui.ViewModels
{
	public sealed class SettingsViewModel
		: INotifyPropertyChanged
	{
		private readonly ApplicationSettings _settings;

		public SettingsViewModel(ApplicationSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			_settings = settings;
		}

		public bool CheckForUpdates
		{
			get { return _settings.AutoUpdate.CheckForUpdates; }
			set
			{
				if (value == CheckForUpdates)
					return;

				_settings.AutoUpdate.CheckForUpdates = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public bool AutomaticallyInstallUpdates
		{
			get { return _settings.AutoUpdate.AutomaticallyInstallUpdates; }
			set
			{
				if (value == AutomaticallyInstallUpdates)
					return;

				_settings.AutoUpdate.AutomaticallyInstallUpdates = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public string ProxyUsername
		{
			get { return _settings.AutoUpdate.ProxyUsername; }
			set
			{
				if (value == ProxyUsername)
					return;

				_settings.AutoUpdate.ProxyUsername = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public string ProxyPassword
		{
			get { return _settings.AutoUpdate.ProxyPassword; }
			set
			{
				if (value == ProxyPassword)
					return;

				_settings.AutoUpdate.ProxyPassword = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public string ProxyServer
		{
			get { return _settings.AutoUpdate.ProxyServer; }
			set
			{
				if (value == ProxyServer)
					return;

				_settings.AutoUpdate.ProxyServer = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public string ExportFolder
		{
			get { return _settings.Export.ExportFolder; }
			set
			{
				if (value == ExportFolder)
					return;

				_settings.Export.ExportFolder = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public bool AlwaysOnTop
		{
			get { return _settings.MainWindow.AlwaysOnTop; }
			set
			{
				if (value == _settings.MainWindow.AlwaysOnTop)
					return;

				_settings.MainWindow.AlwaysOnTop = value;
				EmitPropertyChanged();

				var app = Application.Current;
				var window = app.MainWindow;
				if (window != null)
				{
					_settings.MainWindow.RestoreTo(window);
				}

				_settings.SaveAsync();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}