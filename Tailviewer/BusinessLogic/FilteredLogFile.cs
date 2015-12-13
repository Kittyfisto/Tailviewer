using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Tailviewer.BusinessLogic
{
	public sealed class FilteredLogFile
		: ILogFile
		  , ILogFileListener
	{
		private const int BatchSize = 1000;

		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly ManualResetEvent _endOfSectionHandle;
		private readonly string _stringFilter;
		private readonly LevelFlags _levelFilter;
		private readonly List<int> _indices;
		private readonly LogFileListenerCollection _listeners;
		private readonly ConcurrentQueue<LogFileSection> _pendingModifications;
		private readonly ILogFile _source;
		private readonly Task _readTask;
		private LogFileSection _fullSection;

		public FilteredLogFile(ILogFile source, string stringFilter, LevelFlags levelFilter)
		{
			if (source == null) throw new ArgumentNullException("source");

			_source = source;
			_levelFilter = levelFilter;
			_stringFilter = stringFilter ?? string.Empty;
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

		public void Start(TimeSpan maximumWaitTime)
		{
			if (_readTask.Status == TaskStatus.Created)
			{
				_source.AddListener(this, maximumWaitTime, BatchSize);
				_readTask.Start();
			}
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

		public void GetSection(LogFileSection section, LogEntry[] dest)
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
					var entry = _source.GetEntry(sourceIndex);
					dest[i] = entry;
				}
			}
		}

		public LogEntry GetEntry(int index)
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
		}

		private void Filter(object parameter)
		{
			CancellationToken token = _cancellationTokenSource.Token;
			var entries = new LogEntry[BatchSize];
			int currentSourceIndex = 0;

			while (!token.IsCancellationRequested)
			{
				LogFileSection section;
				while (_pendingModifications.TryDequeue(out section))
				{
					if (section == LogFileSection.Reset)
					{
						_fullSection = new LogFileSection();
						_indices.Clear();
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
					_endOfSectionHandle.Set();

					_listeners.OnRead(_indices.Count);

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
						LogEntry entry = entries[i];
						if (PassesFilter(entry))
						{
							int sourceIndex = nextSection.Index + i;
							_indices.Add(sourceIndex);
							_listeners.OnRead(_indices.Count);
						}
					}

					currentSourceIndex += nextCount;
				}
			}
		}

		[Pure]
		private bool PassesFilter(LogEntry entry)
		{
			int idx = entry.Message.IndexOf(_stringFilter, StringComparison.InvariantCultureIgnoreCase);
			if (idx == -1)
				return false;

			if ((entry.Level & _levelFilter) != 0)
				return true;

			if (entry.Level == LevelFlags.None)
				return true;

			return false;
		}
	}
}