using System.Collections.Generic;
using System.Threading.Tasks;
using Tailviewer.Core.Analysis;

namespace Tailviewer.BusinessLogic.Analysis
{
	public interface IAnalysisStorage
	{
		/// <summary>
		///     The current list of analyses.
		/// </summary>
		IEnumerable<IAnalysis> Analyses { get; }

		/// <summary>
		///     Creates a new analysis from the given template.
		///     Changes to the analysis will reflect back onto the given template instance.
		/// </summary>
		/// <returns></returns>
		IAnalysis CreateAnalysis(AnalysisTemplate template);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		Task<IReadOnlyList<string>> EnumerateSnapshots();

		/// <summary>
		///     Creates a new snapshot from the current state of the given analysis.
		///     The snapshot will be persisted in the background, but can be accessed
		///     via the returned handle immediately.
		/// </summary>
		/// <param name="analysis"></param>
		/// <param name="viewTemplate"></param>
		/// <returns></returns>
		Task SaveSnapshot(IAnalysis analysis, AnalysisViewTemplate viewTemplate);
	}
}