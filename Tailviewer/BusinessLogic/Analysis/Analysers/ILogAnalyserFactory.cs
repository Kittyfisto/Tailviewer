using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.BusinessLogic.Analysis.Analysers
{
	/// <summary>
	///     The interface for a factory to create <see cref="ILogAnalyser" /> instances that perform
	///     a specific analysis on a log file.
	/// </summary>
	/// <remarks>
	/// TODO: Should this interface be part of the plugin architecture? If yes, then how exactly?
	///       Is it good enough to just inherit from <see cref="IPlugin"/>?
	///       Can we move all dependant types to the Tailviewer.Api project?
	/// </remarks>
	public interface ILogAnalyserFactory
	{
		/// <summary>
		///     A (hopefully) globally unique id that describes this factory.
		///     It is advisable that this id be human-readable as well, be it just for debugging purposes.
		/// </summary>
		LogAnalyserFactoryId Id { get; }

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
		/// <param name="scheduler"></param>
		/// <param name="source"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		ILogAnalyser Create(ITaskScheduler scheduler,
			ILogFile source,
			ILogAnalyserConfiguration configuration);
	}
}