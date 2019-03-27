using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Templates.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.Help
{
	public sealed class HelpWidgetPlugin
		: IWidgetPlugin
	{
		public AnalyserPluginId AnalyserId => AnalyserPluginId.Empty;

		public ILogAnalyserConfiguration DefaultAnalyserConfiguration => null;

		public IWidgetConfiguration DefaultViewConfiguration => null;

		public string Name => "Tutorial";

		public string Description => "Describes the first steps after having created an analysis";

		public Geometry Icon => null;

		public IWidgetViewModel CreateViewModel(IWidgetTemplate template, IDataSourceAnalyser dataSourceAnalyser)
		{
			return new HelpWidgetViewModel(template, dataSourceAnalyser);
		}

		public FrameworkElement CreateContentPresenterFor(IWidgetViewModel viewModel)
		{
			return new HelpWidgetControl
			{
				DataContext = viewModel
			};
		}
	}
}
