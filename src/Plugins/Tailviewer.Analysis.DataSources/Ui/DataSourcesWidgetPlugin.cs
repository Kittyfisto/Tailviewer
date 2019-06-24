using System.Windows;
using System.Windows.Media;
using Tailviewer.Analysis.DataSources.BusinessLogic;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Templates.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Analysis.DataSources.Ui
{
	public sealed class DataSourcesWidgetPlugin
		: IWidgetPlugin
	{
		public AnalyserPluginId AnalyserId => DataSourcesAnalyserPlugin.Id;

		public ILogAnalyserConfiguration DefaultAnalyserConfiguration => null;

		public IWidgetConfiguration DefaultViewConfiguration => new DataSourcesWidgetConfiguration();

		public string Name => "Data sources";

		public string Description => "Displays information about the selected data sources in a concise view";

		public Geometry Icon => null;

		public IWidgetViewModel CreateViewModel(IServiceContainer services, IWidgetTemplate template, IDataSourceAnalyser dataSourceAnalyser)
		{
			return new DataSourcesWidgetViewModel(services, template, dataSourceAnalyser);
		}

		public FrameworkElement CreateContentPresenterFor(IServiceContainer services, IWidgetViewModel viewModel)
		{
			return new DataSourcesWidget
			{
				DataContext = viewModel
			};
		}
	}
}
