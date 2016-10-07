using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.Searches
{
	public interface ILogFileSearch
	{
		IEnumerable<LogMatch> Matches { get; }
		void AddListener(ILogFileSearchListener listener);
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