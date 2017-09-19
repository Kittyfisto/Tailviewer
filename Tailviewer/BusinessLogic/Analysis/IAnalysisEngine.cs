using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Responsible for creating and executing <see cref="IDataSourceAnalysis" />.
	/// </summary>
	/// <remarks>
	///     An analysis is executing for as long as the engine exists or until it is removed through
	///     <see cref="RemoveAnalysis" />.
	/// </remarks>
	public interface IAnalysisEngine
	{
		/// <summary>
		/// Creates a new analysis for the given data source.
		/// </summary>
		/// <param name="dataSource"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		IDataSourceAnalysis CreateAnalysis(IDataSource dataSource, DataSourceAnalysisConfiguration configuration);

		void RemoveAnalysis(IDataSourceAnalysis analysis);
	}
}