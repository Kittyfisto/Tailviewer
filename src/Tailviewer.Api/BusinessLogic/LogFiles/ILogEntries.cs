using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Provides read/write access to a list of log entries.
	/// </summary>
	public interface ILogEntries
		: IReadOnlyLogEntries
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
		void CopyFrom<T>(IColumnDescriptor<T> column, int destinationIndex, T[] source, int sourceIndex, int length);

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
		void CopyFrom(IColumnDescriptor column, int destinationIndex, ILogFile source, IReadOnlyList<LogLineIndex> sourceIndices, LogFileQueryOptions queryOptions);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="column"></param>
		/// <param name="destinationIndex"></param>
		/// <param name="source"></param>
		/// <param name="sourceIndices"></param>
		void CopyFrom(IColumnDescriptor column, int destinationIndex, IReadOnlyLogEntries source, IReadOnlyList<int> sourceIndices);

		/// <summary>
		///    Fills the given region of all columns with default values for every column's data type.
		/// </summary>
		/// <remarks>
		///     This buffer must be large enough already to accomodate the data.
		/// </remarks>
		/// <param name="destinationIndex"></param>
		/// <param name="length"></param>
		void FillDefault(int destinationIndex, int length);

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
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		new ILogEntry this[int index] { get; }
	}
}