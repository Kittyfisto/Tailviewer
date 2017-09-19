﻿using Tailviewer.BusinessLogic.DataSources;

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
		///     Creates a new analysis for the given data source.
		///     The analysis will examine the data source and react to changes until the analysis
		///     is removed via <see cref="RemoveAnalysis" />.
		/// </summary>
		/// <param name="dataSource"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		IDataSourceAnalysis CreateAnalysis(IDataSource dataSource, DataSourceAnalysisConfiguration configuration);

		/// <summary>
		///     Removes the given analysis from this engine, if it was created via <see cref="CreateAnalysis" />.
		/// </summary>
		/// <param name="analysis"></param>
		bool RemoveAnalysis(IDataSourceAnalysis analysis);
	}
}