using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     The interface for a factory to create <see cref="ILogAnalyser" /> instances that perform
	///     a specific analysis on a log file.
	/// </summary>
	/// <remarks>
	///    Version 1
	///    Initial definition
	/// </remarks>
	/// <remarks>
	///    Version 2
	///    Breaking changes:
	///    - Changed Create() signature to accept an <see cref="IServiceContainer"/> instead of a <see cref="ITaskScheduler"/>
	/// </remarks>
	[PluginInterfaceVersion(2)]
	public interface ILogAnalyserPlugin
		: IPlugin
	{
		/// <summary>
		///     A (hopefully) globally unique id that describes this factory.
		///     It is advisable that this id be human-readable as well, be it just for debugging purposes.
		/// </summary>
		AnalyserPluginId Id { get; }

		/// <summary>
		///     Creates a new analyser for the <paramref name="source" />.
		///     It is advised that the analyser use the given <see cref="ITaskScheduler" /> to schedule
		///     periodic tasks to actually perform the analysis. The current result of the analysis shall be
		///     accessed via <see cref="ILogAnalyser.Result" />.
		/// </summary>
		/// <remarks>
		///     An implementation should throw appropriate exceptions if the given <paramref name="configuration" />
		///     is unacceptable (for example because the type doesn't fit, or some value is completely out of range).
		/// </remarks>
		/// <param name="services">A service container which may be used to construct types required by this plugin and to inject custom parameters if necessary</param>
		/// <param name="source"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		ILogAnalyser Create(IServiceContainer services,
			ILogFile source,
			ILogAnalyserConfiguration configuration);
	}
}