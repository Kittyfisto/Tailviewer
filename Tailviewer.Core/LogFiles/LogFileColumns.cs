using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Provides access to well-known columns which are provided by all log files:
	///     Every column has a well-defined meaning which will never change.
	/// </summary>
	public static class LogFileColumns
	{
		/// <summary>
		///     The minimum list of columns which every log file must provide.
		///     If a log file cannot provide contents for a certain column, then it should be filled
		///     with default values for that columns <see cref="ILogFileColumn.DataType" />.
		/// </summary>
		/// <remarks>
		///     A log file can supply any number of additional <see cref="CustomLogFileColumn{T}" />s.
		///     This list simply marks those columns which MUST be there.
		/// </remarks>
		/// <remarks>
		///     This list is subject to change over the years...
		/// </remarks>
		public static readonly IReadOnlyList<ILogFileColumn> Minimum;

		/// <summary>
		///     The raw content of the entry as it was extracted from the data source.
		/// </summary>
		/// <remarks>
		///     Might not be readable by a humand, depending on the data source.
		/// </remarks>
		public static readonly ILogFileColumn<string> RawContent;

		/// <summary>
		///     The index of the log entry, from 0 to the number of log entries.
		/// </summary>
		/// <remarks>
		///     TODO: Change this property to <see cref="LogEntryIndex" /> once #140 is done.
		/// </remarks>
		public static readonly ILogFileColumn<LogLineIndex> Index;

		/// <summary>
		///     The index of the log entry another one was created from.
		///     Only differs from <see cref="Index" /> when the log entry has been created
		///     through operations such as filtering, merging, etc...
		/// </summary>
		/// <remarks>
		///     TODO: Change this property to <see cref="LogEntryIndex" /> once #140 is done.
		/// </remarks>
		public static readonly ILogFileColumn<LogLineIndex> OriginalIndex;

		/// <summary>
		///     The index of this log entry, from 0 to the number of log entries.
		/// </summary>
		/// <remarks>
		///     TODO: Remove this property to <see cref="LogEntryIndex" /> once #140 is done.
		/// </remarks>
		[WillBeRemoved("This property will be be removed once multiline log entry handlign is rewritten", "https://github.com/Kittyfisto/Tailviewer/issues/140")]
		public static readonly ILogFileColumn<LogEntryIndex> LogEntryIndex;

		/// <summary>
		///     The (first) line number of the log entry, from 1 to the number of lines in the data source..
		/// </summary>
		public static readonly ILogFileColumn<int> LineNumber;

		/// <summary>
		///     The (first) line number of the log entry, from 1 to the number of lines in the original data source..
		/// </summary>
		public static readonly ILogFileColumn<int> OriginalLineNumber;

		/// <summary>
		///     The log level of the entry (debug, info, warning, etc...)
		/// </summary>
		public static readonly ILogFileColumn<LevelFlags> LogLevel;

		/// <summary>
		///     The absolute timestamp of when the log entry was produced.
		/// </summary>
		public static readonly ILogFileColumn<DateTime?> Timestamp;

		/// <summary>
		///     The amount of time elapsed between the first and this log entry.
		/// </summary>
		public static readonly ILogFileColumn<TimeSpan?> ElapsedTime;

		/// <summary>
		///     The amount of time between this and the previous log entry.
		/// </summary>
		public static readonly ILogFileColumn<TimeSpan?> DeltaTime;

		static LogFileColumns()
		{
			RawContent = new WellKnownLogFileColumn<string>("raw_content");
			Index = new WellKnownLogFileColumn<LogLineIndex>("index");
			OriginalIndex = new WellKnownLogFileColumn<LogLineIndex>("original_index");
			LogEntryIndex = new WellKnownLogFileColumn<LogEntryIndex>("log_entry_index");
			LineNumber = new WellKnownLogFileColumn<int>("line_number");
			OriginalLineNumber = new WellKnownLogFileColumn<int>("original_line_number");
			LogLevel = new WellKnownLogFileColumn<LevelFlags>("log_level");
			Timestamp = new WellKnownLogFileColumn<DateTime?>("timestamp");
			ElapsedTime = new WellKnownLogFileColumn<TimeSpan?>("elapsed_time");
			DeltaTime = new WellKnownLogFileColumn<TimeSpan?>("delta_time");

			Minimum = new ILogFileColumn[]
			{
				RawContent,
				Index,
				OriginalIndex,
				LogEntryIndex,
				LineNumber,
				OriginalLineNumber,
				LogLevel,
				Timestamp,
				ElapsedTime,
				DeltaTime
			};
		}
	}
}