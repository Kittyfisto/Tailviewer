using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Tailviewer
{
	/// <summary>
	/// The interface for a filter that is responsible for deciding whether or not an entire log entry shall be visible
	/// or not.
	/// </summary>
	public interface ILogEntryFilter
		: ILogLineFilter
	{
		/// <summary>
		///     Tests if the given entry passes the filter.
		/// </summary>
		/// <param name="logEntry"></param>
		/// <returns></returns>
		[Pure]
		bool PassesFilter(IEnumerable<IReadOnlyLogEntry> logEntry);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="line"></param>
		/// <param name="matches"></param>
		void Match(IReadOnlyLogEntry line, List<LogLineMatch> matches);
	}
}