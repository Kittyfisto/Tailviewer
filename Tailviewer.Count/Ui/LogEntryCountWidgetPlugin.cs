using System.Windows.Media;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Count.BusinessLogic;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Count.Ui
{
	public sealed class LogEntryCountWidgetPlugin
		: IWidgetPlugin
	{
		public LogAnalyserFactoryId AnalyserId => LogEntryCountAnalyserPlugin.Id;

		public ILogAnalyserConfiguration DefaultAnalyserConfiguration => new LogEntryCountAnalyserConfiguration();

		public IWidgetConfiguration DefaultViewConfiguration => null;

		public string Name => "Log Entry Count";

		public string Description => "Counts the number of log entries matching a filter expression";

		public Geometry Icon => null;

		public IWidgetViewModel CreateViewModel(IDataSourceAnalyser dataSourceAnalyser, IWidgetConfiguration configuration)
		{
			return new EntryCountWidgetViewModel(dataSourceAnalyser);
		}
	}
}
