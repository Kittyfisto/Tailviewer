using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Provides read-only access to a log entry of a <see cref="ILogFile" />.
	/// </summary>
	/// <remarks>
	///     Plugin users really shouldn't directly implement this interface and instead use one the implementations
	///     offered by the Tailviewer.Core library instead (namely ReadOnlyLogEntry, LogEntryArray and LogEntryList).
	///     If a custom implementation isn't avoidable, then the custom implementation should inherit from AbstractLogEntry instead.
	/// </remarks>
	/// <remarks>
	///     This interface is meant to replace <see cref="LogLine" />.
	///     With its introduction, <see cref="LogLineIndex" /> can be removed and be replaced
	///     with <see cref="LogEntryIndex" />.
	/// </remarks>
	public interface IReadOnlyLogEntry
	{
		/// <summary>
		///     The raw content of this log entry.
		/// </summary>
		/// <remarks>
		///     Might not be readable by a humand, depending on the data source.
		/// </remarks>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		string RawContent { get; }

		/// <summary>
		///     The index of this log entry, from 0 to the number of log entries.
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		LogLineIndex Index { get; }

		/// <summary>
		///     The index of the log entry this one was created from.
		///     Only differs from <see cref="Index" /> when this log entry has been created
		///     through operations such as filtering, merging, etc...
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		LogLineIndex OriginalIndex { get; }

		/// <summary>
		///     The index of this log entry, from 0 to the number of log entries.
		/// </summary>
		/// <remarks>
		///     TODO: Remove this property to <see cref="LogEntryIndex" /> once #140 is done.
		/// </remarks>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		[WillBeRemoved("This property will be be removed once multiline log entry handlign is rewritten", "https://github.com/Kittyfisto/Tailviewer/issues/140")]
		LogEntryIndex LogEntryIndex { get; }

		/// <summary>
		///     The (first) line number of the log entry, from 1 to the number of lines in the data source..
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		int LineNumber { get; }

		/// <summary>
		///     The (first) line number of the log entry, from 1 to the number of lines in the original data source..
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		int OriginalLineNumber { get; }

		/// <summary>
		///     The name of the log entry's original data source.
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		string OriginalDataSourceName { get; }

		/// <summary>
		///     The id of the source the log entry comes from.
		/// </summary>
		/// <remarks>
		///     This column is present when multiple sources are merged together in order to tell which file a particular entry belongs to.
		/// </remarks>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		LogLineSourceId SourceId { get; }


		/// <summary>
		/// The log level of this entry.
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		LevelFlags LogLevel { get; }

		/// <summary>
		///     The timestamp of this log entry, if available.
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		DateTime? Timestamp { get; }

		/// <summary>
		///     The amount of time elapsed between the first and this log entry.
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		TimeSpan? ElapsedTime { get; }

		/// <summary>
		///     The amount of time between this and the previous log entry.
		/// </summary>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		TimeSpan? DeltaTime { get; }

		/// <summary>
		///     Returns the value of this log entry for the given column.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="column"></param>
		/// <returns></returns>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		T GetValue<T>(IColumnDescriptor<T> column);

		/// <summary>
		///    Tries to lookup the value of this log entry for the given column.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryGetValue<T>(IColumnDescriptor<T> column, out T value);

		/// <summary>
		///     Returns the value of this log entry for the given column.
		/// </summary>
		/// <param name="column"></param>
		/// <returns></returns>
		/// <exception cref="NoSuchColumnException">When this column doesn't exist</exception>
		/// <exception cref="ColumnNotRetrievedException">When this column hasn't been retrieved</exception>
		object GetValue(IColumnDescriptor column);

		/// <summary>
		///    Tries to lookup the value of this log entry for the given column.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryGetValue(IColumnDescriptor column, out object value);

		/// <summary>
		///     The columns offered by this log entry.
		/// </summary>
		IReadOnlyList<IColumnDescriptor> Columns { get; }
	}
}