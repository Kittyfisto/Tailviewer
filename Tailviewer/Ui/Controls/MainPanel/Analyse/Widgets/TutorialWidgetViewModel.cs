namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets
{
	public sealed class TutorialWidgetViewModel
		: AbstractWidgetViewModel
	{
		public TutorialWidgetViewModel()
			: base(canBeEdited: false)
		{
			Title = "Tutorial";
		}
	}
}