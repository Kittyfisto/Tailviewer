using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Analysis
{
	public sealed class DataSourceAnalysisConfiguration
	{
		/// <summary>
		///     The id of the <see cref="ILogAnalyserFactory" /> to use.
		/// </summary>
		public LogAnalyserFactoryId AnalyserId { get; set; }

		/// <summary>
		///     The configuration forwarded to the <see cref="ILogAnalyser" />.
		/// </summary>
		public ILogAnalyserConfiguration Configuration { get; set; }
	}
}