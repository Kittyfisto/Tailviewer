using Tailviewer.Core.Analysis;
using Tailviewer.Core.Analysis.Layouts;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts.Row
{
	/// <summary>
	///     View model representing the column layout: Widgets are laid out top to bottom, each equally sized.
	///     Only exists so WPF picks the right data template to display widgets.
	/// </summary>
	public sealed class RowWidgetLayoutViewModel
		: AbstractWidgetLayoutViewModel
	{
		private readonly RowWidgetLayoutTemplate _template;

		public RowWidgetLayoutViewModel(RowWidgetLayoutTemplate template)
		{
			_template = template;
		}

		public override IWidgetLayoutTemplate Template => _template;
	}
}