using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tailviewer.BusinessLogic.Searches
{
	public sealed class LogFileSearchProxy
		: ILogFileSearch
		, ILogFileSearchListener
		, IDisposable
	{
		private readonly List<ILogFileSearchListener> _listeners;
		private readonly ConcurrentQueue<KeyValuePair<ILogFileSearch, List<LogMatch>>> _pendingMatches;
		private readonly object _syncRoot;
		private readonly ITaskScheduler _taskScheduler;
		private readonly IPeriodicTask _task;

		private ILogFileSearch _innerSearch;
		private List<LogMatch> _matches;
		private bool _isDisposed;

		public LogFileSearchProxy(ITaskScheduler taskScheduler)
		{
			if (taskScheduler == null)
				throw new ArgumentNullException("taskScheduler");

			_pendingMatches = new ConcurrentQueue<KeyValuePair<ILogFileSearch, List<LogMatch>>>();
			_listeners = new List<ILogFileSearchListener>();
			_taskScheduler = taskScheduler;
			_syncRoot = new object();
			_matches = new List<LogMatch>();

			_task = _taskScheduler.StartPeriodic(RunOnce, TimeSpan.FromMilliseconds(100), "Search Proxy");
		}

		public LogFileSearchProxy(ITaskScheduler taskScheduler, ILogFileSearch innerSearch)
			: this(taskScheduler)
		{
			InnerSearch = innerSearch;
		}

		public void Dispose()
		{
			_taskScheduler.StopPeriodic(_task);
			_isDisposed = true;
		}

		public bool IsDisposed
		{
			get { return _isDisposed; }
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

		public void OnSearchModified(ILogFileSearch sender, List<LogMatch> matches)
		{
			_pendingMatches.Enqueue(new KeyValuePair<ILogFileSearch, List<LogMatch>>(sender, matches));
		}

		private void RunOnce()
		{
			KeyValuePair<ILogFileSearch, List<LogMatch>> pair;
			while (_pendingMatches.TryDequeue(out pair))
			{
				var sender = pair.Key;
				var matches = pair.Value;

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
			foreach (ILogFileSearchListener listener in _listeners)
			{
				listener.OnSearchModified(this, matches.ToList());
			}
		}
	}
}