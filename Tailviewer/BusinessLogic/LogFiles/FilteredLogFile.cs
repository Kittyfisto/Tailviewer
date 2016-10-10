using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Metrolib;
using Tailviewer.BusinessLogic.Filters;

namespace Tailviewer.BusinessLogic.LogFiles
{
	public sealed class FilteredLogFile
		: AbstractLogFile
		, ILogFileListener
	{
		private const int BatchSize = 10000;

		private readonly ILogEntryFilter _filter;
		private readonly List<int> _indices;
		private readonly ConcurrentQueue<LogFileSection> _pendingModifications;
		private readonly ILogFile _source;
		private readonly LogLine[] _buffer;
		private readonly TimeSpan _maximumWaitTime;

		private LogFileSection _fullSourceSection;
		private int _maxCharactersPerLine;
		private int _currentSourceIndex;
		private readonly List<LogLine> _lastLogEntry;

		public FilteredLogFile(ITaskScheduler scheduler, TimeSpan maximumWaitTime, ILogFile source, ILogEntryFilter filter)
			: base(scheduler)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (filter == null) throw new ArgumentNullException("filter");

			_source = source;
			_filter = filter;
			_pendingModifications = new ConcurrentQueue<LogFileSection>();
			_indices = new List<int>();
			_buffer = new LogLine[BatchSize];
			_lastLogEntry = new List<LogLine>();
			_maximumWaitTime = maximumWaitTime;

			_source.AddListener(this, maximumWaitTime, BatchSize);
			StartTask();
		}

		public override bool Exists
		{
			get { return _source.Exists; }
		}

		public override bool EndOfSourceReached
		{
			get { return _source.EndOfSourceReached & base.EndOfSourceReached; }
		}

		public override DateTime? StartTimestamp
		{
			get { return _source.StartTimestamp; }
		}

		public override DateTime LastModified
		{
			get { throw new NotImplementedException(); }
		}

		public override Size FileSize
		{
			get { throw new NotImplementedException(); }
		}

		public override int Count
		{
			get
			{
				lock (_indices)
				{
					return _indices.Count;
				}
			}
		}

		public override int MaxCharactersPerLine
		{
			get { return _maxCharactersPerLine; }
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			_pendingModifications.Enqueue(section);
			ResetEndOfSourceReached();
		}

		public override void GetSection(LogFileSection section, LogLine[] dest)
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
					LogLineIndex index = section.Index + i;
					int sourceIndex = _indices[(int) index];
					LogLine line = _source.GetLine(sourceIndex);
					dest[i] = new LogLine((int)index, line.LogEntryIndex, line.Message, line.Level, line.Timestamp);
				}
			}
		}

		public override LogLine GetLine(int index)
		{
			lock (_indices)
			{
				int sourceIndex = _indices[index];
				var line = _source.GetLine(sourceIndex);
				return new LogLine(index, line.LogEntryIndex, line.Message, line.Level, line.Timestamp);
			}
		}

		public override string ToString()
		{
			return string.Format("{0} (Filtered)", _source);
		}

		protected override TimeSpan RunOnce(CancellationToken token)
		{
			LogFileSection section;
			while (_pendingModifications.TryDequeue(out section) && !token.IsCancellationRequested)
			{
				if (section.IsReset)
				{
					Clear();
					_lastLogEntry.Clear();
					_currentSourceIndex = 0;
				}
				else if (section.InvalidateSection)
				{
					LogLineIndex startIndex = section.Index;
					_fullSourceSection = new LogFileSection(0, (int)startIndex);

					if (_currentSourceIndex > _fullSourceSection.LastIndex)
						_currentSourceIndex = (int)section.Index;

					Invalidate(_currentSourceIndex);
					RemoveInvalidatedLines(_lastLogEntry, _currentSourceIndex);
				}
				else
				{
					_fullSourceSection = LogFileSection.MinimumBoundingLine(_fullSourceSection, section);
				}
			}

			if (!_fullSourceSection.IsEndOfSection(_currentSourceIndex))
			{
				int remaining = _fullSourceSection.Index + _fullSourceSection.Count - _currentSourceIndex;
				int nextCount = Math.Min(remaining, BatchSize);
				var nextSection = new LogFileSection(_currentSourceIndex, nextCount);
				_source.GetSection(nextSection, _buffer);

				for (int i = 0; i < nextCount; ++i)
				{
					if (token.IsCancellationRequested)
						break;

					LogLine line = _buffer[i];
					if (_lastLogEntry.Count == 0 || _lastLogEntry[0].LogEntryIndex == line.LogEntryIndex)
					{
						_lastLogEntry.Add(line);
					}
					else if (line.LogEntryIndex != _lastLogEntry[0].LogEntryIndex)
					{
						TryAddLogLine(_lastLogEntry);
						_lastLogEntry.Clear();
						_lastLogEntry.Add(line);
					}
				}

				_currentSourceIndex += nextCount;
			}

			if (_fullSourceSection.IsEndOfSection(_currentSourceIndex))
			{
				TryAddLogLine(_lastLogEntry);
				Listeners.OnRead(_indices.Count);

				SetEndOfSourceReached();
			}

			return _maximumWaitTime;
		}

		private static void RemoveInvalidatedLines(List<LogLine> lastLogEntry, int currentSourceIndex)
		{
			while (lastLogEntry.Count > 0)
			{
				int i = lastLogEntry.Count - 1;
				LogLine line = lastLogEntry[i];
				if (line.LineIndex >= currentSourceIndex)
				{
					lastLogEntry.RemoveAt(i);
				}
				else
				{
					break;
				}
			}
		}

		private void Invalidate(int currentSourceIndex)
		{
			int numRemoved = 0;
			lock (_indices)
			{
				while (_indices.Count > 0)
				{
					int i = _indices.Count - 1;
					int sourceIndex = _indices[i];
					if (sourceIndex >= currentSourceIndex)
					{
						_indices.RemoveAt(i);
						++numRemoved;
					}
					else
					{
						break;
					}
				}
			}
			Listeners.Invalidate(_indices.Count, numRemoved);
		}

		private void Clear()
		{
			_fullSourceSection = new LogFileSection();
			lock (_indices)
			{
				_indices.Clear();
			}
			Listeners.OnRead(-1);
		}

		private bool TryAddLogLine(List<LogLine> logEntry)
		{
			if (_indices.Count > 0 && logEntry.Count > 0 &&
			    _indices[_indices.Count - 1] == logEntry[logEntry.Count - 1].LineIndex)
				return true;

			if (_filter.PassesFilter(logEntry))
			{
				lock (_indices)
				{
					foreach (LogLine line in logEntry)
					{
						_indices.Add(line.LineIndex);
						_maxCharactersPerLine = Math.Max(_maxCharactersPerLine, line.Message.Length);
					}
				}
				Listeners.OnRead(_indices.Count);
				return true;
			}

			if (logEntry.Count > 0)
			{
				int n = 0;
			}

			return false;
		}
	}
}