using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.LogTables.Parsers
{
	/// <summary>
	///     A log file parser is responsible for separating a given <see cref="LogLine" /> according
	///     to its own rules into different columns, yielding a single <see cref="LogTableRow" />.
	///     The row contains as many fields as the table has columns with each field corresponding
	///     to the cell at the given row and column.
	/// </summary>
	public interface ILogFileParser
	{
		/// <summary>
		///     Parses the given line into a row.
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		[Pure]
		LogTableRow Parse(LogLine line);

		/// <summary>
		///     Parses the given multiline entry into a row.
		/// </summary>
		/// <param name="entry"></param>
		/// <returns></returns>
		[Pure]
		LogTableRow Parse(IEnumerable<LogLine> entry);
	}
}