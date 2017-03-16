using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Metrolib;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Responsible for merging consecutive lines into multi-line log entries,
	///     if they belong together.
	/// </summary>
	/// <remarks>
	///     Two lines are defined to belong together if the first line contains a log
	///     level and the next one does not.
	/// </remarks>
	public sealed class MultiLineLogFile
		: AbstractLogFile
			, ILogFileListener
	{
		private const int BatchSize = 10000;
		private readonly LogLine[] _buffer;
		private readonly List<LogEntryInfo> _indices;
		private readonly TimeSpan _maximumWaitTime;
		private readonly ConcurrentQueue<LogFileSection> _pendingModifications;
		private readonly ILogFile _source;
		private LogEntryInfo _currentLogEntry;
		private LogLineIndex _currentSourceIndex;
		private bool _exists;
		private Size _fileSize;

		private LogFileSection _fullSourceSection;
		private DateTime _lastModified;
		private int _maxCharactersPerLine;
		private DateTime? _startTimestamp;

		public MultiLineLogFile(ITaskScheduler taskScheduler, ILogFile source, TimeSpan maximumWaitTime)
			: base(taskScheduler)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			_maximumWaitTime = maximumWaitTime;
			_pendingModifications = new ConcurrentQueue<LogFileSection>();
			_indices = new List<LogEntryInfo>();
			_currentLogEntry = new LogEntryInfo(-1, 0);

			_buffer = new LogLine[BatchSize];

			_source = source;
			_source.AddListener(this, maximumWaitTime, BatchSize);
			StartTask();
		}

		public override int MaxCharactersPerLine
		{
			get { return _maxCharactersPerLine; }
		}

		public override bool Exists
		{
			get { return _exists; }
		}

		public override DateTime? StartTimestamp
		{
			get { return _startTimestamp; }
		}

		public override DateTime LastModified
		{
			get { return _lastModified; }
		}

		public override Size FileSize
		{
			get { return _fileSize; }
		}

		public override int Count
		{
			get { return (int) _currentSourceIndex; }
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			_pendingModifications.Enqueue(section);
			ResetEndOfSourceReached();
		}

		protected override void DisposeAdditional()
		{
			_source.RemoveListener(this);
		}

		public override void GetSection(LogFileSection section, LogLine[] dest)
		{
			_source.GetSection(section, dest);
			lock (_indices)
			{
				for (var i = 0; i < section.Count; ++i)
					dest[i] = PatchNoLock(dest[i]);
			}
		}

		public override LogLine GetLine(int index)
		{
			var actualLine = _source.GetLine(index);
			LogLine line;

			lock (_indices)
			{
				line = PatchNoLock(actualLine);
			}

			return line;
		}

		private LogLine PatchNoLock(LogLine line)
		{
			var info = _indices[line.LineIndex];

			LevelFlags level;
			DateTime? timestamp;
			if (line.LineIndex != info.FirstLineIndex)
			{
				// This line belongs to the previous line and together they form
				// (part of) a log entry. Even though only a single line mentions
				// the log level, all lines are given the same log level.
				var firstLine = _source.GetLine((int) info.FirstLineIndex);
				level = firstLine.Level;
				timestamp = firstLine.Timestamp;
			}
			else
			{
				level = line.Level;
				timestamp = line.Timestamp;
			}

			return new LogLine(line.LineIndex, info.EntryIndex,
				line.Message,
				level,
				timestamp);
		}

		protected override TimeSpan RunOnce(CancellationToken token)
		{
			var lastCount = _fullSourceSection.Count;

			LogFileSection section;
			while (_pendingModifications.TryDequeue(out section) && !token.IsCancellationRequested)
			{
				if (section.IsReset)
				{
					Clear();
				}
				else
				{
					_fullSourceSection = LogFileSection.MinimumBoundingLine(_fullSourceSection, section);
				}
			}

			if (!_fullSourceSection.IsEndOfSection(_currentSourceIndex))
			{
				var remaining = Math.Min(_fullSourceSection.Count - _currentSourceIndex, BatchSize);
				_source.GetSection(new LogFileSection(_currentSourceIndex, remaining), _buffer);
				LogLineIndex? resetIndex = null;

				lock (_indices)
				{
					for (var i = 0; i < remaining; ++i)
					{
						var line = _buffer[i];
						if (line.Level != LevelFlags.None)
							_currentLogEntry = _currentLogEntry.NextEntry(line.LineIndex);
						else if (_currentLogEntry.FirstLineIndex < lastCount && resetIndex == null)
							resetIndex = _currentLogEntry.FirstLineIndex;
						_indices.Add(_currentLogEntry);
					}
				}

				if (resetIndex != null)
				{
					var resetCount = lastCount - resetIndex.Value;
					if (resetCount > 0)
						Listeners.Invalidate((int) resetIndex.Value, resetCount);
				}

				_currentSourceIndex += remaining;
			}

			_maxCharactersPerLine = _source.MaxCharactersPerLine;
			_exists = _source.Exists;
			_startTimestamp = _source.StartTimestamp;
			_lastModified = _source.LastModified;
			_fileSize = _source.FileSize;

			Listeners.OnRead((int)_currentSourceIndex);

			if (_source.EndOfSourceReached && _fullSourceSection.IsEndOfSection(_currentSourceIndex))
			{
				SetEndOfSourceReached();
			}

			return _maximumWaitTime;
		}

		private void Clear()
		{
			_fullSourceSection = new LogFileSection(0, 0);
			_currentSourceIndex = 0;
			_currentLogEntry = new LogEntryInfo(-1, 0);
			lock (_indices)
			{
				_indices.Clear();
			}
			Listeners.OnRead(-1);
		}

		private struct LogEntryInfo
		{
			public readonly LogEntryIndex EntryIndex;
			public readonly LogLineIndex FirstLineIndex;

			public LogEntryInfo(LogEntryIndex entryIndex, LogLineIndex firstLineIndex)
			{
				EntryIndex = entryIndex;
				FirstLineIndex = firstLineIndex;
			}

			[Pure]
			public LogEntryInfo NextEntry(LogLineIndex lineLineIndex)
			{
				return new LogEntryInfo(EntryIndex + 1, lineLineIndex);
			}

			public override string ToString()
			{
				return string.Format("Log entry {0} starting at line {1}", EntryIndex, FirstLineIndex);
			}
		}
	}
}