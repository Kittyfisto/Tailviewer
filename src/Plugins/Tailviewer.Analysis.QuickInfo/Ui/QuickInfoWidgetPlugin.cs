using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.QuickInfo.BusinessLogic;
using Tailviewer.Templates.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.QuickInfo.Ui
{
	public sealed class QuickInfoWidgetPlugin
		: IWidgetPlugin
	{
		public AnalyserPluginId AnalyserId => QuickInfoAnalyserPlugin.Id;

		public ILogAnalyserConfiguration DefaultAnalyserConfiguration => new QuickInfoAnalyserConfiguration();

		public IWidgetConfiguration DefaultViewConfiguration => new QuickInfoWidgetConfiguration();

		public string Name => "Quick Info";

		public string Description => "Obtain a quick overview by displaying extracts of matching log entries";

		public Geometry Icon => null;

		public IWidgetViewModel CreateViewModel(IWidgetTemplate template, IDataSourceAnalyser dataSourceAnalyser)
		{
			return new QuickInfoWidgetViewModel(template, dataSourceAnalyser);
		}

		public FrameworkElement CreateContentPresenterFor(IWidgetViewModel viewModel)
		{
			return new QuickInfoWidgetControl
			{
				DataContext = viewModel
			};
		}
	}
}