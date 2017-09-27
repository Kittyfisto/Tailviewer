using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.Help
{
	public sealed class HelpWidgetViewModel
		: AbstractWidgetViewModel
	{
		public HelpWidgetViewModel(IDataSourceAnalyser dataSourceAnalyser)
			: base(dataSourceAnalyser, null)
		{
			Title = "Help";
		}
	}
}