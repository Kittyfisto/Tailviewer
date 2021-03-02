using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.Api;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Searches
{
	/// <summary>
	///     Responsible for searching for a given term in an <see cref="ILogSource" />.
	/// </summary>
	public sealed class LogSourceSearch
		: ILogSourceSearch
		, ILogSourceListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly LogFileSearchListenerCollection _listeners;
		private readonly SubstringFilter _filter;
		private readonly ILogSource _logSource;
		private LogBufferArray _logLinesArray;
		private readonly List<LogMatch> _matches;
		private readonly List<LogLineMatch> _matchesBuffer;
		private readonly TimeSpan _maximumWaitTime;
		private readonly ConcurrentQueue<LogSourceModification> _pendingModifications;
		private readonly IPeriodicTask _task;
		private readonly ITaskScheduler _scheduler;
		private readonly object _syncRoot;
		private bool _isDisposed;

		public LogSourceSearch(ITaskScheduler taskScheduler, ILogSource logSource, string searchTerm)
			: this(taskScheduler, logSource, searchTerm, TimeSpan.FromMilliseconds(10))
		{
		}

		public LogSourceSearch(ITaskScheduler taskScheduler, ILogSource logSource, string searchTerm, TimeSpan maximumWaitTime)
		{
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));
			if (logSource == null)
				throw new ArgumentNullException(nameof(logSource));
			if (string.IsNullOrEmpty(searchTerm))
				throw new ArgumentException("searchTerm may not be empty");

			_logSource = logSource;
			_filter = new SubstringFilter(searchTerm, true);
			_matches = new List<LogMatch>();
			_syncRoot = new object();
			_listeners = new LogFileSearchListenerCollection(this);
			_pendingModifications = new ConcurrentQueue<LogSourceModification>();
			_scheduler = taskScheduler;

			const int maximumLineCount = 1000;
			_maximumWaitTime = maximumWaitTime;
			_logLinesArray = new LogBufferArray(maximumLineCount, GeneralColumns.Index, GeneralColumns.RawContent);
			_matchesBuffer = new List<LogLineMatch>();
			_logSource.AddListener(this, _maximumWaitTime, maximumLineCount);

			_task = _scheduler.StartPeriodic(FilterAllPending,
			                                 TimeSpan.FromMilliseconds(100),
			                                 string.Format("Search {0}", logSource));
		}

		public void Dispose()
		{
			_logSource.RemoveListener(this);
			_scheduler.StopPeriodic(_task);

			lock (_syncRoot)
			{
				_matches.Clear();
				_matches.Capacity = 0;

				_matchesBuffer.Clear();
				_matchesBuffer.Capacity = 0;

				_logLinesArray = null;

				_pendingModifications.Clear();
			}

			_isDisposed = true;
		}

		public void OnLogFileModified(ILogSource logSource, LogSourceModification modification)
		{
			_pendingModifications.Enqueue(modification);
		}

		public IEnumerable<LogMatch> Matches
		{
			get
			{
				lock (_syncRoot)
				{
					return _matches.ToList();
				}
			}
		}

		public bool IsDisposed
		{
			get { return _isDisposed; }
		}

		public int Count
		{
			get
			{
				lock (_syncRoot)
				{
					return _matches.Count;
				}
			}
		}

		public void AddListener(ILogFileSearchListener listener)
		{
			_listeners.AddListener(listener);
		}

		public void RemoveListener(ILogFileSearchListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		private void FilterAllPending()
		{
			while (_pendingModifications.TryDequeue(out var modification))
			{
				if (modification.IsReset())
				{
					lock (_syncRoot)
					{
						_matches.Clear();
					}

					_listeners.EmitSearchChanged(_matches);
				}
				else if (modification.IsRemoved(out var removedSection))
				{
					lock (_syncRoot)
					{
						int lastIndexInValidRange;
						for (lastIndexInValidRange = _matches.Count - 1; lastIndexInValidRange >= 0; --lastIndexInValidRange)
						{
							if (_matches[lastIndexInValidRange].Index < removedSection.Index)
								break;
						}

						if (lastIndexInValidRange < _matches.Count)
							_matches.RemoveRange(lastIndexInValidRange + 1, _matches.Count - lastIndexInValidRange - 1);
					}

					_listeners.EmitSearchChanged(_matches);
				}
				else if (modification.IsAppended(out var appendedSection))
				{
					AppendMatches(appendedSection);
				}
			}
		}

		private void AppendMatches(LogSourceSection section)
		{
			try
			{
				LogBufferArray lines;
				lock (_syncRoot)
				{
					lines = _logLinesArray;
					if (lines == null)
						return;
				}

				// We've instructed the logfile to give us exactly up to
				// _logLinesBuffer.Length amount of entries in the ctor, hence the following
				// is correct:
				_logSource.GetEntries(section, lines);

				bool added = false;
				for (int i = 0; i < section.Count; ++i)
				{
					var line = lines[i];

					_filter.Match(line, _matchesBuffer);
					if (_matchesBuffer.Count > 0)
					{
						lock (_syncRoot)
						{
							foreach (LogLineMatch logLineMatch in _matchesBuffer)
							{
								var match = new LogMatch(line.Index, logLineMatch);
								_matches.Add(match);
							}
						}

						_matchesBuffer.Clear();
						added = true;
					}
				}

				if (added)
				{
					_listeners.EmitSearchChanged(_matches);
				}
			}
			catch (IndexOutOfRangeException e)
			{
				// This exception is usually thrown when we access a portion of the
				// log file that has already been reset. This means that a reset event is
				// either pending or soon to be. So not doing anything else to handle
				// this exception is fine.
				Log.DebugFormat("Caught exception while searching log file: {0}", e);
			}
		}
	}
}