using System;
using System.Collections.Generic;
using Tailviewer.Plugins;

namespace Tailviewer.Core.Sources
{
	/// <summary>
	///     Responsible for creating log sources.
	/// </summary>
	public interface ILogSourceFactory
	{
		/// <summary>
		/// </summary>
		IReadOnlyList<ICustomDataSourcePlugin> CustomDataSources { get; }

		/// <summary>
		///     Creates a new log file object which interprets the given file as a windows event log (ETW) file.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		ILogSource CreateEventLogFile(string fileName);

		/// <summary>
		///     Creates a new instance of the Tailviewer.Core.LogFiles.FilteredLogFile type.
		/// </summary>
		/// <param name="maximumWaitTime"></param>
		/// <param name="source"></param>
		/// <param name="filter"></param>
		/// <returns></returns>
		ILogSource CreateFilteredLogFile(TimeSpan maximumWaitTime,
		                                 ILogSource source,
		                                 ILogEntryFilter filter);

		/// <summary>
		///     Creates a new instance of the Tailviewer.Core.LogFiles.LogFileProxy type.
		/// </summary>
		/// <param name="maximumWaitTime"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		ILogSourceProxy CreateLogFileProxy(TimeSpan maximumWaitTime, ILogSource source);

		/// <summary>
		///     Creates a new instance of the Tailviewer.Core.LogFiles.MergedLogFile type.
		/// </summary>
		/// <param name="maximumWaitTime"></param>
		/// <param name="sources"></param>
		/// <returns></returns>
		IMergedLogFile CreateMergedLogFile(TimeSpan maximumWaitTime, IEnumerable<ILogSource> sources);

		/// <summary>
		///     Creates a new instance of the Tailviewer.Core.LogFiles.MultiLineLogFile type.
		/// </summary>
		/// <param name="maximumWaitTime"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		ILogSource CreateMultiLineLogFile(TimeSpan maximumWaitTime, ILogSource source);

		/// <summary>
		///     Creates a new log file to represents the given file.
		/// </summary>
		/// <param name="filePath">The full file path to the file to be opened.</param>
		/// <returns></returns>
		ILogSource Open(string filePath);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		ILogSource CreateCustom(CustomDataSourceId id,
		                        ICustomDataSourceConfiguration configuration);
	}
}