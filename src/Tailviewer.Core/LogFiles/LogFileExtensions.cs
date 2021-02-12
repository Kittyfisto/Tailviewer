using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;

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
		/// <param name="sourceSection"></param>
		/// <param name="column"></param>
		/// <param name="destination"></param>
		public static void GetColumn<T>(this ILogFile logFile,
		                                LogFileSection sourceSection,
		                                ILogFileColumn<T> column,
		                                T[] destination)
		{
			logFile.GetColumn(sourceSection, column, destination, 0);
		}

		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="logFile"></param>
		/// <param name="sourceSection"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		[Pure]
		public static T[] GetColumn<T>(this ILogFile logFile, LogFileSection sourceSection, ILogFileColumn<T> column)
		{
			var cells = new T[sourceSection.Count];
			logFile.GetColumn(sourceSection, column, cells);
			return cells;
		}

		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="logFile"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="column"></param>
		/// <param name="destination"></param>
		public static void GetColumn<T>(this ILogFile logFile,
										IReadOnlyList<LogLineIndex> sourceIndices,
										ILogFileColumn<T> column,
										T[] destination)
		{
			logFile.GetColumn(sourceIndices, column, destination, 0);
		}

		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="logFile"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		[Pure]
		public static T[] GetColumn<T>(this ILogFile logFile, IReadOnlyList<LogLineIndex> sourceIndices, ILogFileColumn<T> column)
		{
			if (sourceIndices == null)
				throw new ArgumentNullException(nameof(sourceIndices));

			var cells = new T[sourceIndices.Count];
			logFile.GetColumn(sourceIndices, column, cells);
			return cells;
		}

		/// <summary>
		///     Retrieves all entries from the given <paramref name="sourceSection" /> from this log file and copies
		///     them into the given <paramref name="destination" />.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="sourceSection"></param>
		/// <param name="destination"></param>
		public static void GetEntries(this ILogFile logFile, LogFileSection sourceSection, ILogEntries destination)
		{
			logFile.GetEntries(sourceSection, destination, 0);
		}

		/// <summary>
		///     Retrieves all entries for all columns of this log file of the given <paramref name="sourceSection" />:
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="sourceSection"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyLogEntries GetEntries(this ILogFile logFile, LogFileSection sourceSection)
		{
			var buffer = new LogEntryBuffer(sourceSection.Count, logFile.Columns);
			GetEntries(logFile, sourceSection, buffer);
			return buffer;
		}

		/// <summary>
		///     Retrieves all entries for the given <paramref name="columns" /> of the given <paramref name="sourceSection" />
		///     from this log file.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="sourceSection"></param>
		/// <param name="columns"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyLogEntries GetEntries(this ILogFile logFile, LogFileSection sourceSection, IEnumerable<ILogFileColumn> columns)
		{
			var buffer = new LogEntryBuffer(sourceSection.Count, columns);
			GetEntries(logFile, sourceSection, buffer);
			return buffer;
		}

		/// <summary>
		///     Retrieves all entries for all columns of this log file of the given <paramref name="sourceIndices" />:
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="sourceIndices"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyLogEntries GetEntries(this ILogFile logFile, IReadOnlyList<LogLineIndex> sourceIndices)
		{
			var buffer = new LogEntryBuffer(sourceIndices.Count, logFile.Columns);
			GetEntries(logFile, sourceIndices, buffer);
			return buffer;
		}

		/// <summary>
		///     Retrieves all entries with the given <paramref name="sourceIndices"/> from this log file and copies
		///     them into the given <paramref name="destination"/>.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="destination"></param>
		public static void GetEntries(this ILogFile logFile, IReadOnlyList<LogLineIndex> sourceIndices, ILogEntries destination)
		{
			logFile.GetEntries(sourceIndices, destination, 0);
		}

		/// <summary>
		///     Retrieves all entries with the given <paramref name="columns" /> of the given <paramref name="sourceIndices" />
		///     from this log file.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="columns"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyLogEntries GetEntries(this ILogFile logFile, IReadOnlyList<LogLineIndex> sourceIndices, IEnumerable<ILogFileColumn> columns)
		{
			var buffer = new LogEntryBuffer(sourceIndices.Count, columns);
			logFile.GetEntries(sourceIndices, buffer);
			return buffer;
		}

		/// <summary>
		///     Retrieves the entry with the given <paramref name="columns" /> at the given <paramref name="sourceIndex" />
		///     from this log file.
		/// </summary>
		/// <remarks>
		///     Do NOT use this method if you want to retrieve multiple entries. Use any overload of ILogFile.GetEntries instead.
		/// </remarks>
		/// <param name="logFile"></param>
		/// <param name="sourceIndex"></param>
		/// <param name="columns"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyLogEntry GetEntry(this ILogFile logFile, LogLineIndex sourceIndex, IEnumerable<ILogFileColumn> columns)
		{
			var buffer = new LogEntryBuffer(1, columns);
			logFile.GetEntries(new LogFileSection(sourceIndex, 1), buffer);
			return buffer[0];
		}

		/// <summary>
		///     Retrieves the entry with all columns of the log file at the given <paramref name="sourceIndex" />.
		/// </summary>
		/// <remarks>
		///     Do NOT use this method if you want to retrieve multiple entries. Use any overload of ILogFile.GetEntries instead.
		/// </remarks>
		/// <param name="logFile"></param>
		/// <param name="sourceIndex"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyLogEntry GetEntry(this ILogFile logFile, LogLineIndex sourceIndex)
		{
			var buffer = new LogEntryBuffer(1, logFile.Columns);
			logFile.GetEntries(new LogFileSection(sourceIndex, 1), buffer);
			return buffer[0];
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
		internal static FilteredLogFile AsFiltered(this ILogFile logFile,
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
		internal static FilteredLogFile AsFiltered(this ILogFile logFile,
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