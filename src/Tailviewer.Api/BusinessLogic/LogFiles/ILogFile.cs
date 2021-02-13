using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     This interface represents a log file (which doesn't have to be a file on disk) and allows the rest of Tailviewer to access the contents thereof.
	///     An ILogFile consists of columns, log entries and properties.
	/// </summary>
	/// <remarks>
	///     An ILogFile should offer (if possible) a minimum set of columns, so Tailviewer may interact with the contents of the log. A log file may offer as many columns
	///     as it wants and the columns do not have to be known to Tailviewer either: A log file may introduce its own column and simply return it via <see cref="Columns"/>.
	/// </remarks>
	/// <remarks>
	///     An ILogFile may offer a set of properties which are each described via an <see cref="ILogFilePropertyDescriptor"/> object and which each may hold a single object.
	///     There exists a set of properties which Tailviewer knows (such as File Size, First Timestamp, Last Modification Time, etc...) but a log file may introduce more properties
	///     which Tailviewer doesn't know. In the end, these properties will be shown to the user.
	/// </remarks>
	/// <remarks>
	///     Last but not least, an ILogFile holds log entries and allows Tailviewer to access them via one of these following methods (or their overloaded versions):
	///     - <see cref="GetEntries(IReadOnlyList{LogLineIndex},Tailviewer.BusinessLogic.LogFiles.ILogEntries,int,LogFileQueryOptions)"/>
	///     - <see cref="GetColumn{T}(IReadOnlyList{LogLineIndex},ILogFileColumnDescriptor{T},T[],int,LogFileQueryOptions)"/>
	///     Tailviewer will call these methods to access portions of the log file. Depending on the size of the log file, Tailviewer might access only a portion of the log file,
	///     for example 1000 log entries beginning with the 5000th one. And even then, it may only be interested in the Index and RawContent column (ignoring any others).
	///     Any implementation of this log file must make sure to properly implement this interface if there shall be any success in getting tailviewer to understand a source.
	///     While tailviewer may hold small portions of the log file in its internal buffers, an <see cref="ILogFile"/> implementation must make sure to guarantee low latencies
	///     when executing any of its methods or performance will suffer.
	/// </remarks>
	/// <remarks>
	///     Out of boundary access:  
	///     As agreed upon within this library, accessing rows outside of the boundaries of a log file is allowed and must not throw.
	///     Instead all invalid cells which are accessed must return their <see cref="ILogFileColumnDescriptor.DefaultValue"/>.
	///     Callers which are interested in finding out if they have accessed invalid portions of a log file are encouraged to include the Index column in their query
	///     and to then check if a particular row's Index is set to <see cref="LogLineIndex.Invalid"/>. If it is, then the entire row wasn't part of the data source.
	///
	///     The reason for this is two-fold:
	///     - Log files may shrink in size and due to the asynchronous nature of accessing log files, out of boundary access is quite possible (and sometimes even likely)
	///     - In the near future, Tailviewer will be ready to continuously stream data into memory when needed (instead of currently where everything is stored in memory)
	///       and when that point in time comes, it's quite likely that one tries to access a portion of a log file which just isn't streamed into memory yet.
	///       Depending on the use case, it is quite acceptable to retrieve the portions of the log file which ARE in memory now and to retrieve the rest at a later point
	///       in time.
	/// </remarks>
	public interface ILogFile
		: IDisposable
	{
		/// <summary>
		///     Whether or not this log file has reached the end of its data source.
		/// </summary>
		bool EndOfSourceReached { get; }

		/// <summary>
		///     The total number of <see cref="LogLine" />s that are offered by this log file at this moment.
		///     If the log file is not modified, then it is expected that <see cref="GetEntries(IReadOnlyList{LogLineIndex},Tailviewer.BusinessLogic.LogFiles.ILogEntries,int,LogFileQueryOptions)" /> may be called
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
		[WillBeRemoved("LogLine will be removed and so will this method sometime in 2021", "https://github.com/Kittyfisto/Tailviewer/issues/143")]
		int MaxCharactersPerLine { get; }

		/// <summary>
		///     The columns offered by this log file.
		/// </summary>
		IReadOnlyList<ILogFileColumnDescriptor> Columns { get; }

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

		#region Properties

		/// <summary>
		///     The properties offered by this log file.
		/// </summary>
		IReadOnlyList<ILogFilePropertyDescriptor> Properties { get; }

		/// <summary>
		///     Retrieves the value for the given property.
		/// </summary>
		/// <remarks>
		///     When the property doesn't exist or when the property isn't available, then
		///     <see cref="ILogFilePropertyDescriptor.DefaultValue" /> is returned instead.
		/// </remarks>
		/// <param name="propertyDescriptor"></param>
		/// <returns></returns>
		object GetValue(ILogFilePropertyDescriptor propertyDescriptor);

		/// <summary>
		///     Retrieves the value for the given property.
		/// </summary>
		/// <remarks>
		///     When the property doesn't exist or when the property isn't available, then
		///     <see cref="ILogFilePropertyDescriptor{T}.DefaultValue" /> is returned instead.
		/// </remarks>
		/// <typeparam name="T"></typeparam>
		/// <param name="propertyDescriptor"></param>
		/// <returns></returns>
		T GetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor);

		/// <summary>
		///     Retrieves the values for the given properties and stores them in the given buffer.
		/// </summary>
		/// <param name="properties"></param>
		/// <exception cref="NoSuchPropertyException">When this log file doesn't offer the given property</exception>
		void GetValues(ILogFileProperties properties);

		#endregion

		#region Data Retrieval
		
		/// <summary>
		///     Retrieves a list of cells for a given column from this log file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sourceIndices"></param>
		/// <param name="column"></param>
		/// <param name="destination"></param>
		/// <param name="destinationIndex">The first index into <paramref name="destination"/> where the first item of the retrieved section is copied to</param>
		/// <param name="queryOptions">Configures how the data is to be retrieved</param>
		void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, ILogFileColumnDescriptor<T> column, T[] destination, int destinationIndex, LogFileQueryOptions queryOptions);
		
		/// <summary>
		///     Retrieves all entries from the given <paramref name="sourceIndices" /> from this log file and copies
		///     them into the given <paramref name="destination" /> starting at the given <paramref name="destinationIndex"/>.
		/// </summary>
		/// <param name="sourceIndices"></param>
		/// <param name="destination"></param>
		/// <param name="destinationIndex">The first index into <paramref name="destination"/> where the first item of the retrieved section is copied to</param>
		/// <param name="queryOptions">Configures how the data is to be retrieved</param>
		void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogEntries destination, int destinationIndex, LogFileQueryOptions queryOptions);

		/// <summary>
		///     The relative progress (in between 0 and 1) between the number of lines currently exposed by this log file versus
		///     the number
		///     of lines in the underlying data source.
		/// </summary>
		/// <remarks>
		///     In case it is unfeasible to determine the number of lines before scanning through the entire source, 1 should be returned.
		/// </remarks>
		double Progress { get; }

		#endregion

		#region Indices

		/// <summary>
		///     Performs a reverse lookup and returns the index of the log entry
		///     which has the given original index.
		/// </summary>
		/// <param name="originalLineIndex"></param>
		/// <returns></returns>
		[Pure]
		LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex);

		#endregion
	}
}