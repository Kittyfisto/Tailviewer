using System.Collections.Generic;
using System.Collections.ObjectModel;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Plugins;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls.MainPanel.Settings.CustomFormats
{
	/// <summary>
	///     Represents the current list of known custom log file formats and allows a user to edit them.
	/// </summary>
	public sealed class CustomFormatsSettingsViewModel
	{
		private readonly IApplicationSettings _settings;
		private readonly ObservableCollection<CustomFormatPluginViewModel> _plugins;

		public CustomFormatsSettingsViewModel(IApplicationSettings settings,
		                                      IServiceContainer serviceContainer,
		                                      IReadOnlyList<EncodingViewModel> textFileEncodings)
		{
			var pluginLoader = serviceContainer.TryRetrieve<IPluginLoader>();
			_settings = settings;
			_plugins = new ObservableCollection<CustomFormatPluginViewModel>();

			if (pluginLoader != null)
			{
				var plugins = pluginLoader.LoadAllOfTypeWithDescription<ICustomLogFileFormatCreatorPlugin>();
				foreach (var pair in plugins)
				{
					if (pair.Plugin != null)
						_plugins.Add(new CustomFormatPluginViewModel(serviceContainer,
						                                             _settings,
						                                             pair.Description.Id,
						                                             pair.Plugin,
						                                             textFileEncodings));
				}
			}
		}

		public IEnumerable<CustomFormatPluginViewModel> Plugins
		{
			get { return _plugins; }
		}
	}
}