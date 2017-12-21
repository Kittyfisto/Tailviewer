using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Metrolib;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     The interface to represent a continuous stream of <see cref="LogLine" />s from
	///     some data source. Such a data source may be a file on disk, a SQL database or whatever.
	/// </summary>
	/// <remarks>
	///     This interface is meant to provide access to the wrapped data source in a coherent way and to notify
	///     the application of changes to the data source, if necessary.
	/// </remarks>
	/// <remarks>
	///     TODO: Create separate (simplier) interface for log file sources (to be used by plugins) so they don't have to implement that many methods...
	/// </remarks>
	public interface ILogFile
		: IDisposable
	{
		/// <summary>
		///     The first identified timestamp of the data source, if any, null otherwise.
		/// </summary>
		DateTime? StartTimestamp { get; }

		/// <summary>
		///     The timestamp (in local time) the data source has last been modified.
		///     A modification is meant to be the addition and/or removal of at least one log line.
		/// </summary>
		DateTime LastModified { get; }

		/// <summary>
		///     The timestamp (in local time) the data source has been created.
		/// </summary>
		/// <remarks>
		///     Is set to <see cref="DateTime.MinValue" /> when the data source doesn't exist.
		/// </remarks>
		DateTime Created { get; }

		/// <summary>
		///     The approximate size of the data source.
		///     Is only needed to be displayed to the user.
		/// </summary>
		Size Size { get; }

		/// <summary>
		///     The error, if any, which describes why this log file is empty.
		/// </summary>
		ErrorFlags Error { get; }

		/// <summary>
		///     Whether or not this log file has reached the end of its data source.
		/// </summary>
		bool EndOfSourceReached { get; }

		/// <summary>
		///     The total number of <see cref="LogLine" />s that are offered by this log file at this moment.
		///     If the log file is not modified, then it is expected that <see cref="GetSection" /> may be called
		///     with as many lines as returned by this property.
		/// </summary>
		int Count { get; }

		/// <summary>
		///     The total number of <see cref="LogLine" />s of the underlying data source, which
		///     is expected to be <see cref="Count" /> most of the time, unless the implementation
		///     filters an underlying data source.
		/// </summary>
		int OriginalCount { get; }

		/// <summary>
		///     The maximum amount of characters of a single <see cref="LogLine" />.
		/// </summary>
		int MaxCharactersPerLine { get; }

		/// <summary>
		///     The columns offered by this log file.
		/// </summary>
		IReadOnlyList<ILogFileColumn> Columns { get; }

		/// <summary>
		///     Adds a new listener to this log file.
		///     The listener will be synchronized to the current state of this log file and then be notified
		///     of any further changes.
		/// </summary>
		/// <param name="listener"></param>
		/// <param name="maximumWaitTime"></param>
		/// <param name="maximumLineCount"></param>
		void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount);

		/// <summary>
		///     Removes the given listener from this log file.
		///     The listener will no longer be notified of changes to this log file.
		/// </summary>
		/// <param name="listener"></param>
		void RemoveListener(ILogFileListener listener);

		#region Data Retrieval

		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="section"></param>
		/// <param name="column"></param>
		/// <param name="buffer"></param>
		/// <param name="destinationIndex">The first index into <paramref name="buffer"/> where the first item of the retrieved section is copied to</param>
		void GetColumn<T>(LogFileSection section, ILogFileColumn<T> column, T[] buffer, int destinationIndex);

		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="indices"></param>
		/// <param name="column"></param>
		/// <param name="buffer"></param>
		/// <param name="destinationIndex">The first index into <paramref name="buffer"/> where the first item of the retrieved section is copied to</param>
		void GetColumn<T>(IReadOnlyList<LogLineIndex> indices, ILogFileColumn<T> column, T[] buffer, int destinationIndex);

		/// <summary>
		///     Retrieves all entries from the given <paramref name="section" /> from this log file and copies
		///     them into the given <paramref name="buffer" /> starting at the given <paramref name="destinationIndex"/>.
		/// </summary>
		/// <param name="section"></param>
		/// <param name="buffer"></param>
		/// <param name="destinationIndex"></param>
		void GetEntries(LogFileSection section, ILogEntries buffer, int destinationIndex);

		/// <summary>
		///     Retrieves all entries from the given <paramref name="indices" /> from this log file and copies
		///     them into the given <paramref name="buffer" /> starting at the given <paramref name="destinationIndex"/>.
		/// </summary>
		/// <param name="indices"></param>
		/// <param name="buffer"></param>
		/// <param name="destinationIndex"></param>
		void GetEntries(IReadOnlyList<LogLineIndex> indices, ILogEntries buffer, int destinationIndex);

		/// <summary>
		///     Retrieves a list of log lines from this log file.
		/// </summary>
		/// <remarks>
		///     This method is currently expected to block until all lines have been retrieved.
		/// </remarks>
		/// <param name="section"></param>
		/// <param name="dest"></param>
		[WillBeRemoved("LogLine will be removed and so will this method sometime in 2018", "https://github.com/Kittyfisto/Tailviewer/issues/143")]
		void GetSection(LogFileSection section, LogLine[] dest);

		/// <summary>
		///     Retrieves the given log line.
		/// </summary>
		/// <remarks>
		///     This method is currently expected to block until the given line has been retrieved.
		/// </remarks>
		/// <param name="index"></param>
		/// <returns></returns>
		[Pure]
		[WillBeRemoved("LogLine will be removed and so will this method sometime in 2018", "https://github.com/Kittyfisto/Tailviewer/issues/143")]
		LogLine GetLine(int index);

		/// <summary>
		///     The relative progress (in between 0 and 1) between the number of lines currently exposed by this log file versus
		///     the number
		///     of lines in the underlying data source.
		/// </summary>
		/// <remarks>
		///     In case it is unfeasable to determine the number of lines before scanning through the entire source, 1 should be returned.
		/// </remarks>
		double Progress { get; }

		#endregion

		#region Indices

		/// <summary>
		/// </summary>
		/// <param name="originalLineIndex"></param>
		/// <returns></returns>
		[Pure]
		LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex);

		/// <summary>
		///     Returns the original log line index for the given index.
		/// </summary>
		/// <remarks>
		///     Returns the same index as given unless this log file filters or reorders
		///     log lines.
		/// </remarks>
		/// <param name="index"></param>
		/// <returns></returns>
		[Pure]
		LogLineIndex GetOriginalIndexFrom(LogLineIndex index);

		#endregion
	}
}