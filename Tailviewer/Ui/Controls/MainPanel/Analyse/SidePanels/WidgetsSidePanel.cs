using System.Collections.Generic;
using System.Windows.Media;
using Metrolib;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Count.Ui;
using Tailviewer.QuickInfo.Ui;
using Tailviewer.Ui.Analysis;
using Tailviewer.Ui.Controls.SidePanel;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels
{
	internal sealed class WidgetsSidePanel
		: AbstractSidePanelViewModel
	{
		private readonly List<WidgetFactoryViewModel> _widgets;

		public static readonly string PanelId = "Analysis.Widgets";

		public WidgetsSidePanel(IPluginLoader pluginLoader)
		{
			_widgets = new List<WidgetFactoryViewModel>();
			foreach(var plugin in pluginLoader.LoadAllOfType<IWidgetPlugin>())
			{
				_widgets.Add(new WidgetFactoryViewModel(plugin));
			}
		}

		public IEnumerable<WidgetFactoryViewModel> Widgets => _widgets;

		public override Geometry Icon => Icons.Widgets;

		public override string Id => PanelId;

		public override void Update()
		{
		}
	}
}