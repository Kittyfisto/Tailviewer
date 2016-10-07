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

		public LogFileSearchProxy()
		{
			_listeners = new List<ILogFileSearchListener>();
			_syncRoot = new object();
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
				if (value == _innerSearch)
					return;

				if(_innerSearch != null)
					_innerSearch.RemoveListener(this);
				_innerSearch = value;
				if (_innerSearch != null)
					_innerSearch.AddListener(this);

				OnSearchModified(Matches.ToList());
			}
		}

		public IEnumerable<LogMatch> Matches
		{
			get
			{
				var search = _innerSearch;
				if (search != null)
					return search.Matches;

				return Enumerable.Empty<LogMatch>();
			}
		}

		public void AddListener(ILogFileSearchListener listener)
		{
			lock (_syncRoot)
			{
				_listeners.Add(listener);
				listener.OnSearchModified(Matches.ToList());
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
			var search = _innerSearch;
			if (search != null)
				return search.Wait(maximumWaitTime);

			return true;
		}

		public void Wait()
		{
			var search = _innerSearch;
			if (search != null)
				search.Wait();
		}

		public void OnSearchModified(List<LogMatch> matches)
		{
			lock (_syncRoot)
			{
				foreach (var listener in _listeners)
				{
					listener.OnSearchModified(matches);
				}
			}
		}
	}
}