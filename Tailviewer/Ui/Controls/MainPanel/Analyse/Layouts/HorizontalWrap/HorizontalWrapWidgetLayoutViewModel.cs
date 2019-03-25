using Tailviewer.Core.Analysis;
using Tailviewer.Core.Analysis.Layouts;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts.HorizontalWrap
{
	/// <summary>
	///     View model representing the horizontal wrap layout: Widgets are sized as small as possible
	///     and are laid our left to right, top to bottom.
	///     Only exists so WPF picks the right data template to display widgets.
	/// </summary>
	public sealed class HorizontalWrapWidgetLayoutViewModel
		: AbstractWidgetLayoutViewModel
	{
		private readonly HorizontalWidgetLayoutTemplate _template;

		public HorizontalWrapWidgetLayoutViewModel(HorizontalWidgetLayoutTemplate template)
		{
			_template = template;
		}

		public override IWidgetLayoutTemplate Template => _template;
	}
}