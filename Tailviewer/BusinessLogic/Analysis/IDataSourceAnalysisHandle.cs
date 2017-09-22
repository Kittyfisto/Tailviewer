namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Represents the contuous analysis of a data source.
	/// </summary>
	public interface IDataSourceAnalysisHandle
	{
		/// <summary>
		///     Actually starts the analysis.
		/// </summary>
		void Start();
	}
}