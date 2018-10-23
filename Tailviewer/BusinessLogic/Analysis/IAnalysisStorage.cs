using System.Collections.Generic;
using System.Threading.Tasks;
using Tailviewer.Core.Analysis;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Maintains the list of active analyses.
	///     Can be used to create new analyses, to store existing ones or to retrieve the list of analyses.
	/// </summary>
	public interface IAnalysisStorage
	{
		/// <summary>
		///    Returns the current list of all active analyses.
		/// </summary>
		IEnumerable<ActiveAnalysisConfiguration> AnalysisTemplates { get; }

		/// <summary>
		/// 
		/// </summary>
		IEnumerable<IAnalysis> Analyses { get; }

		/// <summary>
		///     Saves the given active analysis to disk.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task Save(AnalysisId id);

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="analysis"></param>
		/// <returns></returns>
		bool TryGetAnalysisFor(AnalysisId id, out IAnalysis analysis);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="analysisId"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		bool TryGetTemplateFor(AnalysisId analysisId, out ActiveAnalysisConfiguration configuration);

		/// <summary>
		///     Creates a new analysis from the given template.
		///     Changes to the analysis will reflect back onto the given template instance.
		/// </summary>
		/// <paramref name="template" />
		/// <paramref name="viewTemplate" />
		/// <returns></returns>
		IAnalysis CreateAnalysis(AnalysisTemplate template, AnalysisViewTemplate viewTemplate);

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		void Remove(AnalysisId id);

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