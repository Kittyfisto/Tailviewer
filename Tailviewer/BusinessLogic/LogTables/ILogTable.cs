using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.LogTables
{
	/// <summary>
	///     The interface for a tabular view onto a log file.
	///     It offers a more structured view of the log file's data, contrary to <see cref="ILogFile" />
	///     which offers each line as a string, basically.
	/// </summary>
	public interface ILogTable
	{
		/// <summary>
		///     The number of rows in this table.
		/// </summary>
		int RowCount { get; }

		/// <summary>
		///     The number of columns in this table.
		/// </summary>
		int ColumnCount { get; }

		/// <summary>
		///     Allows access to the n-th row of this table.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		LogTableRow this[int index] { get; }
	}
}