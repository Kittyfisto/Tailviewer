using System.Windows.Media;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.BusinessLogic.Analysis.Analysers.Count;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.Count
{
	public sealed class EntryCountWidgetFactory
		: IWidgetFactory
	{
		public LogAnalyserFactoryId AnalyserId => LogEntryCountAnalyserFactory.Id;

		public ILogAnalyserConfiguration DefaultConfiguration => null;

		public string Name => "Log Entry Count";

		public string Description => "Counts the number of log entries matching a filter expression";

		public Geometry Icon => null;

		public IWidgetViewModel Create(IDataSourceAnalyser dataSourceAnalyser)
		{
			return new EntryCountWidgetViewModel(dataSourceAnalyser);
		}
	}
}
