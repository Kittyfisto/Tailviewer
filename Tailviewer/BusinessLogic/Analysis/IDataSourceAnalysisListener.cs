using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Analysis
{
	public interface IDataSourceAnalysisListener
	{
		void OnProgress(IDataSourceAnalysisHandle handle, Percentage progress);

		void OnAnalysisResultChanged(IDataSourceAnalysisHandle handle, ILogAnalysisResult result);
	}
}