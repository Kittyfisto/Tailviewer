using System;
using System.Threading.Tasks;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.LogTables
{
	/// <summary>
	///     The interface for a tabular view onto a log file.
	///     It offers a more structured view of the log file's data, contrary to <see cref="ILogFile" />
	///     which offers each line as a string, basically.
	/// </summary>
	public interface ILogTable
		: IDisposable
	{
		/// <summary>
		///     The number of rows in this table.
		/// </summary>
		int RowCount { get; }

		/// <summary>
		///     Whether or not the datasource exists (is reachable).
		/// </summary>
		bool Exists { get; }

		/// <summary>
		///     The schema of this table.
		/// </summary>
		ILogTableSchema Schema { get; }

		/// <summary>
		///     Allows access to the n-th row of this table.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		Task<LogEntry> this[LogEntryIndex index] { get; }

		void AddListener(ILogTableListener listener, TimeSpan maximumWaitTime, int maximumLineCount);
		bool RemoveListener(ILogTableListener listener);
	}
}