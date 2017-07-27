using System;
using Tailviewer.Core.Plugins;

namespace Tailviewer.Ui.Controls.Plugins
{
	public sealed class PluginViewModel
	{
		private readonly IPluginDescription _description;

		public PluginViewModel(IPluginDescription description)
		{
			_description = description;
		}

		public string Name => _description.Name;
		public string Author => _description.Author ?? "N/A";
		public string DescriptionOrError => _description.Error != null ? "The plugin failed to be loaded" : _description.Description;
		public Uri Website => _description.Website;
	}
}