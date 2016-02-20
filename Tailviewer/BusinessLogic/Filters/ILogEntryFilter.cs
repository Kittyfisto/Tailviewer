using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Filters
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