using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using Tailviewer.Api;

namespace Tailviewer.Core.Sources
{
	/// <summary>
	///     Extension methods to the <see cref="ILogSource" /> interface.
	/// </summary>
	public static class LogSourceExtensions
	{
		/// <summary>
		///     Returns all properties from this log file.
		/// </summary>
		/// <returns></returns>
		public static IPropertiesBuffer GetAllProperties(this ILogSource that)
		{
			var destination = new PropertiesBufferList();
			that.GetAllProperties(destination);
			return destination;
		}

		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="logSource"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="column"></param>
		/// <param name="destination"></param>
		public static void GetColumn<T>(this ILogSource logSource,
										IReadOnlyList<LogLineIndex> sourceIndices,
										IColumnDescriptor<T> column,
										T[] destination)
		{
			logSource.GetColumn(sourceIndices, column, destination, 0, LogSourceQueryOptions.Default);
		}

		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="logSource"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="column"></param>
		/// <param name="destination"></param>
		/// <param name="queryOptions">Configures how the data is to be retrieved</param>
		public static void GetColumn<T>(this ILogSource logSource,
		                                IReadOnlyList<LogLineIndex> sourceIndices,
		                                IColumnDescriptor<T> column,
		                                T[] destination,
		                                LogSourceQueryOptions queryOptions)
		{
			logSource.GetColumn(sourceIndices, column, destination, 0, queryOptions);
		}

		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="logSource"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="column"></param>
		/// <param name="destination"></param>
		/// <param name="destinationIndex">Configures how the data is to be retrieved</param>
		public static void GetColumn<T>(this ILogSource logSource,
		                                IReadOnlyList<LogLineIndex> sourceIndices,
		                                IColumnDescriptor<T> column,
		                                T[] destination,
		                                int destinationIndex)
		{
			logSource.GetColumn(sourceIndices, column, destination, destinationIndex, LogSourceQueryOptions.Default);
		}

		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="logSource"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		[Pure]
		public static T[] GetColumn<T>(this ILogSource logSource, IReadOnlyList<LogLineIndex> sourceIndices, IColumnDescriptor<T> column)
		{
			return logSource.GetColumn<T>(sourceIndices, column, LogSourceQueryOptions.Default);
		}

		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="logSource"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="column"></param>
		/// <param name="queryOptions">Configures how the data is to be retrieved</param>
		/// <returns></returns>
		[Pure]
		public static T[] GetColumn<T>(this ILogSource logSource, IReadOnlyList<LogLineIndex> sourceIndices, IColumnDescriptor<T> column, LogSourceQueryOptions queryOptions)
		{
			if (sourceIndices == null)
				throw new ArgumentNullException(nameof(sourceIndices));

			var cells = new T[sourceIndices.Count];
			logSource.GetColumn(sourceIndices, column, cells, queryOptions);
			return cells;
		}

		/// <summary>
		///     Retrieves all entries for all columns of this log file.
		/// </summary>
		/// <remarks>
		///     This fetches the entire log file into memory.
		///     KNOW what you're doing.
		/// </remarks>
		/// <param name="logSource"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyLogBuffer GetEntries(this ILogSource logSource)
		{
			return logSource.GetEntries(logSource.Columns);
		}

		/// <summary>
		///     Retrieves all entries for all columns of this log file.
		/// </summary>
		/// <param name="logSource"></param>
		/// <param name="columns"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyLogBuffer GetEntries(this ILogSource logSource, IReadOnlyList<IColumnDescriptor> columns)
		{
			var count = logSource.GetProperty(GeneralProperties.LogEntryCount);
			var buffer = new LogBufferArray(count, columns);
			GetEntries(logSource, new LogSourceSection(0, count), buffer);
			return buffer;
		}

		/// <summary>
		///     Retrieves all entries for all columns of this log file of the given <paramref name="sourceIndices" />.
		/// </summary>
		/// <param name="logSource"></param>
		/// <param name="sourceIndices"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyLogBuffer GetEntries(this ILogSource logSource, IReadOnlyList<LogLineIndex> sourceIndices)
		{
			var buffer = new LogBufferArray(sourceIndices.Count, logSource.Columns);
			GetEntries(logSource, sourceIndices, buffer);
			return buffer;
		}

		/// <summary>
		///     Retrieves all entries with the given <paramref name="sourceIndices"/> from this log file and copies
		///     them into the given <paramref name="destination"/>.
		/// </summary>
		/// <param name="logSource"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="destination"></param>
		public static void GetEntries(this ILogSource logSource, IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination)
		{
			logSource.GetEntries(sourceIndices, destination, 0, LogSourceQueryOptions.Default);
		}

