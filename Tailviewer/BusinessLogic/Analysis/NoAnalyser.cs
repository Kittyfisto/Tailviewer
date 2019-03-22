namespace Tailviewer.BusinessLogic.Analysis
{
	public sealed class NoAnalyser
		: IDataSourceAnalyser
	{
		private readonly AnalyserId _id;
		private readonly LogAnalyserFactoryId _factoryId;

		public NoAnalyser()
		{
			_id = AnalyserId.Empty;
		}

		public NoAnalyser(LogAnalyserFactoryId factoryId)
			: this()
		{
			_factoryId = factoryId;
		}

		public AnalyserId Id => _id;

		public LogAnalyserFactoryId FactoryId => _factoryId;

		public Percentage Progress => Percentage.HundredPercent;

		public ILogAnalysisResult Result => null;

		public bool IsFrozen => false;

		public ILogAnalyserConfiguration Configuration { get; set; }
	}
}