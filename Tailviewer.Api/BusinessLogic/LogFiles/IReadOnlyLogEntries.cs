using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Provides read-only access to a list of log entries.
	/// </summary>
	/// <remarks>
	///     The order/nature of these entries is undefined.
	/// </remarks>
	public interface IReadOnlyLogEntries
		: IReadOnlyList<ILogEntry>
	{
		/// <summary>
		///     The list of columns to store in this buffer.
		/// </summary>
		IReadOnlyList<ILogFileColumn> Columns { get; }
	}
}