using Tailviewer.Core.Analysis;
using Tailviewer.Core.Analysis.Layouts;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts.Column
{
	/// <summary>
	///     View model representing the column layout: Widgets are laid out left to right, each equally sized.
	///     Only exists so WPF picks the right data template to display widgets.
	/// </summary>
	public sealed class ColumnWidgetLayoutViewModel
		: AbstractWidgetLayoutViewModel
	{
		private readonly ColumnWidgetLayoutTemplate _template;

		public ColumnWidgetLayoutViewModel(ColumnWidgetLayoutTemplate template)
		{
			_template = template;
		}

		public override IWidgetLayoutTemplate Template => _template;
	}
}