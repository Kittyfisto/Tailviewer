namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Provides read/write access to a consecutive section of log entries.
	/// </summary>
	public interface ILogEntrySection
		: IReadOnlyLogEntries
	{
		/// <summary>
		///     The section of the log file covered by this buffer.
		/// </summary>
		LogFileSection Section { get; }

		/// <summary>
		///     Copies data from the given array into this buffer.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="column">The column to copy data to</param>
		/// <param name="destinationIndex">The first index in this buffer to which the given <paramref name="source" /> is copied</param>
		/// <param name="source">The source from which to copy data from</param>
		/// <param name="sourceIndex">The first index of <paramref name="source" /> from which to copy data from</param>
		/// <param name="length">The number of elements of <paramref name="source" /> to copy from</param>
		void CopyFrom<T>(ILogFileColumn column, int destinationIndex, T[] source, int sourceIndex, int length);
	}
}