using System.Collections.Generic;
using System.Linq;
using Tailviewer.Api;

namespace Tailviewer.Core.Buffers
{
	/// <summary>
	/// 
	/// </summary>
	public static class LogBufferExtensions
	{
		/// <summary>
		///    Creates a view onto this buffer which hides the specified columns.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="columns"></param>
		/// <returns></returns>
		public static ILogBuffer Except(this ILogBuffer that, IReadOnlyList<IColumnDescriptor> columns)
		{
			return new LogBufferView(that, that.Columns.Except(columns).ToList());
		}

		/// <summary>
		///    Returns a new log buffer which acts as a view onto the original buffer.
		///    If the original buffer already contains the desired column, then the original buffer is returned.
		///    If it does not, then an additional temporary buffer of equal length is created which
		///    holds data for the given <paramref name="columns"/> and the returned buffer combines both the
		///    <paramref name="that" /> as well as the temporary buffer into one view.
		/// </summary>
		/// <remarks>
		///    This method exists for those cases, where, in order to fulfill a certain <see cref="ILogSource.GetEntries(System.Collections.Generic.IReadOnlyList{LogLineIndex}, ILogBuffer, int, LogSourceQueryOptions)"/>
		///    request, one has to make certain to retrieve a particular column from the source. Nothing needs to be done
		///    if the caller is also interested in said column, however if the caller is not, then it becomes a one liner
		///    to create a combined buffer which acts as a proxy for the original for every column but the desired one,
		///    which is copied into an additional buffer.
		///
		///    If it were permissible to query the source log file countless times, then this method wouldn't need to exist,
		///    however since we don't want to do exactly that, we'll have to do this procedure to improve performance, now
		///    that log files are streamed into memory on-demand.
		/// </remarks>
		/// <param name="that"></param>
		/// <param name="columns"></param>
		/// <returns></returns>
		public static ILogBuffer CreateViewWithAdditionalColumns(this ILogBuffer that, IReadOnlyList<IColumnDescriptor> columns)
		{
			if (columns.Count == 0)
				return that;

			var missingColumns = columns.Except(that.Columns).ToList();
			if (missingColumns.Count == 0)
				return that;

			var temporaryBuffer = new LogBufferArray(that.Count, missingColumns);
			var combinedView = new CombinedLogBufferView(new[] {that, temporaryBuffer});
			return combinedView;
		}
		
		/// <summary>
		///    Returns a new log buffer which acts as a view onto the original buffer.
		///    If the original buffer ONLY contains the desired column, then the original buffer is returned.
		///    Otherwise a new temporary buffer is returned with the same size as the original buffer containing
		///    only the given column.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public static ILogBuffer CreateViewOnlyWithColumn(this ILogBuffer that, IColumnDescriptor column)
		{
			if (that.Columns.Count == 1 && that.Contains(column))
				return that;

			var temporaryBuffer = new LogBufferArray(that.Count, column);
			return temporaryBuffer;
		}
	}
}
