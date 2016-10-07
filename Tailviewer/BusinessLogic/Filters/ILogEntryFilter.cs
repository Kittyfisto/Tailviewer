using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Filters
{
	public interface ILogEntryFilter
	{
		/// <summary>
		///     Tests if the given entry passes the filter.
		/// </summary>
		/// <param name="logEntry"></param>
		/// <returns></returns>
		[Pure]
		bool PassesFilter(IEnumerable<LogLine> logEntry);

		[Pure]
		bool PassesFilter(LogLine logLine);

		/// <summary>
		/// Looks for matches of this filter in the given line and returns a list of them
		/// where each entry marks the Start and Length of the match, relative to the start of the line.
		/// </summary>
		/// <returns></returns>
		List<LogLineMatch> Match(LogLine line);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="line"></param>
		/// <param name="matches"></param>
		void Match(LogLine line, List<LogLineMatch> matches);
	}
}