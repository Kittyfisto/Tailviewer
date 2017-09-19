using System;
using System.Windows.Media;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets;

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
		private readonly IWidgetFactory _factory;

		public WidgetFactoryViewModel(
			IWidgetFactory factory)
		{
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			_factory = factory;
		}

		public string Name => _factory.Name;
		public string Description => _factory.Description;
		public Geometry Icon => _factory.Icon;

		public override string ToString()
		{
			return string.Format("WidgetFactory: {0}", Name);
		}

		public IWidgetFactory Factory => _factory;
	}
}