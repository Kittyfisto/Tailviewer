using Tailviewer.BusinessLogic.Analysis.Analysers;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Represents the contuous analysis of a data source.
	///     The result of the analysis may be queried through <see cref="Result" />.
	/// </summary>
	public interface IDataSourceAnalysis
	{
		ILogAnalysisResult Result { get; }
	}
}