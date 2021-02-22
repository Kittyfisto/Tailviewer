using System.Collections.Generic;

namespace Tailviewer
{
	/// <summary>
	///     Provides read/write access to a list of log entries stored in memory.
	/// </summary>
	/// <remarks>
	///     Is mostly used to retrieve a portion of log entries from <see cref="ILogSource"/>s.
	/// </remarks>
	public interface ILogBuffer
		: IReadOnlyLogBuffer
		, IEnumerable<ILogEntry>
	{
		/// <summary>
		///     Copies data from the given array into this buffer.
		/// </summary>
		/// <remarks>
		///     This buffer must be large enough already to accomodate the data.
		/// </remarks>
		/// <typeparam name="T"></typeparam>
		/// <param name="column">The column to copy data to</param>
		/// <param name="destinationIndex">The first index in this buffer to which the given <paramref name="source" /> is copied</param>
		/// <param name="source">The source from which to copy data from</param>
		/// <param name="sourceIndex">The first index of <paramref name="source" /> from which to copy data from</param>
		/// <param name="length">The number of elements of <paramref name="source" /> to copy from</param>
		void CopyFrom<T>(IColumnDescriptor<T> column, int destinationIndex, IReadOnlyList<T> source, int sourceIndex, int length);

		/// <summary>
		///     Copies the given *non-contiguous* segment of data from the given log file into this buffer in a contiguous block.
		/// </summary>
		/// <remarks>
		///     This buffer must be large enough already to accomodate the data.
		/// </remarks>
		/// <param name="column">The column to copy the data from the log file to this buffer</param>
		/// <param name="destinationIndex">The first index in this buffer to which the data from the given <paramref name="source" /> is copied</param>
		/// <param name="source">The log file from which data should be copied from</param>
		/// <param name="sourceIndices">The non-contiguous section of the log file from which to copy from (e.g. from index 5, 10 entries)</param>
		/// <param name="queryOptions">Configures how the data is to be retrieved</param>
		[WillBeRemoved("TODO: Remove this abomination", "")]
		void CopyFrom(IColumnDescriptor column, int destinationIndex, ILogSource source, IReadOnlyList<LogLineIndex> sourceIndices, LogSourceQueryOptions queryOptions);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="column"></param>
		/// <param name="destinationIndex"></param>
		/// <param name="source"></param>
		/// <param name="sourceIndices"></param>
		void CopyFrom(IColumnDescriptor column, int destinationIndex, IReadOnlyLogBuffer source, IReadOnlyList<int> sourceIndices);

		/// <summary>
		///    Fills the given region of all columns with default values for every column's data type.
		/// </summary>
		/// <remarks>
		///     This buffer must be large enough already to accomodate the data.
		/// </remarks>
		/// <param name="offset">The offset into this buffer from which onward log entries should be filled with default values</param>
		/// <param name="length">The number of log entries in this buffer to fill with default values</param>
		void FillDefault(int offset, int length);

		/// <summary>
		///    Fills the given region of the given column with default values for that column's data type.
		/// </summary>
		/// <remarks>
		///     This buffer must be large enough already to accomodate the data.
		/// </remarks>
		/// <param name="column"></param>
		/// <param name="destinationIndex"></param>
		/// <param name="length"></param>
		void FillDefault(IColumnDescriptor column, int destinationIndex, int length);

		/// <summary>
		///    Fills the given region of the given column with default values for that column's data type.
		/// </summary>
		/// <remarks>
		///     This buffer must be large enough already to accomodate the data.
		/// </remarks>
		/// <param name="column"></param>
		/// <param name="value"></param>
		/// <param name="destinationIndex"></param>
		/// <param name="length"></param>
		void Fill<T>(IColumnDescriptor<T> column, T value, int destinationIndex, int length);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		new ILogEntry this[int index] { get; }
	}
}