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
		public string Author => _description.Author;
		public string Description => _description.Description;
		public Uri Website => _description.Website;
	}
}