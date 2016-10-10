using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Searches
{
	public sealed class SearchResults
		: ISearchResults
	{
		private readonly Dictionary<LogLineIndex, List<LogLineMatch>> _matchesByLine;
		private readonly List<LogMatch> _matches;

		public SearchResults()
		{
			_matchesByLine = new Dictionary<LogLineIndex, List<LogLineMatch>>();
			_matches = new List<LogMatch>();
		}

		public void Add(LogLineIndex index, LogLineMatch match)
		{
			Add(new LogMatch(index, match));
		}

		public void Add(LogMatch match)
		{
			_matches.Add(match);

			List<LogLineMatch> lineMatches;
			if (!_matchesByLine.TryGetValue(match.Index, out lineMatches))
			{
				lineMatches = new List<LogLineMatch>();
				_matchesByLine.Add(match.Index, lineMatches);
			}
			lineMatches.Add(match.Match);
		}

		public void Add(IEnumerable<LogMatch> matches)
		{
			foreach (var match in matches)
			{
				Add(match);
			}
		}

		public void Clear()
		{
			_matches.Clear();
			_matchesByLine.Clear();
		}

		public int Count
		{
			get { return _matches.Count; }
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
	}
}