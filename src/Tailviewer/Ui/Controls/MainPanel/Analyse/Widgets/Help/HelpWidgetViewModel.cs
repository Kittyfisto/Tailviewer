using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;
using Tailviewer.Templates.Analysis;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.Help
{
	public sealed class HelpWidgetViewModel
		: AbstractWidgetViewModel
	{
		public HelpWidgetViewModel(IServiceContainer services, IWidgetTemplate template, IDataSourceAnalyser dataSourceAnalyser)
			: base(services, template, dataSourceAnalyser)
		{
			Title = "Help";
		}
	}
}