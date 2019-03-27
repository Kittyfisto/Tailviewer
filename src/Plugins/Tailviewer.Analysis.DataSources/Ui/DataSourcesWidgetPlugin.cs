using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.DataSources.BusinessLogic;
using Tailviewer.Templates.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.DataSources.Ui
{
	public sealed class DataSourcesWidgetPlugin
		: IWidgetPlugin
	{
		public LogAnalyserFactoryId LogAnalyserId => DataSourcesAnalyserPlugin.Id;

		public DataSourceAnalyserPluginId DataSourceAnalyserId => DataSourceAnalyserPluginId.Empty;

		public ILogAnalyserConfiguration DefaultAnalyserConfiguration => null;

		public IWidgetConfiguration DefaultViewConfiguration => null;

		public string Name => "Data sources";

		public string Description => "Displays information about the selected data sources in a concise view";

		public Geometry Icon => null;

		public IWidgetViewModel CreateViewModel(IWidgetTemplate template, IDataSourceAnalyser dataSourceAnalyser)
		{
			return new DataSourcesWidgetViewModel(template, dataSourceAnalyser);
		}

		public FrameworkElement CreateContentPresenterFor(IWidgetViewModel viewModel)
		{
			return new DataSourcesWidget
			{
				DataContext = viewModel
			};
		}
	}
}
