using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Provides read/write access to a list of log entries.
	/// </summary>
	public interface ILogEntries
		: IReadOnlyLogEntries
	{
		/// <summary>
		///     Copies data from the given array into this buffer.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="column">The column to copy data to</param>
		/// <param name="destinationIndex">The first index in this buffer to which the given <paramref name="source" /> is copied</param>
		/// <param name="source">The source from which to copy data from</param>
		/// <param name="sourceIndex">The first index of <paramref name="source" /> from which to copy data from</param>
		/// <param name="length">The number of elements of <paramref name="source" /> to copy from</param>
		void CopyFrom<T>(ILogFileColumn<T> column, int destinationIndex, T[] source, int sourceIndex, int length);

		/// <summary>
		///     Copies data from the given column of the given log file into this buffer.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="destinationIndex"></param>
		/// <param name="source"></param>
		/// <param name="section"></param>
		void CopyFrom(ILogFileColumn column, int destinationIndex, ILogFile source, LogFileSection section);

		/// <summary>
		///     Copies data from the given column of the given log file into this buffer.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="destinationIndex"></param>
		/// <param name="source"></param>
		/// <param name="indices"></param>
		void CopyFrom(ILogFileColumn column, int destinationIndex, ILogFile source, IReadOnlyList<LogLineIndex> indices);

		/// <summary>
		///    Fills the given region of all columns with default values for every column's data type.
		/// </summary>
		/// <param name="destinationIndex"></param>
		/// <param name="length"></param>
		void FillDefault(int destinationIndex, int length);

		/// <summary>
		///    Fills the given region of the given column with default values for that column's data type.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="destinationIndex"></param>
		/// <param name="length"></param>
		void FillDefault(ILogFileColumn column, int destinationIndex, int length);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		new ILogEntry this[int index] { get; }
	}
}