using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Analysis;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Responsible for creating and maintaining a list of <see cref="IDataSourceAnalyser" />s.
	/// </summary>
	public interface IDataSourceAnalyserEngine
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="template"></param>
		/// <returns></returns>
		IDataSourceAnalyser CreateAnalyser(ILogFile logFile, AnalyserTemplate template);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="analyser"></param>
		void RemoveAnalyser(IDataSourceAnalyser analyser);
	}
}