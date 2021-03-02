using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     Provides access to well-known columns which are provided by all log files:
	///     Every column has a well-defined meaning which will never change.
	/// </summary>
	public static class GeneralColumns
	{
		/// <summary>
		///     The minimum list of columns which every log file must provide.
		///     If a log file cannot provide contents for a certain column, then it should be filled
		///     with default values for that columns <see cref="IColumnDescriptor.DataType" />.
		/// </summary>
		/// <remarks>
		///     A log file can supply any number of additional <see cref="CustomColumnDescriptor{T}" />s.
		///     This list simply marks those columns which MUST be there.
		/// </remarks>
		/// <remarks>
		///     This list is subject to change over the years...
		/// </remarks>
		public static readonly IReadOnlyList<IColumnDescriptor> Minimum;

		/// <summary>
		///     The raw content of the entry as it was extracted from the data source.
		/// </summary>
		/// <remarks>
		///     Might not be readable by a human, depending on the data source.
		/// </remarks>
		public static readonly IColumnDescriptor<string> RawContent;

		/// <summary>
		///     The index of the log entry, from 0 to the number of log entries.
		/// </summary>
		/// <remarks>
		///     TODO: Change this property to <see cref="LogEntryIndex" /> once #140 is done.
		/// </remarks>
		public static readonly IColumnDescriptor<LogLineIndex> Index;

		/// <summary>
		///     The index of the log entry another one was created from.
		///     Only differs from <see cref="Index" /> when the log entry has been created
		///     through operations such as filtering, merging, etc...
		/// </summary>
		/// <remarks>
		///     TODO: Change this property to <see cref="LogEntryIndex" /> once #140 is done.
		/// </remarks>
		public static readonly IColumnDescriptor<LogLineIndex> OriginalIndex;

		/// <summary>
		///     The index of this log entry, from 0 to the number of log entries.
		/// </summary>
		/// <remarks>
		///     TODO: Remove this property to <see cref="LogEntryIndex" /> once #140 is done.
		/// </remarks>
		[WillBeRemoved("This property will be be removed once multiline log entry handling is rewritten", "https://github.com/Kittyfisto/Tailviewer/issues/140")]
		public static readonly IColumnDescriptor<LogEntryIndex> LogEntryIndex;

		/// <summary>
		///     The (first) line number of the log entry, from 1 to the number of lines in the data source..
		/// </summary>
		/// <remarks>
		///     TODO: Remove this property to <see cref="LogEntryIndex" /> once #140 is done.
		/// </remarks>
		[WillBeRemoved("This property will be removed once multiline log entry handling is rewritten", "https://github.com/Kittyfisto/Tailviewer/issues/140")]
		public static readonly IColumnDescriptor<int> LineNumber;

		/// <summary>
		///     The (first) line number of the log entry, from 1 to the number of lines in the original data source..
		/// </summary>
		public static readonly IColumnDescriptor<int> OriginalLineNumber;

		/// <summary>
		///     The name of the log entry's original data source.
		/// </summary>
		public static readonly IColumnDescriptor<string> OriginalDataSourceName;

		/// <summary>
		///     The id of the source the log entry comes from.
		/// </summary>
		/// <remarks>
		///     This column is present when multiple sources are merged together in order to tell which file a particular entry belongs to.
		/// </remarks>
		public static readonly IColumnDescriptor<LogEntrySourceId> SourceId;

		/// <summary>
		///     The log level of the entry (debug, info, warning, etc...)
		/// </summary>
		public static readonly IColumnDescriptor<LevelFlags> LogLevel;

		/// <summary>
		///     The absolute timestamp of when the log entry was produced.
		/// </summary>
		public static readonly IColumnDescriptor<DateTime?> Timestamp;

		/// <summary>
		///     The amount of time elapsed between the first and this log entry.
		/// </summary>
		public static readonly IColumnDescriptor<TimeSpan?> ElapsedTime;

		/// <summary>
		///     The amount of time between this and the previous log entry.
		/// </summary>
		public static readonly IColumnDescriptor<TimeSpan?> DeltaTime;

		/// <summary>
		///     The (human readable) message of the log entry (i.e. the actual textual information that does not info into
		///     any other column).
		/// </summary>
		public static readonly IColumnDescriptor<string> Message;

		static GeneralColumns()
		{
			RawContent = new WellKnownColumnDescriptor<string>("raw_content");
			Index = new WellKnownColumnDescriptor<LogLineIndex>("index", LogLineIndex.Invalid);
			OriginalIndex = new WellKnownColumnDescriptor<LogLineIndex>("original_index", LogLineIndex.Invalid);
			LogEntryIndex = new WellKnownColumnDescriptor<LogEntryIndex>("log_entry_index", Api.LogEntryIndex.Invalid);
			LineNumber = new WellKnownColumnDescriptor<int>("line_number");
			OriginalLineNumber = new WellKnownColumnDescriptor<int>("original_line_number");
			OriginalDataSourceName = new WellKnownColumnDescriptor<string>("original_data_source_name");
			SourceId = new WellKnownColumnDescriptor<LogEntrySourceId>("source_id");
			LogLevel = new WellKnownColumnDescriptor<LevelFlags>("log_level");
			Timestamp = new WellKnownColumnDescriptor<DateTime?>("timestamp");
			ElapsedTime = new WellKnownColumnDescriptor<TimeSpan?>("elapsed_time");
			DeltaTime = new WellKnownColumnDescriptor<TimeSpan?>("delta_time");
			Message = new WellKnownColumnDescriptor<string>("message", "Message");

			Minimum = new IColumnDescriptor[]
			{
				RawContent,
				Index,
				OriginalIndex,
				LogEntryIndex,
				LineNumber,
				OriginalLineNumber,
				OriginalDataSourceName,
				LogLevel,
				Timestamp,
				ElapsedTime,
				DeltaTime
			};
		}

		/// <summary>
		///     Creates a list of columns which consists of the <see cref="Minimum" /> set of columns
		///     as well as the given ones. Duplicate columns will only appear once.
		/// </summary>
		/// <param name="columns"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyList<IColumnDescriptor> CombineWithMinimum(IEnumerable<IColumnDescriptor> columns)
		{
			var actualColumns = new List<IColumnDescriptor>(Minimum);
			foreach (var column in columns)
				if (!actualColumns.Contains(column))
					actualColumns.Add(column);
			return actualColumns;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="columns"></param>
		/// <param name="additionalColumns"></param>
		/// <returns></returns>
		public static IReadOnlyList<IColumnDescriptor> Combine(IEnumerable<IColumnDescriptor> columns,
		                                                    IEnumerable<IColumnDescriptor> additionalColumns)
		{
			var allColumns = new List<IColumnDescriptor>(columns);
			foreach (var column in additionalColumns)
			{
				if (!allColumns.Contains(column))
				{
					allColumns.Add(column);
				}
			}
			return allColumns;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="columns"></param>
		/// <param name="additionalColumns"></param>
		/// <returns></returns>
		public static IReadOnlyList<IColumnDescriptor> Combine(IEnumerable<IColumnDescriptor> columns,
		                                                    params IColumnDescriptor[] additionalColumns)
		{
			return Combine(columns, (IEnumerable<IColumnDescriptor>) additionalColumns);
		}
	}
}