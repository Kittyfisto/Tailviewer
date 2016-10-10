using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Searches
{
	public interface ISearchResults
	{
		int Count { get; }
		IEnumerable<LogMatch> Matches { get; }
		bool TryGetMatches(LogLineIndex index, out IEnumerable<LogLineMatch> matches);
		IEnumerable<LogLineMatch> this[int index] { get; }
	}
}