using System;
using System.Collections.Generic;
using System.Linq;

namespace Tailviewer.BusinessLogic.Searches
{
	public sealed class LogFileSearchProxy
		: ILogFileSearch
		  , ILogFileSearchListener
	{
		private readonly List<ILogFileSearchListener> _listeners;
		private readonly object _syncRoot;
		private ILogFileSearch _innerSearch;
		private List<LogMatch> _matches;

		public LogFileSearchProxy()
		{
			_listeners = new List<ILogFileSearchListener>();
			_syncRoot = new object();
			_matches = new List<LogMatch>();
		}

		public LogFileSearchProxy(ILogFileSearch innerSearch)
			: this()
		{
			InnerSearch = innerSearch;
		}

		public ILogFileSearch InnerSearch
		{
			get { return _innerSearch; }
			set
			{
				lock (_syncRoot)
				{
					if (value == _innerSearch)
						return;

					if (_innerSearch != null)
						_innerSearch.RemoveListener(this);
					_innerSearch = value;

					// AddListener automatically forwards the current list of matches to us (and we, in turn, forward them as well).
					if (_innerSearch != null)
						_innerSearch.AddListener(this);
				}
			}
		}

		public IEnumerable<LogMatch> Matches
		{
			get { return _matches; }
		}

		public int Count
		{
			get { return _matches.Count; }
		}

		public void AddListener(ILogFileSearchListener listener)
		{
			lock (_syncRoot)
			{
				_listeners.Add(listener);
				listener.OnSearchModified(this, Matches.ToList());
			}
		}

		public void RemoveListener(ILogFileSearchListener listener)
		{
			lock (_syncRoot)
			{
				_listeners.Remove(listener);
			}
		}

		public bool Wait(TimeSpan maximumWaitTime)
		{
			ILogFileSearch search = _innerSearch;
			if (search != null)
				return search.Wait(maximumWaitTime);

			return true;
		}

		public void Wait()
		{
			ILogFileSearch search = _innerSearch;
			if (search != null)
				search.Wait();
		}

		public void OnSearchModified(ILogFileSearch sender, List<LogMatch> matches)
		{
			lock (_syncRoot)
			{
				// We need to make sure that we don't forward search results from a previously
				// _innerSearch. We can do so by ensuring that the sender is most definately
				// our current _innerSearch (and also by ensuring a correct order when replacing
				// it).
				if (sender != _innerSearch)
					return;

				_matches = matches;
				EmitSearchModified(_matches);
			}
		}

		private void EmitSearchModified(IEnumerable<LogMatch> matches)
		{
			foreach (ILogFileSearchListener listener in _listeners)
			{
				listener.OnSearchModified(this, matches.ToList());
			}
		}
	}
}