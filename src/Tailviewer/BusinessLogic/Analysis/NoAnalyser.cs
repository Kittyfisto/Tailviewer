using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Analysis
{
	public sealed class NoAnalyser
		: IDataSourceAnalyser
	{
		private readonly AnalyserId _id;
		private readonly AnalyserPluginId _pluginId;

		public NoAnalyser()
		{
			_id = AnalyserId.Empty;
		}

		public NoAnalyser(AnalyserPluginId pluginId)
			: this()
		{
			_pluginId = pluginId;
		}

		public AnalyserId Id => _id;

		public AnalyserPluginId AnalyserPluginId => _pluginId;

		public Percentage Progress => Percentage.HundredPercent;

		public ILogAnalysisResult Result => null;

		public bool IsFrozen => false;

		public ILogAnalyserConfiguration Configuration { get; set; }
		public void OnLogFileAdded(DataSourceId id, ILogFile logFile)
		{}

		public void OnLogFileRemoved(DataSourceId id, ILogFile logFile)
		{}

		public void Dispose()
		{
		}
	}
}