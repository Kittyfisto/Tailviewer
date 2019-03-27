using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.Searches
{
	public interface ISearchResults
	{
		ISearchResultsByLogLineIndex MatchesByLine { get; }
		IReadOnlyList<LogMatch> Matches { get; }
	}
}