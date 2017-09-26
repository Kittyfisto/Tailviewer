using System.Windows.Media;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.Core;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.Help
{
	public sealed class HelpWidgetFactory
		: IWidgetFactory
	{
		public LogAnalyserFactoryId AnalyserId => LogAnalyserFactoryId.Empty;

		public ILogAnalyserConfiguration DefaultAnalyserConfiguration => null;

		public IWidgetConfiguration DefaultViewConfiguration => null;

		public string Name => "Tutorial";

		public string Description => "Describes the first steps after having created an analysis";

		public Geometry Icon => null;

		public IWidgetViewModel Create(IDataSourceAnalyser dataSourceAnalyser, IWidgetConfiguration configuration)
		{
			return new HelpWidgetViewModel(dataSourceAnalyser);
		}
	}
}
