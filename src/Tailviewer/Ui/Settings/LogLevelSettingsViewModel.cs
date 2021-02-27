using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Settings
{
	public sealed class LogLevelSettingsViewModel
		: INotifyPropertyChanged
	{
		private readonly IApplicationSettings _settings;
		private readonly LogLevelSettings _logLevelSettings;

		public LogLevelSettingsViewModel(IApplicationSettings settings, LogLevelSettings logLevelSettings)
		{
			_settings = settings;
			_logLevelSettings = logLevelSettings;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public Color ForegroundColor
		{
			get { return _logLevelSettings.ForegroundColor; }
			set
			{
				if (value == _logLevelSettings.ForegroundColor)
					return;

				_logLevelSettings.ForegroundColor = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}
		public Color BackgroundColor
		{
			get { return _logLevelSettings.BackgroundColor; }
			set
			{
				if (value == _logLevelSettings.BackgroundColor)
					return;

				_logLevelSettings.BackgroundColor = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}