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
		/// <param name="buffer"></param>
		public static void GetColumn<T>(this ILogFile logFile,
		                                LogFileSection section,
		                                ILogFileColumn<T> column,
		                                T[] buffer)
		{
			logFile.GetColumn(section, column, buffer, 0);
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
		/// <param name="indices"></param>
		/// <param name="column"></param>
		/// <param name="buffer"></param>
		public static void GetColumn<T>(this ILogFile logFile,
										IReadOnlyList<LogLineIndex> indices,
										ILogFileColumn<T> column,
		                                T[] buffer)
		{
			logFile.GetColumn(indices, column, buffer, 0);
		}

		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="logFile"></param>
		/// <param name="indices"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		[Pure]
		public static T[] GetColumn<T>(this ILogFile logFile, IReadOnlyList<LogLineIndex> indices, ILogFileColumn<T> column)
		{
			if (indices == null)
				throw new ArgumentNullException(nameof(indices));

			var cells = new T[indices.Count];
			logFile.GetColumn(indices, column, cells);
			return cells;
		}

		/// <summary>
		///     Retrieves all entries from the given <paramref name="section" /> from this log file and copies
		///     them into the given <paramref name="buffer" />.
		/// </summary>
		/// <remarks>
		///     TODO: Move this method into the <see cref="ILogFile"/> interface
		/// </remarks>
		/// <param name="logFile"></param>
		/// <param name="section"></param>
		/// <param name="buffer"></param>
		/// <param name="destinationIndex"></param>
		public static void GetEntries(this ILogFile logFile, LogFileSection section, ILogEntries buffer, int destinationIndex)
		{
			foreach (var column in buffer.Columns)
				buffer.CopyFrom(column, destinationIndex, logFile, section);
		}

		/// <summary>
		///     Retrieves all entries from the given <paramref name="section" /> from this log file and copies
		///     them into the given <paramref name="buffer" />.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="section"></param>
		/// <param name="buffer"></param>
		public static void GetEntries(this ILogFile logFile, LogFileSection section, ILogEntries buffer)
		{
			GetEntries(logFile, section, buffer, 0);
		}

		/// <summary>
		///     Retrieves all entries for the given <paramref name="columns" /> of the given <paramref name="section" />
		///     from this log file.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="section"></param>
		/// <param name="columns"></param>
		/// <returns></returns>
		public static IReadOnlyLogEntries GetEntries(this ILogFile logFile, LogFileSection section, params ILogFileColumn[] columns)
		{
			var buffer = new LogEntryBuffer(section.Count, columns);
			GetEntries(logFile, section, buffer);
			return buffer;
		}

		/// <summary>
		///     Retrieves all entries for the given <paramref name="columns" /> of the given <paramref name="section" />
		///     from this log file.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="section"></param>
		/// <param name="columns"></param>
		/// <returns></returns>
		public static IReadOnlyLogEntries GetEntries(this ILogFile logFile, LogFileSection section, IEnumerable<ILogFileColumn> columns)
		{
			var buffer = new LogEntryBuffer(section.Count, columns);
			GetEntries(logFile, section, buffer);
			return buffer;
		}

		/// <summary>
		///     Retrieves all entries with the given <paramref name="indices"/> from this log file and copies
		///     them into the given <paramref name="buffer"/>.
		/// </summary>
		/// <remarks>
		///     TODO: Move this method into the <see cref="ILogFile"/> interface
		/// </remarks>
		/// <param name="logFile"></param>
		/// <param name="indices"></param>
		/// <param name="buffer"></param>
		public static void GetEntries(this ILogFile logFile, IReadOnlyList<LogLineIndex> indices, ILogEntries buffer)
		{
			foreach (var column in buffer.Columns)
				buffer.CopyFrom(column, 0, logFile, indices);
		}

		/// <summary>
		///     Retrieves all entries with the given <paramref name="columns" /> of the given <paramref name="indices" />
		///     from this log file.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="indices"></param>
		/// <param name="columns"></param>
		/// <returns></returns>
		public static IReadOnlyLogEntries GetEntries(this ILogFile logFile, IReadOnlyList<LogLineIndex> indices, params ILogFileColumn[] columns)
		{
			var buffer = new LogEntryBuffer(indices.Count);
			logFile.GetEntries(indices, buffer);
			return buffer;
		}

		/// <summary>
		///     Retrieves all entries with the given <paramref name="columns" /> of the given <paramref name="indices" />
		///     from this log file.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="indices"></param>
		/// <param name="columns"></param>
		/// <returns></returns>
		public static IReadOnlyLogEntries GetEntries(this ILogFile logFile, IReadOnlyList<LogLineIndex> indices, IEnumerable<ILogFileColumn> columns)
		{
			var buffer = new LogEntryBuffer(indices.Count);
			logFile.GetEntries(indices, buffer);
			return buffer;
		}

		/// <summary>
		///     Retrieves a list of log lines from this log file.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="section"></param>
		/// <returns></returns>
		[Pure]
		[WillBeRemoved("LogLine will be removed and so will this method sometime in 2018",
			"https://github.com/Kittyfisto/Tailviewer/issues/143")]
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