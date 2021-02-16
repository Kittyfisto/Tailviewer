using System;

namespace Tailviewer
{
	/// <summary>
	///     Represents a log entry which can be modified.
	/// </summary>
	/// <remarks>
	///     The intention of this interface is to provide read-write access to a log entry backed by memory.
	///     Changes to certain columns must not flow back to the log source.
	/// </remarks>
	/// <remarks>
	///     Plugin users really shouldn't directly implement this interface and instead use one the implementations
	///     offered by the Tailviewer.Core library instead (namely LogEntry, LogEntryArray and LogEntryList).
	///     If a custom implementation isn't avoidable, then the custom implementation should inherit from AbstractLogEntry instead.
	/// </remarks>
	public interface ILogEntry
		: IReadOnlyLogEntry
	{
		/// <summary>
		///     The raw content of this log entry.
		/// </summary>
		/// <remarks>
		///     Might not be readable by a human, depending on the data source.
		/// </remarks>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		new string RawContent { get; set; }

		/// <summary>
		///     The index of this log entry, from 0 to the number of log entries.
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		new LogLineIndex Index { get; set; }

		/// <summary>
		///     The index of the log entry this one was created from.
		///     Only differs from <see cref="Index" /> when this log entry has been created
		///     through operations such as filtering, merging, etc...
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		new LogLineIndex OriginalIndex { get; set; }

		/// <summary>
		///     The index of this log entry, from 0 to the number of log entries.
		/// </summary>
		/// <remarks>
		///     TODO: Remove this property to <see cref="LogEntryIndex" /> once #140 is done.
		/// </remarks>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		[WillBeRemoved("This property will be be removed once multiline log entry handling is rewritten", "https://github.com/Kittyfisto/Tailviewer/issues/140")]
		new LogEntryIndex LogEntryIndex { get; set; }

		/// <summary>
		///     The (first) line number of the log entry, from 1 to the number of lines in the data source..
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		new int LineNumber { get; set; }

		/// <summary>
		///     The (first) line number of the log entry, from 1 to the number of lines in the original data source..
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		new int OriginalLineNumber { get; set; }

		/// <summary>
		///     The name of the log entry's original data source.
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		new string OriginalDataSourceName { get; set; }

		/// <summary>
		///     The id of the source the log entry comes from.
		/// </summary>
		/// <remarks>
		///     This column is present when multiple sources are merged together in order to tell which file a particular entry belongs to.
		/// </remarks>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		new LogLineSourceId SourceId { get; set; }

		/// <summary>
		/// The log level of this entry.
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		new LevelFlags LogLevel { get; set; }

		/// <summary>
		///     The timestamp of this log entry, if available.
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		new DateTime? Timestamp { get; set; }

		/// <summary>
		///     The amount of time elapsed between the first and this log entry.
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		new TimeSpan? ElapsedTime { get; set; }

		/// <summary>
		///     The amount of time between this and the previous log entry.
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		new TimeSpan? DeltaTime { get; set; }

		/// <summary>
		///     Sets the value of this log entry for the given column.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="value"></param>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		void SetValue(IColumnDescriptor column, object value);

		/// <summary>
		///     Sets the value of this log entry for the given column.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="column"></param>
		/// <param name="value"></param>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		void SetValue<T>(IColumnDescriptor<T> column, T value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logEntry"></param>
		void CopyFrom(IReadOnlyLogEntry logEntry);
	}
}
