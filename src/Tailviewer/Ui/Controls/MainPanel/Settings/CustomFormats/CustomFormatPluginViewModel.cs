using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Metrolib;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Settings;
using Tailviewer.Settings.CustomFormats;

namespace Tailviewer.Ui.Controls.MainPanel.Settings.CustomFormats
{
	public sealed class CustomFormatPluginViewModel
	{
		private readonly IServiceContainer _serviceContainer;
		private readonly IApplicationSettings _settings;
		private readonly string _name;
		private readonly ICommand _addCommand;
		private readonly ObservableCollection<CustomFormatViewModel> _formats;
		private readonly PluginId _pluginId;
		private readonly ICustomLogFileFormatCreatorPlugin _plugin;
		private readonly IEnumerable<EncodingViewModel> _encodings;

		public CustomFormatPluginViewModel(IServiceContainer serviceContainer,
		                                   IApplicationSettings settings,
		                                   PluginId pluginId,
		                                   ICustomLogFileFormatCreatorPlugin plugin,
		                                   IEnumerable<EncodingViewModel> encodings)
		{
			_serviceContainer = serviceContainer;
			_settings = settings;
			_pluginId = pluginId;
			_plugin = plugin;
			_encodings = encodings;
			_name = plugin.FormatName;
			_addCommand = new DelegateCommand2(Add);
			_formats = new ObservableCollection<CustomFormatViewModel>();

			foreach (var customFormat in _settings.CustomFormats)
			{
				Add(customFormat);
			}
		}

		private void Add()
		{
			var customFormat = new CustomLogFileFormat(_pluginId,
			                                           $"My custom {_name} format",
			                                           null,
			                                           _encodings.First().Encoding);
			Add(customFormat);
			_settings.CustomFormats.Add(customFormat);
			_settings.SaveAsync();
		}

		private void Add(CustomLogFileFormat customFormat)
		{
			var viewModel = new CustomFormatViewModel(_serviceContainer,
			                                          _settings,
			                                          _plugin,
			                                          _encodings,
			                                          customFormat);
			viewModel.RemoveCommand = new DelegateCommand2(() => Remove(viewModel));
			_formats.Add(viewModel);
		}

		private void Remove(CustomFormatViewModel viewModel)
		{
			_formats.Remove(viewModel);
		}

		public string Name => _name;

		public ICommand AddCommand => _addCommand;

		public IEnumerable<CustomFormatViewModel> Formats
		{
			get { return _formats; }
		}
	}
}