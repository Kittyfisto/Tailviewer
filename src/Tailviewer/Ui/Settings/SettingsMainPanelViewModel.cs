using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using log4net;
using Metrolib;
using Ookii.Dialogs.Wpf;
using Tailviewer.Settings;
using Tailviewer.Ui.MainPanel;
using Tailviewer.Ui.Settings.CustomFormats;
using Tailviewer.Ui.SidePanel;

namespace Tailviewer.Ui.Settings
{
	public sealed class SettingsMainPanelViewModel
		: AbstractMainPanelViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly IReadOnlyList<EncodingViewModel> Encodings;

		static SettingsMainPanelViewModel()
		{
			Encodings = new[]
			{
				// TODO: Move to service
				new EncodingViewModel(null, "Auto detect"),
				new EncodingViewModel(Encoding.Default),
				new EncodingViewModel(Encoding.ASCII),
				new EncodingViewModel(Encoding.UTF8),
				new EncodingViewModel(Encoding.UTF7),
				new EncodingViewModel(Encoding.UTF32),
				new EncodingViewModel(Encoding.BigEndianUnicode),
				new EncodingViewModel(Encoding.Unicode)
			};
		}

		private readonly IApplicationSettings _settings;
		private readonly LogLevelSettingsViewModel _otherLevel;
		private readonly LogLevelSettingsViewModel _traceLevel;
		private readonly LogLevelSettingsViewModel _debugLevel;
		private readonly LogLevelSettingsViewModel _infoLevel;
		private readonly LogLevelSettingsViewModel _warnLevel;
		private readonly LogLevelSettingsViewModel _errorLevel;
		private readonly LogLevelSettingsViewModel _fatalLevel;
		private string _pluginRepositories;
		private EncodingViewModel _defaultTextFileEncoding;
		private readonly CustomFormatsSettingsViewModel _customFormats;

		public SettingsMainPanelViewModel(IApplicationSettings applicationSettings,
		                                  IServiceContainer serviceContainer)
			: base(applicationSettings)
		{
			_settings = applicationSettings;

			var repos = applicationSettings.AutoUpdate.PluginRepositories;
			_pluginRepositories = repos != null ? string.Join(Environment.NewLine, repos) : string.Empty;

			var defaultEncoding = applicationSettings.LogFile?.DefaultEncoding;
			_defaultTextFileEncoding = Encodings.FirstOrDefault(x => Equals(x.Encoding, defaultEncoding));
			if (_defaultTextFileEncoding == null)
			{
				var @default = TextFileEncodings.FirstOrDefault();
				Log.WarnFormat("Unable to find encoding '{0}', setting default to '{1}'...",  defaultEncoding, @default?.Encoding);

				_defaultTextFileEncoding = @default;
			}

			_otherLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Other);
			_traceLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Trace);
			_debugLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Debug);
			_infoLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Info);
			_warnLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Warning);
			_errorLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Error);
			_fatalLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Fatal);
			_customFormats = new CustomFormatsSettingsViewModel(_settings, serviceContainer, Encodings);
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

		public string PluginRepositories
		{
			get { return _pluginRepositories; }
			set
			{
				_pluginRepositories = value;
				_settings.AutoUpdate.PluginRepositories =
					_pluginRepositories?.Split(new[]{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
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
		
		public int TabWidth
		{
			get { return _settings.LogViewer.TabWidth; }
			set
			{
				if (value == _settings.LogViewer.TabWidth)
					return;

				_settings.LogViewer.TabWidth = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public LogLevelSettingsViewModel OtherLevel => _otherLevel;
		public LogLevelSettingsViewModel TraceLevel => _traceLevel;
		public LogLevelSettingsViewModel DebugLevel => _debugLevel;
		public LogLevelSettingsViewModel InfoLevel => _infoLevel;
		public LogLevelSettingsViewModel WarningLevel => _warnLevel;
		public LogLevelSettingsViewModel ErrorLevel => _errorLevel;
		public LogLevelSettingsViewModel FatalLevel => _fatalLevel;

		public CustomFormatsSettingsViewModel CustomFormats => _customFormats;

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

		public IEnumerable<EncodingViewModel> TextFileEncodings => Encodings;

		public EncodingViewModel DefaultTextFileEncoding
		{
			get { return _defaultTextFileEncoding; }
			set
			{
				if (Equals(value, _defaultTextFileEncoding))
					return;

				_defaultTextFileEncoding = value;
				EmitPropertyChanged();

				_settings.LogFile.DefaultEncoding = value?.Encoding;
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