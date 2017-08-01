using System.Collections.Generic;
using System.Linq;
using Tailviewer.Archiver.Plugins;

namespace Tailviewer.Ui.Controls.Plugins
{
	public sealed class PluginsViewModel
	{
		private readonly IReadOnlyList<PluginViewModel> _plugins;

		public PluginsViewModel(IEnumerable<IPluginDescription> plugins)
		{
			_plugins = plugins.Select(x => new PluginViewModel(x)).ToList();
		}

		public IEnumerable<PluginViewModel> Plugins => _plugins;
		public bool HasPlugins => _plugins.Count > 0;
	}
}