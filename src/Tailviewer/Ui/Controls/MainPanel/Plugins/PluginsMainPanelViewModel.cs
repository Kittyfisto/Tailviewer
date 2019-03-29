using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Metrolib;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.SidePanel;

namespace Tailviewer.Ui.Controls.MainPanel.Plugins
{
	/// <summary>
	///     Represents the plugin page (displays the list of plugins, etc...).
	/// </summary>
	public sealed class PluginsMainPanelViewModel
		: AbstractMainPanelViewModel
	{
		private readonly IReadOnlyList<PluginViewModel> _plugins;
		private readonly DelegateCommand2 _openPluginFolderCommand;

		public PluginsMainPanelViewModel(IApplicationSettings applicationSettings,
			IEnumerable<IPluginDescription> plugins)
			: base(applicationSettings)
		{
			_plugins = plugins.Select(x => new PluginViewModel(x)).ToList();
			_openPluginFolderCommand = new DelegateCommand2(OpenPluginFolder);
		}

		public override IEnumerable<ISidePanelViewModel> SidePanels => Enumerable.Empty<ISidePanelViewModel>();
		public ICommand OpenPluginFolderCommand => _openPluginFolderCommand;
		public IEnumerable<PluginViewModel> Plugins => _plugins;
		public string PluginPath => Constants.PluginPath;
		public bool HasPlugins => _plugins.Count > 0;

		public override void Update()
		{
		}

		private void OpenPluginFolder()
		{
			string argument = string.Format(@"{0}", Constants.PluginPath);
			Process.Start("explorer.exe", argument);
		}
	}
}