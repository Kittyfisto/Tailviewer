using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Responsible for creating and executing <see cref="IDataSourceAnalysisHandle" />.
	/// </summary>
	/// <remarks>
	///     An analysis is executing for as long as the engine exists or until it is removed through
	///     <see cref="RemoveAnalysis" />.
	/// </remarks>
	public interface IAnalysisEngine
	{
		/// <summary>
		///     Creates a new analysis for the given data source.
		///     The analysis will examine the data source and react to changes until the analysis
		///     is removed via <see cref="RemoveAnalysis" />.
		/// </summary>
		/// <remarks>
		///     DO NOT FORGET TO CALL <see cref="IDataSourceAnalysisHandle.Start" />. NOTHING WILL BE DONE BEFORE.
		/// </remarks>
		/// <param name="logFile"></param>
		/// <param name="configuration"></param>
		/// <param name="listener">The listener which is notified of changes</param>
		/// <returns></returns>
		IDataSourceAnalysisHandle CreateAnalysis(ILogFile logFile,
			DataSourceAnalysisConfiguration configuration,
			IDataSourceAnalysisListener listener);

		/// <summary>
		///     Removes the given analysis from this engine, if it was created via <see cref="CreateAnalysis" />.
		/// </summary>
		/// <param name="analysis"></param>
		bool RemoveAnalysis(IDataSourceAnalysisHandle analysis);
	}
}