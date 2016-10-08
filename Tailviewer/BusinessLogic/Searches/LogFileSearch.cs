using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using log4net;

namespace Tailviewer.BusinessLogic.Searches
{
	/// <summary>
	///     Responsible for searching for a given term in an <see cref="ILogFile" />.
	/// </summary>
	public sealed class LogFileSearch
		: ILogFileSearch
		, IDisposable
		, ILogFileListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ManualResetEventSlim _finished;
		private readonly SubstringFilter _filter;
		private readonly List<ILogFileSearchListener> _listeners;
		private readonly ILogFile _logFile;
		private readonly LogLine[] _logLinesBuffer;
		private readonly List<LogMatch> _matches;
		private readonly List<LogLineMatch> _matchesBuffer;
		private readonly TimeSpan _maximumWaitTime;
		private readonly ConcurrentQueue<LogFileSection> _pendingModifications;
		private readonly Thread _searchThread;
		private readonly object _syncRoot;
		private volatile bool _isDisposed;

		public LogFileSearch(ILogFile logFile, string searchTerm)
			: this(logFile, searchTerm, TimeSpan.FromMilliseconds(10))
		{}

		public LogFileSearch(ILogFile logFile, string searchTerm, TimeSpan maximumWaitTime)
		{
			if (logFile == null)
				throw new ArgumentNullException("logFile");
			if (string.IsNullOrEmpty(searchTerm))
				throw new ArgumentException("searchTerm may not be empty");

			_logFile = logFile;
			_filter = new SubstringFilter(searchTerm, true);
			_finished = new ManualResetEventSlim();
			_matches = new List<LogMatch>();
			_syncRoot = new object();
			_listeners = new List<ILogFileSearchListener>();
			_pendingModifications = new ConcurrentQueue<LogFileSection>();
			_searchThread = new Thread(DoFilter)
				{
					Name = string.Format("Search {0}", logFile),
					IsBackground = true
				};

			const int maximumLineCount = 1000;
			_maximumWaitTime = maximumWaitTime;
			_logLinesBuffer = new LogLine[maximumLineCount];
			_matchesBuffer = new List<LogLineMatch>();
			_logFile.AddListener(this, _maximumWaitTime, maximumLineCount);
			_searchThread.Start();
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

		public void Dispose()
		{
			_logFile.RemoveListener(this);
			_isDisposed = true;
			if (!_searchThread.Join(TimeSpan.FromSeconds(1)))
			{
				Log.WarnFormat("Failed to join search thread in 1 second, continuing anyways...");
			}
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			_finished.Reset();
			_pendingModifications.Enqueue(section);
		}

		public void AddListener(ILogFileSearchListener listener)
		{
			lock (_syncRoot)
			{
				_listeners.Add(listener);
				listener.OnSearchModified(this, _matches.ToList());
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
			var started = DateTime.Now;
			if (!_logFile.Wait(maximumWaitTime))
				return false;

			var elapsed = DateTime.Now - started;
			var remaining = maximumWaitTime - elapsed;
			if (remaining < TimeSpan.Zero)
				remaining = TimeSpan.Zero;

			return _finished.Wait(remaining);
		}

		public void Wait()
		{
			_logFile.Wait();
			_finished.Wait();
		}

		private void DoFilter()
		{
			while (!_isDisposed)
			{
				try
				{
					FilterAllPending();

					Thread.Sleep(_maximumWaitTime);
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Cauhgt unexpected exception: {0}", e);
				}
			}
		}

		private void FilterAllPending()
		{
			LogFileSection section;
			while (_pendingModifications.TryDequeue(out section))
			{
				if (section.IsReset)
				{
					lock (_syncRoot)
					{
						_matches.Clear();
						NotifyListeners();
					}
				}
				else
				{
					AppendMatches(section);
				}
			}

			_finished.Set();
		}

		private void AppendMatches(LogFileSection section)
		{
			try
			{
				// We've instructed the logfile to give us exactly up to
				// _logLinesBuffer.Length amount of entries in the ctor, hence the following
				// is correct:
				_logFile.GetSection(section, _logLinesBuffer);

				foreach (LogLine line in _logLinesBuffer)
				{
					_filter.Match(line, _matchesBuffer);
					if (_matchesBuffer.Count > 0)
					{
						lock (_syncRoot)
						{
							foreach (var logLineMatch in _matchesBuffer)
							{
								var match = new LogMatch(line.LineIndex, logLineMatch);
								_matches.Add(match);
							}
							NotifyListeners();
						}
						_matchesBuffer.Clear();
					}
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

		private void NotifyListeners()
		{
			foreach (ILogFileSearchListener listener in _listeners)
			{
				listener.OnSearchModified(this, _matches.ToList());
			}
		}
	}
}