using System.Threading.Tasks;
using Tailviewer.Core.Analysis;

namespace Tailviewer.BusinessLogic.Analysis
{
	public interface IAnalysisStorage
	{
		/// <summary>
		///     Creates a new snapshot from the current state of the given analysis.
		///     The snapshot will be persisted in the background, but can be accessed
		///     via the returned handle immediately.
		/// </summary>
		/// <param name="analysis"></param>
		/// <param name="template"></param>
		/// <returns></returns>
		Task SaveSnapshot(IAnalysis analysis, AnalysisTemplate template);
	}
}