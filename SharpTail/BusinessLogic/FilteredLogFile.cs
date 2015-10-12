using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace SharpTail.BusinessLogic
{
	public sealed class FilteredLogFile
		: ILogFile
		  , ILogFileListener
	{
		private const int BatchSize = 1000;

		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly ManualResetEvent _endOfSectionHandle;
		private readonly string _filterString;
		private readonly List<int> _indices;
		private readonly LogFileListenerCollection _listeners;
		private readonly ConcurrentQueue<LogFileSection> _pendingModifications;
		private readonly ILogFile _source;
		private readonly Task _task;
		private LogFileSection _fullSection;

		public FilteredLogFile(ILogFile source, string filterString)
		{
			if (source == null) throw new ArgumentNullException("source");

			_source = source;
			_filterString = filterString ?? string.Empty;
			_cancellationTokenSource = new CancellationTokenSource();
			_endOfSectionHandle = new ManualResetEvent(false);
			_task = new Task(Filter,
			                 _cancellationTokenSource.Token,
			                 _cancellationTokenSource.Token);
			_listeners = new LogFileListenerCollection();
			_pendingModifications = new ConcurrentQueue<LogFileSection>();
			_indices = new List<int>();
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_task.Wait();
			_task.Dispose();
		}

		public void Start()
		{
			if (_task.Status == TaskStatus.Created)
			{
				_source.AddListener(this, TimeSpan.FromMilliseconds(10), BatchSize);
				_task.Start();
			}
		}

		public void Wait()
		{
			_endOfSectionHandle.WaitOne();
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

		public void GetSection(LogFileSection section, string[] dest)
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

		public string GetEntry(int index)
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
			var entries = new string[BatchSize];
			int currentSourceIndex = 0;

			while (!token.IsCancellationRequested)
			{
				LogFileSection section;
				while (_pendingModifications.TryDequeue(out section))
				{
					_fullSection = LogFileSection.MinimumBoundingLine(_fullSection, section);
				}

				if (_fullSection.IsEndOfSection(currentSourceIndex))
				{
					_endOfSectionHandle.Set();

					_listeners.OnLineRead(_indices.Count - 1);

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
						string entry = entries[i];
						if (PassesFilter(entry))
						{
							int sourceIndex = nextSection.Index + i;
							_indices.Add(sourceIndex);
							_listeners.OnLineRead(_indices.Count - 1);
						}
					}

					currentSourceIndex += nextCount;
				}
			}
		}

		[Pure]
		private bool PassesFilter(string entry)
		{
			string filter = _filterString;
			if (string.IsNullOrEmpty(filter))
				return true;

			int idx = entry.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase);
			return idx != -1;
		}
	}
}