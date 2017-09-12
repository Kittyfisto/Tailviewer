using System;
using System.Windows.Input;
using System.Windows.Media;
using Metrolib;
using Tailviewer.Archiver.Plugins;

namespace Tailviewer.Ui.Controls.MainPanel.Plugins
{
	public sealed class PluginViewModel
	{
		private readonly IPluginDescription _description;
		private readonly DelegateCommand _deleteCommand;

		public PluginViewModel(IPluginDescription description)
		{
			_description = description;
			//_deleteCommand = new DelegateCommand();
		}

		public string Name => _description.Name;
		public Version Version => _description.Version;
		public string Author => _description.Author ?? "N/A";
		public string Description => _description.Description;
		public string DescriptionOrError => _description.Error != null ? "The plugin failed to be loaded" : _description.Description;
		public Uri Website => _description.Website;
		public ImageSource Icon => _description.Icon;
		public ICommand DeleteCommand => _deleteCommand;
	}
}