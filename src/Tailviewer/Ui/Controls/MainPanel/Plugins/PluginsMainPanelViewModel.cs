using System.Collections.Generic;
using System.Linq;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.SidePanel;

namespace Tailviewer.Ui.Controls.MainPanel.Plugins
{
	public sealed class PluginsMainPanelViewModel
		: AbstractMainPanelViewModel
	{
		private readonly IReadOnlyList<PluginViewModel> _plugins;

		public PluginsMainPanelViewModel(IApplicationSettings applicationSettings,
										 IEnumerable<IPluginDescription> plugins)
			: base(applicationSettings)
		{
			_plugins = plugins.Select(x => new PluginViewModel(x)).ToList();
		}

		public override IEnumerable<ISidePanelViewModel> SidePanels => Enumerable.Empty<ISidePanelViewModel>();

		public IEnumerable<PluginViewModel> Plugins => _plugins;
		public string PluginPath => Constants.PluginPath;
		public bool HasPlugins => _plugins.Count > 0;

		public override void Update()
		{
		}
	}
}