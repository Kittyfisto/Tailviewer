using System.Windows;
using System.Windows.Media;
using Tailviewer.Analysis.Count.BusinessLogic;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Templates.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Analysis.Count.Ui
{
	public sealed class LogEntryCountWidgetPlugin
		: IWidgetPlugin
	{
		public AnalyserPluginId AnalyserId => LogEntryCountAnalyserPlugin.Id;

		public ILogAnalyserConfiguration DefaultAnalyserConfiguration => new LogEntryCountAnalyserConfiguration();

		public IWidgetConfiguration DefaultViewConfiguration => new LogEntryCountWidgetConfiguration();

		public string Name => "Log Entry Count";

		public string Description => "Counts the number of log entries matching a filter expression";

		public Geometry Icon => null;

		public IWidgetViewModel CreateViewModel(IServiceContainer services, IWidgetTemplate template, IDataSourceAnalyser dataSourceAnalyser)
		{
			return new LogEntryCountWidgetViewModel(services, template, dataSourceAnalyser);
		}

		public FrameworkElement CreateContentPresenterFor(IServiceContainer services, IWidgetViewModel viewModel)
		{
			return new LogEntryCountWidgetControl
			{
				DataContext = viewModel
			};
		}
	}
}
