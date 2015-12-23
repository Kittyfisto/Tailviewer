using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tailviewer.BusinessLogic
{
	internal sealed class FilteredLogFile
		: ILogFile
		  , ILogFileListener
	{
		private const int BatchSize = 1000;

		private readonly ILogEntryFilter _filter;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly ManualResetEvent _endOfSectionHandle;
		private readonly List<int> _indices;
		private readonly LogFileListenerCollection _listeners;
		private readonly ConcurrentQueue<LogFileSection> _pendingModifications;
		private readonly Task _readTask;
		private readonly ILogFile _source;
		private LogFileSection _fullSection;

		public FilteredLogFile(ILogFile source, ILogEntryFilter filter)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (filter == null) throw new ArgumentNullException("filter");

			_source = source;
			_filter = filter;
			_cancellationTokenSource = new CancellationTokenSource();
			_endOfSectionHandle = new ManualResetEvent(false);
			_readTask = new Task(Filter,
			                     _cancellationTokenSource.Token,
			                     _cancellationTokenSource.Token,
			                     TaskCreationOptions.LongRunning);
			_listeners = new LogFileListenerCollection();
			_pendingModifications = new ConcurrentQueue<LogFileSection>();
			_indices = new List<int>();
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_readTask.Wait();
			_readTask.Dispose();
		}

		public void Wait()
		{
			while (true)
			{
				if (_endOfSectionHandle.WaitOne(TimeSpan.FromMilliseconds(100)))
					break;

				if (_readTask.IsFaulted)
					throw _readTask.Exception;
			}
		}

		public int Count
		{
			get { return _indices.Count; }
		}

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void Remove(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public void GetSection(LogFileSection section, LogLine[] dest)
		{
			if (section.Index < 0)
				throw new ArgumentOutOfRangeException("section.Index");
			if (section.Count < 0)
				throw new ArgumentOutOfRangeException("section.Count");
			if (dest == null)
				throw new ArgumentNullException("dest");
			if (dest.Length < section.Count)
				throw new ArgumentOutOfRangeException("section.Count");

			lock (_indices)
			{
				if (section.Index + section.Count > _indices.Count)
					throw new ArgumentOutOfRangeException("section");

				for (int i = 0; i < section.Count; ++i)
				{
					int index = section.Index + i;
					int sourceIndex = _indices[index];
					LogLine line = _source.GetEntry(sourceIndex);
					dest[i] = line;
				}
			}
		}

		public LogLine GetEntry(int index)
		{
			lock (_indices)
			{
				int sourceIndex = _indices[index];
				return _source.GetEntry(sourceIndex);
			}
		}

		public void OnLogFileModified(LogFileSection section)
		{
			_pendingModifications.Enqueue(section);
			_endOfSectionHandle.Reset();
		}

		public void Start(TimeSpan maximumWaitTime)
		{
			if (_readTask.Status == TaskStatus.Created)
			{
				_source.AddListener(this, maximumWaitTime, BatchSize);
				_readTask.Start();
			}
		}

		private void Filter(object parameter)
		{
			CancellationToken token = _cancellationTokenSource.Token;
			var entries = new LogLine[BatchSize];
			int currentSourceIndex = 0;
			var lastLogEntry = new List<LogLine>();

			while (!token.IsCancellationRequested)
			{
				LogFileSection section;
				while (_pendingModifications.TryDequeue(out section))
				{
					if (section == LogFileSection.Reset)
					{
						_fullSection = new LogFileSection();
						_indices.Clear();

						lastLogEntry.Clear();
						currentSourceIndex = 0;

						_listeners.OnRead(-1);
					}
					else
					{
						_fullSection = LogFileSection.MinimumBoundingLine(_fullSection, section);
					}
				}

				if (_fullSection.IsEndOfSection(currentSourceIndex))
				{
					if (!TryAddLogLine(lastLogEntry))
						_listeners.OnRead(_indices.Count);

					_endOfSectionHandle.Set();

					// There's no more data, let's wait for more (or until we're disposed)
					token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(10));
				}
				else
				{
					int remaining = _fullSection.Index + _fullSection.Count - currentSourceIndex;
					int nextCount = Math.Min(remaining, BatchSize);
					var nextSection = new LogFileSection(currentSourceIndex, nextCount);
					_source.GetSection(nextSection, entries);

					for (int i = 0; i < nextCount; ++i)
					{
						if (token.IsCancellationRequested)
							break;

						LogLine line = entries[i];
						if (lastLogEntry.Count == 0 || lastLogEntry[0].LogEntryIndex == line.LogEntryIndex)
						{
							lastLogEntry.Add(line);
						}
						else if (line.LogEntryIndex != lastLogEntry[0].LogEntryIndex)
						{
							TryAddLogLine(lastLogEntry);
							lastLogEntry.Clear();
							lastLogEntry.Add(line);
						}
					}

					currentSourceIndex += nextCount;
				}
			}
		}

		private bool TryAddLogLine(List<LogLine> logEntry)
		{
			if (_indices.Count > 0 && logEntry.Count > 0 &&
			    _indices[_indices.Count - 1] == logEntry[logEntry.Count - 1].LineIndex)
				return true;

			if (_filter.PassesFilter(logEntry))
			{
				foreach (var line in logEntry)
				{
					_indices.Add(line.LineIndex);
				}
				_listeners.OnRead(_indices.Count);
				return true;
			}

			return false;
		}
	}
}