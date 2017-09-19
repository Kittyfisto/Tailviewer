using System;
using System.Windows.Media;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.BusinessLogic.Analysis.Analysers.Count;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.LineCount
{
	public sealed class LineCountWidgetFactory
		: IWidgetFactory
	{
		public Type AnalyserType => typeof(CountLogAnalyser);

		public ILogAnalyserConfiguration DefaultConfiguration => null;

		public string Name => "Line Count";

		public string Description => "Counts the number of lines matching a filter expression";

		public Geometry Icon => null;

		public IWidgetViewModel Create(IDataSourceAnalyser dataSourceAnalyser)
		{
			return new LineCountWidgetViewModel(dataSourceAnalyser);
		}
	}
}
