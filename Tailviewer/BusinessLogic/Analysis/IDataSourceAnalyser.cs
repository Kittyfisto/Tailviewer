using Tailviewer.BusinessLogic.Analysis.Analysers;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Represents a continuous analysis of a set of data sources where
	///     the configuration of the analysis may be changed on the fly
	///     (as opposed to <see cref="DataSourceAnalysis" /> which only supports
	///     a static configuration).
	/// </summary>
	public interface IDataSourceAnalyser
	{
		/// <summary>
		///     The result of the analysis.
		///     May change over the lifetime of this analyser.
		///     May be null.
		/// </summary>
		ILogAnalysisResult Result { get; }

		/// <summary>
		///     The current configuration used by the analyser.
		///     When the configuration is changed (by calling the setter),
		///     then the analysis is restarted using the new configuration
		///     and <see cref="Result" /> will eventually represent the result
		///     of the analysis using the new configuration.
		/// </summary>
		ILogAnalyserConfiguration Configuration { get; set; }
	}
}