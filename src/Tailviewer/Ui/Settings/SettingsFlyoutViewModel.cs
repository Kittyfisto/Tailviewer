using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using log4net;
using Metrolib;
using Ookii.Dialogs.Wpf;
using Tailviewer.Api;
using Tailviewer.Settings;
using Tailviewer.Localization;
using Tailviewer.Ui.Settings.CustomFormats;

namespace Tailviewer.Ui.Settings
{
	public sealed class SettingsFlyoutViewModel
		: IFlyoutViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly IReadOnlyList<EncodingViewModel> Encodings;

		static SettingsFlyoutViewModel()
		{
			Encodings = new[]
			{
				// TODO: Move to service
				new EncodingViewModel(null, Strings.AutoDetect),
				new EncodingViewModel(Encoding.Default),
				new EncodingViewModel(Encoding.ASCII),
				new EncodingViewModel(Encoding.UTF8),
				new EncodingViewModel(Encoding.UTF7),
				new EncodingViewModel(Encoding.UTF32),
				new EncodingViewModel(Encoding.BigEndianUnicode),
				new EncodingViewModel(Encoding.Unicode)
			};
		}

		public IReadOnlyList<LanguageViewModel> Languages { get; } = new[]
		{
			new LanguageViewModel("en", Tailviewer.Localization.Strings.English),
			new LanguageViewModel("zh-CN", Tailviewer.Localization.Strings.ChineseSimplified)
		};

		private LanguageViewModel _language;
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
		private readonly ICommand _restoreDefaultColorsCommand;

		public SettingsFlyoutViewModel(IApplicationSettings applicationSettings,
		                                  IServiceContainer serviceContainer)
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

			_otherLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Other, LevelFlags.Other);
			_traceLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Trace, LevelFlags.Trace);
			_debugLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Debug, LevelFlags.Debug);
			_infoLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Info, LevelFlags.Info);
			_warnLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Warning, LevelFlags.Warning);
			_errorLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Error, LevelFlags.Error);
			_fatalLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Fatal, LevelFlags.Fatal);
			_customFormats = new CustomFormatsSettingsViewModel(_settings, serviceContainer, Encodings);
			_restoreDefaultColorsCommand = new DelegateCommand2(RestoreDefaultColors);
			_language = Languages.FirstOrDefault(x => string.Equals(x.Code, applicationSettings.Ui.Language, StringComparison.InvariantCultureIgnoreCase)) ?? Languages.First();
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

		public IReadOnlyList<ThemeModeOption> ThemeModes { get; } = new[]
		{
			new ThemeModeOption(ThemeMode.Light, Strings.ThemeModeLight),
			new ThemeModeOption(ThemeMode.Dark, Strings.ThemeModeDark),
			new ThemeModeOption(ThemeMode.System, Strings.ThemeModeSystem)
		};

		public ThemeMode ThemeMode
		{
			get { return _settings.Ui.ThemeMode; }
			set
			{
				if (value == _settings.Ui.ThemeMode)
					return;

				_settings.Ui.ThemeMode = value;
				EmitPropertyChanged();

				_settings.SaveAsync();

				// Defer the theme application until the selection has finished updating,
				// otherwise mutating the application resources during the toggle's own
				// callback re-enters the layout/rendering pass.
				var dispatcher = Application.Current?.Dispatcher;
				if (dispatcher != null)
				{
					dispatcher.BeginInvoke(new Action(() => ThemeManager.Apply(ThemeColor, value)));
				}
				else
				{
					ThemeManager.Apply(ThemeColor, value);
				}
			}
		}

		public Color ThemeColor
		{
			get { return _settings.Ui.ThemeColor; }
			set
			{
				if (value == _settings.Ui.ThemeColor)
					return;

				_settings.Ui.ThemeColor = value;
				EmitPropertyChanged();

				_settings.SaveAsync();

				// Defer the theme application until the color picker has finished updating,
				// otherwise mutating the application resources during the picker's own
				// selection callback re-enters the layout/rendering pass.
				var dispatcher = Application.Current?.Dispatcher;
				if (dispatcher != null)
				{
					dispatcher.BeginInvoke(new Action(() => ThemeManager.Apply(value, _settings.Ui.ThemeMode)));
				}
				else
				{
					ThemeManager.Apply(value, _settings.Ui.ThemeMode);
				}
			}
		}

		public ICommand RestoreDefaultColorsCommand => _restoreDefaultColorsCommand;

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

		public LanguageViewModel Language
		{
			get { return _language; }
			set
			{
				if (Equals(value, _language))
				return;

				_language = value;
				_settings.Ui.Language = value?.Code;
				EmitPropertyChanged();
				_settings.SaveAsync();
			}
		}

		public void Update()
		{}

		public string Name => Strings.MenuSettings;

		private void ChooseExportFolder()
		{
			var dialog = new VistaFolderBrowserDialog
			{
				SelectedPath = ExportFolder,
				Description = Strings.ChooseExportFolder,
				UseDescriptionForTitle = true
			};

			if (dialog.ShowDialog() == true)
			{
				ExportFolder = dialog.SelectedPath;
			}
		}

		private void RestoreDefaultColors()
		{
			var themeColorChanged = _settings.Ui.ThemeColor != UISettings.DefaultThemeColor;
			_settings.Ui.ThemeColor = UISettings.DefaultThemeColor;
			_settings.LogViewer.RestoreDefaultColors();
			if (themeColorChanged)
				EmitPropertyChanged(nameof(ThemeColor));

			_settings.SaveAsync();

			// Re-apply even when the accent color was already the default: resetting
			// custom log colors still needs to refresh the visible log viewer and the
			// color pickers through ThemeChanged.
			var dispatcher = Application.Current?.Dispatcher;
			Action applyTheme = () => ThemeManager.Apply(UISettings.DefaultThemeColor, _settings.Ui.ThemeMode);
			if (dispatcher != null)
			{
				dispatcher.BeginInvoke(applyTheme);
			}
			else
			{
				applyTheme();
			}
		}

		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
