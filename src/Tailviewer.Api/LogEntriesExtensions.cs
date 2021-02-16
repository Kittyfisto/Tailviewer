using System.Collections.Generic;

namespace Tailviewer
{
	/// <summary>
	///    Provides overloaded methods for <see cref="ILogBuffer"/> for convenience.
	/// </summary>
	public static class LogEntriesExtensions
	{
		/// <summary>
		///     Copies the given *non-contiguous* segment of data from the given log file into this buffer in a contiguous block.
		/// </summary>
		/// <remarks>
		///     This buffer must be large enough already to accomodate the data.
		/// </remarks>
		/// <param name="that"></param>
		/// <param name="column">The column to copy the data from the log file to this buffer</param>
		/// <param name="queryOptions">Configures how the data is to be retrieved</param>
		/// <param name="source">The log file from which data should be copied from</param>
		/// <param name="sourceIndices">The non-contiguous section of the log file from which to copy from (e.g. from index 5, 10 entries)</param>
		public static void CopyFrom(this ILogBuffer that,
		                            IColumnDescriptor column,
		                            ILogSource source,
		                            IReadOnlyList<LogLineIndex> sourceIndices,
		                            LogFileQueryOptions queryOptions)
		{
			that.CopyFrom(column, 0, source, sourceIndices, LogFileQueryOptions.Default);
		}

		/// <summary>
		///     Copies the given *non-contiguous* segment of data from the given log file into this buffer in a contiguous block.
		/// </summary>
		/// <remarks>
		///     This buffer must be large enough already to accomodate the data.
		/// </remarks>
		/// <param name="that"></param>
		/// <param name="column">The column to copy the data from the log file to this buffer</param>
		/// <param name="destinationIndex">The first index in this buffer to which the data from the given <paramref name="source" /> is copied</param>
		/// <param name="source">The log file from which data should be copied from</param>
		/// <param name="sourceIndices">The non-contiguous section of the log file from which to copy from (e.g. from index 5, 10 entries)</param>
		public static void CopyFrom(this ILogBuffer that,
		                            IColumnDescriptor column,
		                            int destinationIndex,
		                            ILogSource source,
		                            IReadOnlyList<LogLineIndex> sourceIndices)
		{
			that.CopyFrom(column, destinationIndex, source, sourceIndices, LogFileQueryOptions.Default);
		}
	}
}