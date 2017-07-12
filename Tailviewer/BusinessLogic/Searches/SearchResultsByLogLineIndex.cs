using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Searches
{
	public sealed class SearchResultsByLogLineIndex
		: ISearchResultsByLogLineIndex
	{
		private readonly Dictionary<LogLineIndex, List<LogLineMatch>> _matches;

		public SearchResultsByLogLineIndex()
		{
			_matches = new Dictionary<LogLineIndex, List<LogLineMatch>>();
		}

		public void Add(LogLineIndex index, LogLineMatch match)
		{
			Add(new LogMatch(index, match));
		}

		public void Add(LogMatch match)
		{
			List<LogLineMatch> lineMatches;
			if (!_matches.TryGetValue(match.Index, out lineMatches))
			{
				lineMatches = new List<LogLineMatch>();
				_matches.Add(match.Index, lineMatches);
			}
			lineMatches.Add(match.Match);
		}

		public void Clear()
		{
			_matches.Clear();
		}

		public int Count
		{
			get { return _matches.Count; }
		}

		public IEnumerator<KeyValuePair<LogLineIndex, IEnumerable<LogLineMatch>>> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool ContainsKey(LogLineIndex key)
		{
			throw new System.NotImplementedException();
		}

		public bool TryGetValue(LogLineIndex key, out IEnumerable<LogLineMatch> value)
		{
			List<LogLineMatch> values;
			if (_matches.TryGetValue(key, out values))
			{
				value = values;
				return true;
			}

			value = Enumerable.Empty<LogLineMatch>();
			return false;
		}

		IEnumerable<LogLineMatch> IReadOnlyDictionary<LogLineIndex, IEnumerable<LogLineMatch>>.this[LogLineIndex key]
		{
			get
			{
				IEnumerable<LogLineMatch> value;
				TryGetValue(key, out value);
				return value;
			}
		}

		public IEnumerable<LogLineIndex> Keys
		{
			get { return _matches.Keys; }
		}

		public IEnumerable<IEnumerable<LogLineMatch>> Values
		{
			get { return _matches.Values; }
		}
	}
}