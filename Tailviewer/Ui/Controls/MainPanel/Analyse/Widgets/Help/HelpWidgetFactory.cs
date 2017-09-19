using System;
using System.Windows.Media;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Analysis.Analysers;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.Help
{
	public sealed class HelpWidgetFactory
		: IWidgetFactory
	{
		public Type AnalyserType => null;

		public ILogAnalyserConfiguration DefaultConfiguration => null;

		public string Name => "Tutorial";

		public string Description => "Describes the first steps after having created an analysis";

		public Geometry Icon => null;

		public IWidgetViewModel Create(IDataSourceAnalyser dataSourceAnalyser)
		{
			return new HelpWidgetViewModel(dataSourceAnalyser);
		}
	}
}
