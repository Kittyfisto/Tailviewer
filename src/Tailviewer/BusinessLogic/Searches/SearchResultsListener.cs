using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.Searches
{
	public sealed class SearchResultsListener
		: ILogFileSearchListener
		, ISearchResults
	{
		private readonly SearchResults _results;

		public SearchResultsListener()
		{
			_results = new SearchResults();
		}

		public ISearchResultsByLogLineIndex MatchesByLine
		{
			get { return _results.MatchesByLine; }
		}

		IReadOnlyList<LogMatch> ISearchResults.Matches
		{
			get { return _results.Matches; }
		}

		public void OnSearchModified(ILogSourceSearch sender, IEnumerable<LogMatch> matches)
		{
			foreach (var match in matches)
			{
				_results.Add(match);
			}
		}
	}
}