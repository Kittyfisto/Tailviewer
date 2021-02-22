using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Tailviewer.BusinessLogic.Searches
{
	public sealed class LogSourceSearchProxy
		: ILogSourceSearch
		, ILogFileSearchListener
	{
		private readonly List<ILogFileSearchListener> _listeners;
		private readonly ILogSource _logSource;
		private readonly TimeSpan _maximumWaitTime;
		private readonly ConcurrentQueue<KeyValuePair<ILogSourceSearch, List<LogMatch>>> _pendingMatches;
		private readonly object _syncRoot;
		private readonly IPeriodicTask _task;
		private readonly ITaskScheduler _taskScheduler;

		private LogSourceSearch _innerSearch;
		private bool _isDisposed;
		private List<LogMatch> _matches;
		private string _searchTerm;

		public LogSourceSearchProxy(ITaskScheduler taskScheduler, ILogSource logSource, TimeSpan maximumWaitTime)
		{
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));
			if (logSource == null)
				throw new ArgumentNullException(nameof(logSource));

			_pendingMatches = new ConcurrentQueue<KeyValuePair<ILogSourceSearch, List<LogMatch>>>();
			_logSource = logSource;
			_listeners = new List<ILogFileSearchListener>();
			_taskScheduler = taskScheduler;
			_syncRoot = new object();
			_matches = new List<LogMatch>();
			_maximumWaitTime = maximumWaitTime;

			_task = _taskScheduler.StartPeriodic(RunOnce, _maximumWaitTime, "Search Proxy");
		}

		public bool IsDisposed
		{
			get { return _isDisposed; }
		}

		public string SearchTerm
		{
			get { return _searchTerm; }
			set
			{
				if (value == _searchTerm)
					return;

				_searchTerm = value;
				InnerSearch = !string.IsNullOrEmpty(value)
					              ? CreateNewSearch(_taskScheduler, _logSource, value, _maximumWaitTime)
					              : null;
			}
		}

		private LogSourceSearch InnerSearch
		{
			set
			{
				lock (_syncRoot)
				{
					if (_innerSearch != null)
					{
						_innerSearch.RemoveListener(this);
						_innerSearch.Dispose();
					}

					_innerSearch = value;

					if (_innerSearch != null)
					{
						// When we have a new search then attaching ourselves as a listener is enough
						// to be notified about that searches current status.
						_innerSearch.AddListener(this);
					}
					else
					{
						// However when we're placing the old search with a null search, then
						// we must notify listeners about that (otherwise they still think the last
						// match is the current state, which it is obviously no longer).
						_pendingMatches.Enqueue(new KeyValuePair<ILogSourceSearch, List<LogMatch>>(null, new List<LogMatch>()));
					}
				}
			}
		}

		public void Dispose()
		{
			_innerSearch?.Dispose();

			_taskScheduler.StopPeriodic(_task);
			_isDisposed = true;
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

		public void OnSearchModified(ILogSourceSearch sender, IEnumerable<LogMatch> matches)
		{
			KeyValuePair<ILogSourceSearch, List<LogMatch>> unused;
			while(_pendingMatches.TryDequeue(out unused))
			{}

			_pendingMatches.Enqueue(new KeyValuePair<ILogSourceSearch, List<LogMatch>>(sender, matches.ToList()));
		}

		private static LogSourceSearch CreateNewSearch(ITaskScheduler scheduler, ILogSource logfile, string searchterm,
		                                              TimeSpan maximumwaittime)
		{
			return new LogSourceSearch(scheduler, logfile, searchterm, maximumwaittime);
		}

		private void RunOnce()
		{
			KeyValuePair<ILogSourceSearch, List<LogMatch>> pair;
			while (_pendingMatches.TryDequeue(out pair))
			{
				ILogSourceSearch sender = pair.Key;
				List<LogMatch> matches = pair.Value;

				// We need to make sure that we don't forward search results from a previously
				// _innerSearch. We can do so by ensuring that the sender is most definately
				// our current _innerSearch (and also by ensuring a correct order when replacing
				// it).
				if (sender == _innerSearch)
				{
					_matches = matches;
					EmitSearchModified(_matches);
				}
			}
		}

		private void EmitSearchModified(IEnumerable<LogMatch> matches)
		{
			lock (_syncRoot)
			{
				foreach (ILogFileSearchListener listener in _listeners)
				{
					listener.OnSearchModified(this, matches.ToList());
				}
			}
		}
	}
}