using System.Windows.Media;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Analysis.Analysers;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.Help
{
	public sealed class HelpWidgetFactory
		: IWidgetFactory
	{
		public LogAnalyserFactoryId AnalyserId => LogAnalyserFactoryId.None;

		public ILogAnalyserConfiguration DefaultAnalyserConfiguration => null;

		public string Name => "Tutorial";

		public string Description => "Describes the first steps after having created an analysis";

		public Geometry Icon => null;

		public IWidgetViewModel Create(IDataSourceAnalyser dataSourceAnalyser)
		{
			return new HelpWidgetViewModel(dataSourceAnalyser);
		}
	}
}
