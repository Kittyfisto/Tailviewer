using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	/// </summary>
	/// <remarks>
	///    Version 1
	///    Initial definition
	/// </remarks>
	/// <remarks>
	///    Version 2
	///    Breaking changes:
	///    - Added ITaskScheduler parameter to Create method
	/// </remarks>
	[PluginInterfaceVersion(2)]
	public interface IDataSourceAnalyserPlugin
		: IPlugin
	{
		/// <summary>
		///     A unique id which distinguishes this plugin implementation from any other.
		/// </summary>
		AnalyserPluginId Id { get; }

		/// <summary>
		///     Creates a new data source analyser.
		/// </summary>
		/// <remarks>
		///     A data source analyser is similar to a <see cref="ILogAnalyser" />, however contrary to the former,
		///     a <see cref="IDataSourceAnalyser" /> instance is re-used when the <see cref="ILogAnalyserConfiguration" /> changes
		///     and its <see cref="IDataSourceAnalyser.Configuration" /> is simply changed.
		///     This fact may make implementing the <see cref="IDataSourceAnalyser" /> interface more complicated than
		///     <see cref="ILogAnalyser" />
		///     (which is why the latter exists), but allows analysis data to be carried over in between changes.
		/// </remarks>
		/// <param name="id">TODO: Remove this parameter</param>
		/// <param name="scheduler"></param>
		/// <param name="logFile"/>
		/// <param name="configuration"></param>
		/// <returns></returns>
		IDataSourceAnalyser Create(AnalyserId id, ITaskScheduler scheduler, ILogFile logFile, ILogAnalyserConfiguration configuration);
	}
}