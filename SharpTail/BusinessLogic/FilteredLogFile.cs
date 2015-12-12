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
		private readonly string[] _levels;
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
			_levels = GetLevelStrings(levelFilter, stringFilter);
			_filterString = stringFilter ?? string.Empty;
			_cancellationTokenSource = new CancellationTokenSource();
			_endOfSectionHandle = new ManualResetEvent(false);
			_readTask = new Task(Filter,
			                 _cancellationTokenSource.Token,
			                 _cancellationTokenSource.Token);
			_listeners = new LogFileListenerCollection();
			_pendingModifications = new ConcurrentQueue<LogFileSection>();
			_indices = new List<int>();
		}

		private string[] GetLevelStrings(LevelFlags levelFilter, string stringFilter)
		{
			var ret = new List<string>();

			if (levelFilter.HasFlag(LevelFlags.Debug))
				ret.Add("DEBUG");
			if (levelFilter.HasFlag(LevelFlags.Info))
				ret.Add("INFO");
			if (levelFilter.HasFlag(LevelFlags.Warning))
				ret.Add("WARN");
			if (levelFilter.HasFlag(LevelFlags.Error))
				ret.Add("ERROR");
			if (levelFilter.HasFlag(LevelFlags.Fatal))
				ret.Add("FATAL");

			if (!string.IsNullOrEmpty(stringFilter))
				ret.Add(stringFilter);

			return ret.ToArray();
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_readTask.Wait();
			_readTask.Dispose();
		}

		public void Start()
		{
			if (_readTask.Status == TaskStatus.Created)
			{
				_source.AddListener(this, TimeSpan.FromMilliseconds(10), BatchSize);
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
						string entry = entries[i];
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
		private bool PassesFilter(string entry)
		{
			int idx = entry.IndexOf(_filterString, StringComparison.InvariantCultureIgnoreCase);
			if (idx == -1)
				return false;

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ForCanBeConvertedToForeach
			for (int i = 0; i < _levels.Length; ++i)
// ReSharper restore ForCanBeConvertedToForeach
// ReSharper restore LoopCanBeConvertedToQuery
			{
				var filter = _levels[i];
				idx = entry.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase);
				if (idx != -1)
					return true;
			}

			return false;
		}
	}
}