using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Filters;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Extension methods to the <see cref="ILogFile" /> interface.
	/// </summary>
	public static class LogFileExtensions
	{
		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="logFile"></param>
		/// <param name="section"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		[Pure]
		public static T[] GetColumn<T>(this ILogFile logFile, LogFileSection section, ILogFileColumn<T> column)
		{
			var cells = new T[section.Count];
			logFile.GetColumn(section, column, cells);
			return cells;
		}

		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="logFile"></param>
		/// <param name="section"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		[Pure]
		public static T[] GetColumn<T>(this ILogFile logFile, IReadOnlyList<LogLineIndex> section, ILogFileColumn<T> column)
		{
			var cells = new T[section.Count];
			logFile.GetColumn(section, column, cells);
			return cells;
		}

		/// <summary>
		///     Retrieves a list of log lines from this log file.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="section"></param>
		/// <returns></returns>
		[Pure]
		[WillBeRemoved("LogLine will be removed and so will this method sometime in 2018", "https://github.com/Kittyfisto/Tailviewer/issues/143")]
		public static LogLine[] GetSection(this ILogFile logFile, LogFileSection section)
		{
			var entries = new LogLine[section.Count];
			logFile.GetSection(section, entries);
			return entries;
		}

		/// <summary>
		///     Creates a filtered view onto this log file.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="scheduler"></param>
		/// <param name="logLineFilter"></param>
		/// <param name="logEntryFilter"></param>
		/// <returns></returns>
		public static FilteredLogFile AsFiltered(this ILogFile logFile,
		                                         ITaskScheduler scheduler,
		                                         ILogLineFilter logLineFilter,
		                                         ILogEntryFilter logEntryFilter)
		{
			return AsFiltered(logFile, scheduler, logLineFilter, logEntryFilter, TimeSpan.FromMilliseconds(value: 10));
		}

		/// <summary>
		///     Creates a filtered view onto this log file.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="scheduler"></param>
		/// <param name="logLineFilter"></param>
		/// <param name="logEntryFilter"></param>
		/// <param name="maximumWaitTime"></param>
		/// <returns></returns>
		public static FilteredLogFile AsFiltered(this ILogFile logFile,
		                                         ITaskScheduler scheduler,
		                                         ILogLineFilter logLineFilter,
		                                         ILogEntryFilter logEntryFilter,
		                                         TimeSpan maximumWaitTime)
		{
			var file = new FilteredLogFile(scheduler, maximumWaitTime, logFile, logLineFilter, logEntryFilter);
			return file;
		}
	}
}