using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Searches
{
	public sealed class SearchResultsListener
		: ILogFileSearchListener
		, ISearchResults
	{
		private readonly Dictionary<LogLineIndex, List<LogLineMatch>> _matchesByLine;
		private List<LogMatch> _matches;

		public SearchResultsListener()
		{
			_matchesByLine = new Dictionary<LogLineIndex, List<LogLineMatch>>();
			_matches = new List<LogMatch>();
		}

		public int Count
		{
			get { return _matchesByLine.Count; }
		}

		public IEnumerable<LogMatch> Matches
		{
			get { return _matches; }
		}

		public bool TryGetMatches(LogLineIndex index, out IEnumerable<LogLineMatch> matches)
		{
			List<LogLineMatch> values;
			if (_matchesByLine.TryGetValue(index, out values))
			{
				matches = values;
				return true;
			}

			matches = Enumerable.Empty<LogLineMatch>();
			return false;
		}

		public IEnumerable<LogLineMatch> this[int index]
		{
			get
			{
				IEnumerable<LogLineMatch> matches;
				TryGetMatches(index, out matches);
				return matches;
			}
		}

		public void OnSearchModified(ILogFileSearch sender, IEnumerable<LogMatch> matches)
		{
			_matches = matches.ToList();
			_matchesByLine.Clear();
			foreach (var match in _matches)
			{
				List<LogLineMatch> lineMatches;
				if (!_matchesByLine.TryGetValue(match.Index, out lineMatches))
				{
					lineMatches = new List<LogLineMatch>();
					_matchesByLine.Add(match.Index, lineMatches);
				}
				lineMatches.Add(match.Match);
			}
		}
	}
}