		/// <summary>
		///     Retrieves all entries with the given <paramref name="sourceIndices"/> from this log file and copies
		///     them into the given <paramref name="destination"/>.
		/// </summary>
		/// <param name="logSource"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="destination"></param>
		/// <param name="queryOptions">Configures how the data is to be retrieved</param>
		public static void GetEntries(this ILogSource logSource, IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, LogSourceQueryOptions queryOptions)
		{
			logSource.GetEntries(sourceIndices, destination, 0, queryOptions);
		}

		/// <summary>
		///     Retrieves all entries with the given <paramref name="sourceIndices"/> from this log file and copies
		///     them into the given <paramref name="destination"/>.
		/// </summary>
		/// <param name="logSource"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="destination"></param>
		/// <param name="destinationIndex"></param>
		public static void GetEntries(this ILogSource logSource, IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, int destinationIndex)
		{
			logSource.GetEntries(sourceIndices, destination, destinationIndex, LogSourceQueryOptions.Default);
		}

		/// <summary>
		///     Retrieves all entries with the given <paramref name="columns" /> of the given <paramref name="sourceIndices" />
		///     from this log file.
		/// </summary>
		/// <param name="logSource"></param>
		/// <param name="sourceIndices"></param>
		/// <param name="columns"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyLogBuffer GetEntries(this ILogSource logSource, IReadOnlyList<LogLineIndex> sourceIndices, IEnumerable<IColumnDescriptor> columns)
		{
			var buffer = new LogBufferArray(sourceIndices.Count, columns);
			logSource.GetEntries(sourceIndices, buffer);
			return buffer;
		}

		/// <summary>
		///     Retrieves the entry with the given <paramref name="columns" /> at the given <paramref name="sourceIndex" />
		///     from this log file.
		/// </summary>
		/// <remarks>
		///     Do NOT use this method if you want to retrieve multiple entries. Use any overload of ILogFile.GetEntries instead.
		/// </remarks>
		/// <param name="logSource"></param>
		/// <param name="sourceIndex"></param>
		/// <param name="columns"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyLogEntry GetEntry(this ILogSource logSource, LogLineIndex sourceIndex, IEnumerable<IColumnDescriptor> columns)
		{
			var buffer = new LogBufferArray(1, columns);
			logSource.GetEntries(new LogSourceSection(sourceIndex, 1), buffer);
			return buffer[0];
		}

		/// <summary>
		///     Retrieves the entry with all columns of the log file at the given <paramref name="sourceIndex" />.
		/// </summary>
		/// <remarks>
		///     Do NOT use this method if you want to retrieve multiple entries. Use any overload of ILogFile.GetEntries instead.
		/// </remarks>
		/// <param name="logSource"></param>
		/// <param name="sourceIndex"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyLogEntry GetEntry(this ILogSource logSource, LogLineIndex sourceIndex)
		{
			var buffer = new LogBufferArray(1, logSource.Columns);
			logSource.GetEntries(new LogSourceSection(sourceIndex, 1), buffer);
			return buffer[0];
		}

		/// <summary>
		///     Creates a filtered view onto this log file.
		/// </summary>
		/// <param name="logSource"></param>
		/// <param name="scheduler"></param>
		/// <param name="logLineFilter"></param>
		/// <param name="logEntryFilter"></param>
		/// <returns></returns>
		internal static FilteredLogSource AsFiltered(this ILogSource logSource,
		                                           ITaskScheduler scheduler,
		                                           ILogLineFilter logLineFilter,
		                                           ILogEntryFilter logEntryFilter)
		{
			return AsFiltered(logSource, scheduler, logLineFilter, logEntryFilter, TimeSpan.FromMilliseconds(value: 10));
		}

		/// <summary>
		///     Creates a filtered view onto this log file.
		/// </summary>
		/// <param name="logSource"></param>
		/// <param name="scheduler"></param>
		/// <param name="logLineFilter"></param>
		/// <param name="logEntryFilter"></param>
		/// <param name="maximumWaitTime"></param>
		/// <returns></returns>
		internal static FilteredLogSource AsFiltered(this ILogSource logSource,
		                                           ITaskScheduler scheduler,
		                                           ILogLineFilter logLineFilter,
		                                           ILogEntryFilter logEntryFilter,
		                                           TimeSpan maximumWaitTime)
		{
			var file = new FilteredLogSource(scheduler, maximumWaitTime, logSource, logLineFilter, logEntryFilter);
			return file;
		}
	}
}