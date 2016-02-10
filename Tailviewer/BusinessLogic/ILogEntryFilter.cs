using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Tailviewer.BusinessLogic
{
	public interface ILogEntryFilter
	{
		/// <summary>
		/// Tests if the given entry passes the filter.
		/// </summary>
		/// <param name="logEntry"></param>
		/// <returns></returns>
		[Pure]
		bool PassesFilter(IEnumerable<LogLine> logEntry);

		[Pure]
		bool PassesFilter(LogLine logLine);
	}
}