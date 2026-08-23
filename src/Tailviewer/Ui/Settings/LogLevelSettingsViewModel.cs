using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Tailviewer.Api;
using Tailviewer.Settings;
using Tailviewer.Ui;

namespace Tailviewer.Ui.Settings
{
	public sealed class LogLevelSettingsViewModel
		: INotifyPropertyChanged
	{
		private readonly IApplicationSettings _settings;
		private readonly LogLevelSettings _logLevelSettings;
		private readonly LevelFlags _level;

		public LogLevelSettingsViewModel(IApplicationSettings settings, LogLevelSettings logLevelSettings, LevelFlags level)
		{
			_settings = settings;
			_logLevelSettings = logLevelSettings;
			_level = level;

			ThemeManager.ThemeChanged += OnThemeChanged;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public Color ForegroundColor
		{
			get
			{
				return _logLevelSettings.IsCustom
					? _logLevelSettings.ForegroundColor
					: LogLevelDefaults.For(ThemeManager.CurrentLogLevelPalette).Get(_level).ForegroundColor;
			}
			set
			{
				if (_logLevelSettings.IsCustom)
				{
					if (value == _logLevelSettings.ForegroundColor)
						return;

					_logLevelSettings.ForegroundColor = value;
					EmitPropertyChanged();
					_settings.SaveAsync();
				}
				else
				{
					var defaults = LogLevelDefaults.For(ThemeManager.CurrentLogLevelPalette).Get(_level);
					_logLevelSettings.ForegroundColor = defaults.ForegroundColor;
					_logLevelSettings.BackgroundColor = defaults.BackgroundColor;
					_logLevelSettings.ForegroundColor = value;
					_logLevelSettings.IsCustom = true;
					EmitPropertyChanged();
					_settings.SaveAsync();
				}
			}
		}

		public Color BackgroundColor
		{
			get
			{
				return _logLevelSettings.IsCustom
					? _logLevelSettings.BackgroundColor
					: LogLevelDefaults.For(ThemeManager.CurrentLogLevelPalette).Get(_level).BackgroundColor;
			}
			set
			{
				if (_logLevelSettings.IsCustom)
				{
					if (value == _logLevelSettings.BackgroundColor)
						return;

					_logLevelSettings.BackgroundColor = value;
					EmitPropertyChanged();
					_settings.SaveAsync();
				}
				else
				{
					var defaults = LogLevelDefaults.For(ThemeManager.CurrentLogLevelPalette).Get(_level);
					_logLevelSettings.ForegroundColor = defaults.ForegroundColor;
					_logLevelSettings.BackgroundColor = defaults.BackgroundColor;
					_logLevelSettings.BackgroundColor = value;
					_logLevelSettings.IsCustom = true;
					EmitPropertyChanged();
					_settings.SaveAsync();
				}
			}
		}

		private void OnThemeChanged(bool darkMode)
		{
			EmitPropertyChanged(nameof(ForegroundColor));
			EmitPropertyChanged(nameof(BackgroundColor));
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
