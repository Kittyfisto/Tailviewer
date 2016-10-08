using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.Searches
{
	public interface ILogFileSearch
	{
		/// <summary>
		///     The list of matches found in the source.
		///     A match marks the precise location (line index + location + length) in the source file, to allow
		///     for highlighting.
		/// </summary>
		IEnumerable<LogMatch> Matches { get; }

		/// <summary>
		///     The number of matches found in the source.
		/// </summary>
		int Count { get; }

		/// <summary>
		///     Adds the given listener to this search.
		///     The listener will immediately be notified of the current <see cref="Matches" />.
		///     Whenever new results arrive (or when the source is modified),
		///     the listener will be notified again (and again).
		/// </summary>
		/// <param name="listener"></param>
		void AddListener(ILogFileSearchListener listener);

		/// <summary>
		///     Removes the given listener from this search.
		///     It will no longer be notified.
		/// </summary>
		/// <param name="listener"></param>
		void RemoveListener(ILogFileSearchListener listener);

		/// <summary>
		///     Waits until the search is completed or the given time has elapsed.
		/// </summary>
		/// <param name="maximumWaitTime"></param>
		/// <returns></returns>
		bool Wait(TimeSpan maximumWaitTime);

		/// <summary>
		///     Waits until the search is completed.
		/// </summary>
		void Wait();
	}
}