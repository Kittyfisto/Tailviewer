using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Represents an ongoing analysis (or a snapshot thereof).
	///     An analysis produces results for various aspects of one or more log file(s)
	///     where each aspect is analysed by a single analyser
	///     (this interface groups all of them together).
	/// </summary>
	public interface IAnalysis
	{
		/// <summary>
		///     A globally unique identifier for this group.
		///     No two groups have the same id.
		/// </summary>
		/// <remarks>
		///     Even snapshots have a different id than the group they were created from.
		/// </remarks>
		AnalysisId Id { get; }

		/// <summary>
		///     The current list of analysers.
		/// </summary>
		IEnumerable<IDataSourceAnalyser> Analysers { get; }

		/// <summary>
		///     The current list of log files being analyses by the <see cref="Analysers" />.
		/// </summary>
		IEnumerable<ILogFile> LogFiles { get; }

		/// <summary>
		///     The combined progress of all analysers.
		///     If there are none, then <see cref="Percentage.HundredPercent" /> is returned.
		/// </summary>
		Percentage Progress { get; }

		/// <summary>
		///     Whether or not this analyser group is frozen.
		///     A frozen analyser may not be modified and thus
		///     adding / removing is not allowed then.
		/// </summary>
		bool IsFrozen { get; }

		/// <summary>
		/// </summary>
		/// <param name="logFile"></param>
		void Add(ILogFile logFile);

		/// <summary>
		/// </summary>
		/// <param name="logFile"></param>
		void Remove(ILogFile logFile);

		/// <summary>
		/// </summary>
		/// <param name="pluginId"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		IDataSourceAnalyser Add(AnalyserPluginId pluginId, ILogAnalyserConfiguration configuration);

		/// <summary>
		/// </summary>
		/// <param name="analyser"></param>
		void Remove(IDataSourceAnalyser analyser);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="analyserId"></param>
		/// <param name="analyser"></param>
		/// <returns></returns>
		bool TryGetAnalyser(AnalyserId analyserId, out IDataSourceAnalyser analyser);
	}
}