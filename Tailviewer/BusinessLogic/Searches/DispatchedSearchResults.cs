using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Searches
{
	public sealed class DispatchedSearchResults
		: ISearchResults
		, ILogFileSearchListener
	{
		private readonly SearchResults _actualResults;
		private readonly object _syncRoot;

		private bool _modified;
		private List<LogMatch> _matches;

		public DispatchedSearchResults()
		{
			_actualResults = new SearchResults();
			_syncRoot = new object();
		}

		public bool Update()
		{
			lock (_syncRoot)
			{
				if (!_modified)
					return false;

				_actualResults.Clear();
				_actualResults.Add(_matches);

				_modified = false;
				return true;
			}
		}

		public int Count
		{
			get { return _actualResults.Count; }
		}

		public IEnumerable<LogMatch> Matches
		{
			get { return _actualResults.Matches; }
		}

		public bool TryGetMatches(LogLineIndex index, out IEnumerable<LogLineMatch> matches)
		{
			return _actualResults.TryGetMatches(index, out matches);
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
			lock (_syncRoot)
			{
				_matches = matches.ToList();
				_modified = true;
			}
		}
	}
}