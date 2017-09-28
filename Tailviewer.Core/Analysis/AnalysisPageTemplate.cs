using System.Collections.Generic;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     The template for an entire page of an analysis:
	///     Maintains the page's layout and all widgets placed on that page.
	/// </summary>
	public sealed class AnalysisPageTemplate
		: IAnalysisPageTemplate
	{
		private readonly List<WidgetTemplate> _widgets;
		private IWidgetLayoutTemplate _layout;

		/// <summary>
		///     Initializes this template.
		/// </summary>
		public AnalysisPageTemplate()
		{
			_widgets = new List<WidgetTemplate>();
		}

		/// <inheritdoc />
		public IEnumerable<IWidgetTemplate> Widgets => _widgets;

		/// <inheritdoc />
		public IWidgetLayoutTemplate Layout
		{
			get { return _layout; }
			set { _layout = value; }
		}

		/// <inheritdoc />
		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Layout", _layout);
			writer.WriteAttribute("Widgets", _widgets);
		}

		/// <inheritdoc />
		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Layout", out _layout);
			reader.TryReadAttribute("Widgets", _widgets);
		}
	}
}