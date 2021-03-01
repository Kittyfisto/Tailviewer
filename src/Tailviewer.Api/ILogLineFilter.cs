using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Tailviewer
{
	/// <summary>
	/// The interface for a filter that is responsible for deciding whether or not a <see cref="IReadOnlyLogEntry"/> shall be visible
	/// or not.
	/// </summary>
	public interface ILogLineFilter
	{
		/// <summary>
		/// Tests if the given log line passes this filter.
		/// </summary>
		/// <param name="logLine"></param>
		/// <returns>true if the log line passes (and shall be displayed), false otherwise</returns>
		[Pure]
		bool PassesFilter(IReadOnlyLogEntry logLine);

		/// <summary>
		/// Looks for matches of this filter in the given line and returns a list of them
		/// where each entry marks the Start and Length of the match, relative to the start of the line.
		/// </summary>
		/// <returns></returns>
		List<LogLineMatch> Match(IReadOnlyLogEntry line);
	}
}