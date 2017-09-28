using System.Collections.Generic;

namespace Tailviewer.Settings.Analysis
{
	/// <summary>
	///     The template for an entire page of an analysis:
	///     Maintains the page's layout and all widgets placed on that page.
	/// </summary>
	public sealed class AnalysisPageTemplate
		: ISerializableType
	{
		private readonly List<WidgetTemplate> _widgets;
		private IWidgetLayoutTemplate _layout;

		public AnalysisPageTemplate()
		{
			_widgets = new List<WidgetTemplate>();
		}

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Layout", _layout);
			writer.WriteAttribute("Widgets", _widgets);
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Layout", out _layout);
			reader.TryReadAttribute("Widgets", _widgets);
		}
	}
}