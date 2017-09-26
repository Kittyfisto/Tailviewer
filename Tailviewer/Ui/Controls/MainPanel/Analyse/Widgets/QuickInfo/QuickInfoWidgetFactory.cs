using System.Windows.Media;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo;
using Tailviewer.Core;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.QuickInfo
{
	public sealed class QuickInfoWidgetFactory
		: IWidgetFactory
	{
		public LogAnalyserFactoryId AnalyserId => QuickInfoAnalyserFactory.Id;

		public ILogAnalyserConfiguration DefaultAnalyserConfiguration => new QuickInfoAnalyserConfiguration();

		public IWidgetConfiguration DefaultViewConfiguration => new QuickInfoWidgetConfiguration();

		public string Name => "Quick Info";

		public string Description => "Obtain a quick overview by displaying extracts of matching log entries";

		public Geometry Icon => null;

		public IWidgetViewModel Create(IDataSourceAnalyser dataSourceAnalyser, IWidgetConfiguration configuration)
		{
			return new QuickInfoWidgetViewModel(dataSourceAnalyser, configuration as QuickInfoWidgetConfiguration);
		}
	}
}