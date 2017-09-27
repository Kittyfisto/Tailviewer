using System;
using System.Windows.Media;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels
{
	/// <summary>
	///     This view model represents a type of widget to the user:
	///     In the end the user is expected to chose between many factories
	///     when adding new widgets (and thus name, description and icon should
	///     be as descriptive as possible).
	/// </summary>
	public sealed class WidgetFactoryViewModel
	{
		private readonly IWidgetPlugin _plugin;

		public WidgetFactoryViewModel(
			IWidgetPlugin plugin)
		{
			if (plugin == null)
				throw new ArgumentNullException(nameof(plugin));

			_plugin = plugin;
		}

		public string Name => _plugin.Name;
		public string Description => _plugin.Description;
		public Geometry Icon => _plugin.Icon;

		public override string ToString()
		{
			return string.Format("WidgetFactory: {0}", Name);
		}

		public IWidgetPlugin Plugin => _plugin;
	}
}