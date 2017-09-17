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
		private readonly Func<IWidgetViewModel> _factory;

		public WidgetFactoryViewModel(
			Func<IWidgetViewModel> factory,
			string name,
			string description,
			Geometry icon = null)
		{
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (description == null)
				throw new ArgumentNullException(nameof(description));

			_factory = factory;
			Name = name;
			Description = description;
			Icon = icon;
		}

		public string Name { get; }
		public string Description { get; }
		public Geometry Icon { get; }

		public override string ToString()
		{
			return string.Format("WidgetFactory: {0}", Name);
		}

		public IWidgetViewModel Create()
		{
			return _factory();
		}
	}
}