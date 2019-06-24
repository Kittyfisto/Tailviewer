using System.Windows;
using System.Windows.Media;
using Tailviewer.Analysis.QuickInfo.BusinessLogic;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Templates.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Analysis.QuickInfo.Ui
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

		public IWidgetViewModel CreateViewModel(IServiceContainer services, IWidgetTemplate template, IDataSourceAnalyser dataSourceAnalyser)
		{
			return new QuickInfoWidgetViewModel(services, template, dataSourceAnalyser);
		}

		public FrameworkElement CreateContentPresenterFor(IServiceContainer services, IWidgetViewModel viewModel)
		{
			return new QuickInfoWidgetControl
			{
				DataContext = viewModel
			};
		}
	}
}