using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Metrolib;
using Ookii.Dialogs.Wpf;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.SidePanel;

namespace Tailviewer.Ui.Controls.MainPanel.Settings
{
	public sealed class SettingsMainPanelViewModel
		: AbstractMainPanelViewModel
	{
		private readonly IApplicationSettings _settings;

		public SettingsMainPanelViewModel(IApplicationSettings applicationSettings) : base(applicationSettings)
		{
			_settings = applicationSettings;
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

		public ICommand ChooseExportFolderCommand
		{
			get { return new DelegateCommand2(ChooseExportFolder); }
		}

		public int ScrollSpeed
		{
			get { return _settings.LogViewer.LinesScrolledPerWheelTick; }
			set
			{
				if (value == _settings.LogViewer.LinesScrolledPerWheelTick)
					return;

				_settings.LogViewer.LinesScrolledPerWheelTick = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public int FontSize
		{
			get { return _settings.LogViewer.FontSize; }
			set
			{
				if (value == _settings.LogViewer.FontSize)
					return;

				_settings.LogViewer.FontSize = value;
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

		public bool FolderDataSourceRecursive
		{
			get { return _settings.DataSources.FolderDataSourceRecursive; }
			set
			{
				if (value == _settings.DataSources.FolderDataSourceRecursive)
					return;

				_settings.DataSources.FolderDataSourceRecursive = value;
				_settings.SaveAsync();
			}
		}

		public string FolderDataSourcePatterns
		{
			get { return _settings.DataSources.FolderDataSourcePattern; }
			set
			{
				if (string.Equals(_settings.DataSources.FolderDataSourcePattern, value))
					return;

				_settings.DataSources.FolderDataSourcePattern = value;
				_settings.SaveAsync();
			}
		}

		public override IEnumerable<ISidePanelViewModel> SidePanels => Enumerable.Empty<ISidePanelViewModel>();

		public override void Update()
		{}

		private void ChooseExportFolder()
		{
			var dialog = new VistaFolderBrowserDialog
			{
				SelectedPath = ExportFolder,
				Description = "Choose export folder",
				UseDescriptionForTitle = true
			};

			if (dialog.ShowDialog() == true)
			{
				ExportFolder = dialog.SelectedPath;
			}
		}
	}
}