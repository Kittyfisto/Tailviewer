using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Analysis.Analysers;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.Help
{
	public sealed class HelpWidgetViewModel
		: AbstractWidgetViewModel
	{
		public HelpWidgetViewModel(IDataSourceAnalyser dataSourceAnalyser)
			: base(dataSourceAnalyser)
		{
			Title = "Help";
		}

		protected override ILogAnalyserConfiguration Configuration
		{
			get { throw new System.NotImplementedException(); }
		}

		public override void OnUpdate()
		{
			
		}
	}
}