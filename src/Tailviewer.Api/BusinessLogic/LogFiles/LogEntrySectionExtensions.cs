namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	/// 
	/// </summary>
	public static class LogEntrySectionExtensions
	{
		/// <summary>
		///     Copies data from the given array into this buffer.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="section"></param>
		/// <param name="column">The column to copy data to</param>
		/// <param name="source">The source from which to copy data from</param>
		public static void CopyFrom<T>(this ILogEntries section, ILogFileColumn<T> column, T[] source)
		{
			section.CopyFrom(column, 0, source, 0, source.Length);
		}
	}
}