namespace Tailviewer.BusinessLogic.Analysis
{
	public sealed class NoAnalyser
		: IDataSourceAnalyser
	{
		private readonly AnalyserId _id;

		public NoAnalyser()
		{
			_id = AnalyserId.CreateNew();
		}

		public AnalyserId Id => _id;

		public Percentage Progress => Percentage.HundredPercent;

		public ILogAnalysisResult Result => null;

		public bool IsFrozen => false;

		public ILogAnalyserConfiguration Configuration { get; set; }
	}
}