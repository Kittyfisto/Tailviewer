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
		///     Returns all properties from this log file.
		/// </summary>
		/// <returns></returns>
		public static ILogFileProperties GetAllProperties(this ILogFile that)
		{
			var destination = new LogFilePropertyList();
			that.GetAllProperties(destination);
			return destination;
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
										IColumnDescriptor<T> column,
										T[] destination)
		{
			logFile.GetColumn(sourceIndices, column, destination, 0, LogFileQueryOptions.Default);
		}

		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="logFile"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="column"></param>
		/// <param name="destination"></param>
		/// <param name="queryOptions">Configures how the data is to be retrieved</param>
		public static void GetColumn<T>(this ILogFile logFile,
		                                IReadOnlyList<LogLineIndex> sourceIndices,
		                                IColumnDescriptor<T> column,
		                                T[] destination,
		                                LogFileQueryOptions queryOptions)
		{
			logFile.GetColumn(sourceIndices, column, destination, 0, queryOptions);
		}

		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="logFile"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="column"></param>
		/// <param name="destination"></param>
		/// <param name="destinationIndex">Configures how the data is to be retrieved</param>
		public static void GetColumn<T>(this ILogFile logFile,
		                                IReadOnlyList<LogLineIndex> sourceIndices,
		                                IColumnDescriptor<T> column,
		                                T[] destination,
		                                int destinationIndex)
		{
			logFile.GetColumn(sourceIndices, column, destination, destinationIndex, LogFileQueryOptions.Default);
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
		public static T[] GetColumn<T>(this ILogFile logFile, IReadOnlyList<LogLineIndex> sourceIndices, IColumnDescriptor<T> column)
		{
			return logFile.GetColumn<T>(sourceIndices, column, LogFileQueryOptions.Default);
		}

		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="logFile"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="column"></param>
		/// <param name="queryOptions">Configures how the data is to be retrieved</param>
		/// <returns></returns>
		[Pure]
		public static T[] GetColumn<T>(this ILogFile logFile, IReadOnlyList<LogLineIndex> sourceIndices, IColumnDescriptor<T> column, LogFileQueryOptions queryOptions)
		{
			if (sourceIndices == null)
				throw new ArgumentNullException(nameof(sourceIndices));

			var cells = new T[sourceIndices.Count];
			logFile.GetColumn(sourceIndices, column, cells, queryOptions);
			return cells;
		}

		/// <summary>
		///     Retrieves all entries for all columns of this log file.
		/// </summary>
		/// <remarks>
		///     This fetches the entire log file into memory.
		///     KNOW what you're doing.
		/// </remarks>
		/// <param name="logFile"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyLogEntries GetEntries(this ILogFile logFile)
		{
			return logFile.GetEntries(logFile.Columns);
		}

		/// <summary>
		///     Retrieves all entries for all columns of this log file.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="columns"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyLogEntries GetEntries(this ILogFile logFile, IReadOnlyList<IColumnDescriptor> columns)
		{
			var count = logFile.GetProperty(Properties.LogEntryCount);
			var buffer = new LogEntryArray(count, columns);
			GetEntries(logFile, new LogFileSection(0, count), buffer);
			return buffer;
		}

		/// <summary>
		///     Retrieves all entries for all columns of this log file of the given <paramref name="sourceIndices" />.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="sourceIndices"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyLogEntries GetEntries(this ILogFile logFile, IReadOnlyList<LogLineIndex> sourceIndices)
		{
			var buffer = new LogEntryArray(sourceIndices.Count, logFile.Columns);
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
			logFile.GetEntries(sourceIndices, destination, 0, LogFileQueryOptions.Default);
		}

		/// <summary>
		///     Retrieves all entries with the given <paramref name="sourceIndices"/> from this log file and copies
		///     them into the given <paramref name="destination"/>.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="destination"></param>
		/// <param name="queryOptions">Configures how the data is to be retrieved</param>
		public static void GetEntries(this ILogFile logFile, IReadOnlyList<LogLineIndex> sourceIndices, ILogEntries destination, LogFileQueryOptions queryOptions)
		{
			logFile.GetEntries(sourceIndices, destination, 0, queryOptions);
		}

		/// <summary>
		///     Retrieves all entries with the given <paramref name="sourceIndices"/> from this log file and copies
		///     them into the given <paramref name="destination"/>.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="destination"></param>
		/// <param name="destinationIndex"></param>
		public static void GetEntries(this ILogFile logFile, IReadOnlyList<LogLineIndex> sourceIndices, ILogEntries destination, int destinationIndex)
		{
			logFile.GetEntries(sourceIndices, destination, destinationIndex, LogFileQueryOptions.Default);
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
		public static IReadOnlyLogEntries GetEntries(this ILogFile logFile, IReadOnlyList<LogLineIndex> sourceIndices, IEnumerable<IColumnDescriptor> columns)
		{
			var buffer = new LogEntryArray(sourceIndices.Count, columns);
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
		public static IReadOnlyLogEntry GetEntry(this ILogFile logFile, LogLineIndex sourceIndex, IEnumerable<IColumnDescriptor> columns)
		{
			var buffer = new LogEntryArray(1, columns);
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
			var buffer = new LogEntryArray(1, logFile.Columns);
			logFile.GetEntries(new LogFileSection(sourceIndex, 1), buffer);
			return buffer[0];
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