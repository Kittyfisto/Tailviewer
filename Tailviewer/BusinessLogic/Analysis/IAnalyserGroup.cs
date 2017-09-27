using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Responsible for holding one or more <see cref="IDataSourceAnalyser" />s
	///     as well the list of <see cref="ILogFile" />s they shall analyse.
	/// </summary>
	public interface IAnalyserGroup
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
		/// <param name="analyserId"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		IDataSourceAnalyser Add(LogAnalyserFactoryId analyserId, ILogAnalyserConfiguration configuration);

		/// <summary>
		/// </summary>
		/// <param name="analyser"></param>
		void Remove(IDataSourceAnalyser analyser);
	}
}