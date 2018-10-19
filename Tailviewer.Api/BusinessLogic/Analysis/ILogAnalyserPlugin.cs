using System;
using System.Collections.Generic;
using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     The interface for a factory to create <see cref="ILogAnalyser" /> instances that perform
	///     a specific analysis on a log file.
	/// </summary>
	public interface ILogAnalyserPlugin
		: IPlugin
	{
		/// <summary>
		///     A (hopefully) globally unique id that describes this factory.
		///     It is advisable that this id be human-readable as well, be it just for debugging purposes.
		/// </summary>
		LogAnalyserFactoryId Id { get; }

		/// <summary>
		///     Types which are expected to be serialized. This must include the <see cref="ILogAnalyserConfiguration"/> implementation
		/// if this plugin ships with one.
		/// </summary>
		/// <remarks>
		///     It is expected that the name for a certain logical type does NOT change over the lifetime of the plugin.
		///     Renaming a certain <see cref="Type"/> is fine, so as long as the name returned in this dictionary does NOT change.
		/// </remarks>
		IEnumerable<KeyValuePair<string, Type>> SerializableTypes { get; }

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