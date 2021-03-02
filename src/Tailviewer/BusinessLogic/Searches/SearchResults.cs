using System.Collections.Generic;
using Tailviewer.Api;

namespace Tailviewer.BusinessLogic.Searches
{
	public sealed class SearchResults
		: ISearchResults
	{
		private readonly List<LogMatch> _matches;
		private readonly SearchResultsByLogLineIndex _matchesByLine;

		public SearchResults()
		{
			_matchesByLine = new SearchResultsByLogLineIndex();
			_matches = new List<LogMatch>();
		}

		public int Count
		{
			get { return _matches.Count; }
		}

		public ISearchResultsByLogLineIndex MatchesByLine
		{
			get { return _matchesByLine; }
		}

		public IReadOnlyList<LogMatch> Matches
		{
			get { return _matches; }
		}

		public void Add(LogLineIndex index, LogLineMatch match)
		{
			Add(new LogMatch(index, match));
		}

		public void Add(LogMatch match)
		{
			_matchesByLine.Add(match);
			_matches.Add(match);
		}

		public void Add(IEnumerable<LogMatch> matches)
		{
			foreach (LogMatch match in matches)
			{
				Add(match);
			}
		}

		public void Clear()
		{
			_matches.Clear();
			_matchesByLine.Clear();
		}
	}
}