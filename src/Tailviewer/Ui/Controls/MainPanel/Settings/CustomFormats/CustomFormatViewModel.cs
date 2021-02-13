using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using log4net;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.LogFileFormats;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Settings;
using Tailviewer.Settings.CustomFormats;

namespace Tailviewer.Ui.Controls.MainPanel.Settings.CustomFormats
{
	public sealed class CustomFormatViewModel
		: INotifyPropertyChanged
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IServiceContainer _serviceContainer;
		private readonly IApplicationSettings _settings;
		private readonly ILogFileFormatRegistry _logFileFormatRegistry;
		private readonly PluginId _pluginId;
		private readonly ICustomLogFileFormatCreatorPlugin _plugin;
		private readonly IEnumerable<EncodingViewModel> _encodings;
		private CustomLogFileFormat _customFormat;
		private string _name;
		private string _format;
		private EncodingViewModel _encoding;
		private string _errorMessage;

		public CustomFormatViewModel(IServiceContainer serviceContainer,
		                             IApplicationSettings settings,
		                             ICustomLogFileFormatCreatorPlugin plugin,
		                             IEnumerable<EncodingViewModel> encodings,
		                             CustomLogFileFormat customFormat)
		{
			_serviceContainer = serviceContainer;
			_settings = settings;
			_logFileFormatRegistry = serviceContainer.Retrieve<ILogFileFormatRegistry>();
			_pluginId = customFormat.PluginId;
			_plugin = plugin;
			_encodings = encodings;
			_customFormat = customFormat;
			_name = customFormat.Name;
			_encoding = Encodings.FirstOrDefault(x => Equals(x.Encoding, customFormat.Encoding));
			_format = customFormat.Format;
		}

		public string Name
		{
			get { return _name; }
			set
			{
				if (value == _name)
					return;

				_name = value;
				EmitPropertyChanged();
			}
		}

		public string Format
		{
			get { return _format; }
			set
			{
				if (value == _format)
					return;

				_format = value;
				EmitPropertyChanged();

				TryCreateFormat();
				Save();
			}
		}

		public EncodingViewModel Encoding
		{
			get { return _encoding; }
			set
			{
				if (value == _encoding)
					return;

				_encoding = value;
				EmitPropertyChanged();

				TryCreateFormat();
				Save();
			}
		}

		public IEnumerable<EncodingViewModel> Encodings => _encodings;

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

		public ICommand RemoveCommand { get; set; }

		public CustomLogFileFormat CustomFormat => _customFormat;

		public event PropertyChangedEventHandler PropertyChanged;

		private void TryCreateFormat()
		{
			try
			{
				var newCustomFormat = new CustomLogFileFormat(_pluginId, _name, _format, _encoding?.Encoding);
				if (_plugin.TryCreate(_serviceContainer, newCustomFormat, out var newLogFileFormat))
				{
					if (newLogFileFormat == null)
					{
						Log.WarnFormat("Plugin '{0}' claimed to have created a new custom log file format, but didn't!", _plugin);
					}
				}

				ReplaceCustomFormat(newCustomFormat, newLogFileFormat);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Caught exception while parsing format '{0}': {1}", _format, e);
				_logFileFormatRegistry.Remove(_customFormat);
			}
		}

		private void ReplaceCustomFormat(CustomLogFileFormat newCustomFormat, ILogFileFormat newLogFileFormat)
		{
			_logFileFormatRegistry.Replace(_customFormat, newCustomFormat, newLogFileFormat);
			_settings.CustomFormats.Remove(_customFormat);
			_settings.CustomFormats.Add(newCustomFormat);
			_customFormat = newCustomFormat;
		}

		private void Save()
		{
			_settings.SaveAsync();
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}