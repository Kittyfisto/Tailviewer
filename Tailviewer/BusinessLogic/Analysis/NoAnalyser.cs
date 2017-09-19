using Tailviewer.BusinessLogic.Analysis.Analysers;

namespace Tailviewer.BusinessLogic.Analysis
{
	public sealed class NoAnalyser
		: IDataSourceAnalyser
	{
		public ILogAnalysisResult Result => null;
		public ILogAnalyserConfiguration Configuration { get; set; }
	}
}