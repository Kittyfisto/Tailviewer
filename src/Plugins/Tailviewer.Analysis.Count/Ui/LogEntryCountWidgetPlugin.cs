using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Count.BusinessLogic;
using Tailviewer.Templates.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Count.Ui
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

		public IWidgetViewModel CreateViewModel(IWidgetTemplate template, IDataSourceAnalyser dataSourceAnalyser)
		{
			return new LogEntryCountWidgetViewModel(template, dataSourceAnalyser);
		}

		public FrameworkElement CreateContentPresenterFor(IWidgetViewModel viewModel)
		{
			return new LogEntryCountWidgetControl
			{
				DataContext = viewModel
			};
		}
	}
}
