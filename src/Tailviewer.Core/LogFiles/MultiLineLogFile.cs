using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Responsible for merging consecutive lines into multi-line log entries,
	///     if they belong together.
	/// </summary>
	/// <remarks>
	///     Two lines are defined to belong together if the first line contains a log
	///     level and the next one does not.
	/// </remarks>
	/// <remarks>
	///    Plugin authors are deliberately prevented from instantiating this type directly because it's constructor signature may change
	///    over time. In order to create an instance of this type, simply call <see cref="IServiceContainer.CreateMultiLineLogFile"/>
	///    who's signature is guaranteed to never change.
	/// </remarks>
	[DebuggerTypeProxy(typeof(LogFileView))]
	internal sealed class MultiLineLogFile
		: AbstractLogFile
		, ILogFileListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private const int MaximumBatchSize = 10000;

		private readonly object _syncRoot;
		private readonly List<LogEntryInfo> _indices;
		private readonly TimeSpan _maximumWaitTime;
		private readonly ConcurrentQueue<LogFileSection> _pendingModifications;
		private readonly ILogFile _source;
		private readonly ILogFileProperties _properties;
		private LogEntryInfo _currentLogEntry;
		private LogLineIndex _currentSourceIndex;

		private LogFileSection _fullSourceSection;
		private int _maxCharactersPerLine;
		private LevelFlags _currentLogEntryLevel;
		private DateTime? _currentLogEntryTimestamp;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <remarks>
		///    Plugin authors are deliberately prevented from calling this constructor directly because it's signature may change
		///    over time. In order to create an instance of this type, simply call <see cref="IServiceContainer.CreateMultiLineLogFile"/>.
		/// </remarks>
		/// <param name="taskScheduler"></param>
		/// <param name="source"></param>
		/// <param name="maximumWaitTime"></param>
		public MultiLineLogFile(ITaskScheduler taskScheduler, ILogFile source, TimeSpan maximumWaitTime)
			: base(taskScheduler)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			_maximumWaitTime = maximumWaitTime;
			_pendingModifications = new ConcurrentQueue<LogFileSection>();
			_syncRoot = new object();
			_indices = new List<LogEntryInfo>();

			// The log file we were given might offer even more properties than the minimum set and we
			// want to expose those as well.
			_properties = new LogFilePropertyList(LogFileProperties.CombineWithMinimum(source.Properties));
			_properties.SetValue(LogFileProperties.EmptyReason, ErrorFlags.SourceDoesNotExist);

			_currentLogEntry = new LogEntryInfo(-1, 0);

			_source = source;
			_source.AddListener(this, maximumWaitTime, MaximumBatchSize);
			StartTask();
		}

		/// <inheritdoc />
		public override int MaxCharactersPerLine => _maxCharactersPerLine;

		/// <inheritdoc />
		public override IReadOnlyList<ILogFileColumn> Columns => _source.Columns;

		/// <inheritdoc />
		public override IReadOnlyList<ILogFilePropertyDescriptor> Properties => _properties.Properties;

		/// <inheritdoc />
		public override object GetValue(ILogFilePropertyDescriptor propertyDescriptor)
		{
			object value;
			_properties.TryGetValue(propertyDescriptor, out value);
			return value;
		}

		/// <inheritdoc />
		public override T GetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor)
		{
			T value;
			_properties.TryGetValue(propertyDescriptor, out value);
			return value;
		}

		/// <inheritdoc />
		public override void GetValues(ILogFileProperties properties)
		{
			_properties.GetValues(properties);
		}

		/// <inheritdoc />
		public override int Count => (int) _currentSourceIndex;

		/// <inheritdoc />
		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			_pendingModifications.EnqueueMany(section.Split(MaximumBatchSize));
			ResetEndOfSourceReached();
		}

		/// <inheritdoc />
		protected override void DisposeAdditional()
		{
			_source.RemoveListener(this);
		}

		/// <inheritdoc />
		public override int OriginalCount => _source.OriginalCount;

		/// <inheritdoc />
		public override void GetColumn<T>(LogFileSection section, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			if (destinationIndex + section.Count > buffer.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

			if (!TryGetSpecialColumn(section, column, buffer, destinationIndex))
			{
				_source.GetColumn(section, column, buffer, destinationIndex);
			}
		}

		/// <inheritdoc />
		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> indices, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			if (indices == null)
				throw new ArgumentNullException(nameof(indices));
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			if (destinationIndex + indices.Count > buffer.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

			if (!TryGetSpecialColumn(indices, column, buffer, destinationIndex))
			{
				_source.GetColumn(indices, column, buffer, destinationIndex);
			}
		}

		/// <inheritdoc />
		public override void GetEntries(LogFileSection section, ILogEntries buffer, int destinationIndex)
		{
			_source.GetEntries(section, buffer, destinationIndex);
		}

		/// <inheritdoc />
		public override void GetEntries(IReadOnlyList<LogLineIndex> indices, ILogEntries buffer, int destinationIndex)
		{
			_source.GetEntries(indices, buffer, destinationIndex);
		}

		/// <inheritdoc />
		public override void GetSection(LogFileSection section, LogLine[] dest)
		{
			_source.GetSection(section, dest);
			lock (_syncRoot)
			{
				for (var i = 0; i < section.Count; ++i)
					dest[i] = PatchNoLock(dest[i]);
			}
		}

		/// <inheritdoc />
		public override LogLine GetLine(int index)
		{
			var actualLine = _source.GetLine(index);
			LogLine line;

			lock (_syncRoot)
			{
				line = PatchNoLock(actualLine);
			}

			return line;
		}

		/// <inheritdoc />
		public override double Progress => 1;

		/// <inheritdoc />
		public override string ToString()
		{
			return $"MultiLineLogFile({_source})";
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

		/// <inheritdoc />
		protected override TimeSpan RunOnce(CancellationToken token)
		{
			bool performedWork = false;
			while (_pendingModifications.TryDequeue(out var nextSection) && !token.IsCancellationRequested)
			{
				if (nextSection.IsReset)
				{
					Clear();
				}
				else if (nextSection.IsInvalidate)
				{
					Invalidate(nextSection);
				}
				else
				{
					Append(nextSection);
				}

				performedWork = true;
			}

			// Now we can perform a block-copy of all properties.
			_source.GetValues(_properties);

			_maxCharactersPerLine = _source.MaxCharactersPerLine;

			if (_indices.Count != _currentSourceIndex)
			{
				Log.ErrorFormat("Inconsistency detected: We have {0} indices for {1} lines", _indices.Count,
				                _currentSourceIndex);
			}

			Listeners.OnRead((int)_currentSourceIndex);

			if (_source.EndOfSourceReached && _fullSourceSection.IsEndOfSection(_currentSourceIndex))
			{
				SetEndOfSourceReached();
			}

			if (performedWork)
				return TimeSpan.Zero;

			return _maximumWaitTime;
		}

		private void Append(LogFileSection section)
		{
			var buffer = new LogLine[section.Count];
			_source.GetSection(section, buffer);

			lock (_syncRoot)
			{
				for (var i = 0; i < section.Count; ++i)
				{
					var line = buffer[i];

					if (_currentLogEntry.EntryIndex.IsInvalid ||
					    !AppendToCurrentLogEntry(line))
					{
						_currentLogEntry = _currentLogEntry.NextEntry(line.LineIndex);
						_currentLogEntryLevel = line.Level;
						_currentLogEntryTimestamp = line.Timestamp;
					}

					_indices.Add(_currentLogEntry);
				}
			}

			_currentSourceIndex += section.Count;
			_fullSourceSection = new LogFileSection(0, _currentSourceIndex.Value);
		}

		private void Invalidate(LogFileSection sectionToInvalidate)
		{
			var firstInvalidIndex = LogLineIndex.Min(_fullSourceSection.LastIndex, sectionToInvalidate.Index);
			var lastInvalidIndex = LogLineIndex.Min(_fullSourceSection.LastIndex, sectionToInvalidate.LastIndex);
			var invalidateCount = lastInvalidIndex - firstInvalidIndex + 1;
			var previousSourceIndex = _currentSourceIndex;

			_fullSourceSection = new LogFileSection(0, (int)firstInvalidIndex);
			if (_fullSourceSection.Count > 0)
			{
				// It's possible (likely) that we've received an invalidation for a region of the source
				// that we've already processed (i.e. created indices for). If that's the case, then we need
				// to rewind the index. Otherwise nothing needs to be done...
				var newIndex = _fullSourceSection.LastIndex + 1;
				if (newIndex < _currentSourceIndex)
				{
					_currentSourceIndex = newIndex;
				}
			}
			else
			{
				_currentSourceIndex = 0;
			}

			lock (_syncRoot)
			{
				var toRemove = _indices.Count - lastInvalidIndex;
				if (toRemove > 0)
				{
					_indices.RemoveRange((int)firstInvalidIndex, toRemove);
					_currentLogEntry = new LogEntryInfo(firstInvalidIndex - 1, 0);
				}
				if (previousSourceIndex != _currentSourceIndex)
				{
					_indices.RemoveRange((int) _currentSourceIndex, _indices.Count - _currentSourceIndex);
				}
			}

			if (_indices.Count != _currentSourceIndex)
			{
				Log.ErrorFormat("Inconsistency detected: We have {0} indices for {1} lines", _indices.Count,
					_currentSourceIndex);
			}

			Listeners.Invalidate((int)firstInvalidIndex, invalidateCount);

			if (_fullSourceSection.Count > firstInvalidIndex)
			{
				_fullSourceSection = new LogFileSection(0, firstInvalidIndex.Value);
			}
		}

		private void Clear()
		{
			_fullSourceSection = new LogFileSection(0, 0);
			_currentSourceIndex = 0;
			_currentLogEntry = new LogEntryInfo(-1, 0);
			lock (_syncRoot)
			{
				_indices.Clear();
			}
			Listeners.OnRead(-1);
		}

		private bool TryGetSpecialColumn<T>(IReadOnlyList<LogLineIndex> indices, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			if (Equals(column, LogFileColumns.Timestamp))
			{
				var firstLineIndices = GetFirstLineIndices(indices);
				_source.GetColumn(firstLineIndices, column, buffer, destinationIndex);
				return true;
			}

			if (Equals(column, LogFileColumns.LogEntryIndex))
			{
				GetLogEntryIndex(indices, (LogEntryIndex[])(object)buffer, destinationIndex);
				return true;
			}

			return false;
		}

		private bool AppendToCurrentLogEntry(LogLine logLine)
		{
			if (_currentLogEntryTimestamp != null && logLine.Timestamp == null)
				return true;
			if (_currentLogEntryLevel != LevelFlags.None && logLine.Level == LevelFlags.None)
				return true;

			return false;
		}

		private void GetLogEntryIndex(IReadOnlyList<LogLineIndex> indices, LogEntryIndex[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for(int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					var entryInfo = TryGetLogEntryInfo(index);
					buffer[destinationIndex + i] = entryInfo?.EntryIndex ?? LogEntryIndex.Invalid;
				}
			}
		}

		private IReadOnlyList<LogLineIndex> GetFirstLineIndices(IReadOnlyList<LogLineIndex> indices)
		{
			lock (_syncRoot)
			{
				var firstLineIndices = new List<LogLineIndex>(indices.Count);
				foreach (var index in indices)
				{
					var entryInfo = TryGetLogEntryInfo(index);
					if (entryInfo != null)
						firstLineIndices.Add(entryInfo.Value.FirstLineIndex);
					else
						firstLineIndices.Add(LogLineIndex.Invalid);
				}
				return firstLineIndices;
			}
		}

		private LogEntryInfo? TryGetLogEntryInfo(LogLineIndex logLineIndex)
		{
			if (logLineIndex >= 0 && logLineIndex < _indices.Count)
			{
				return _indices[(int) logLineIndex];
			}
			return null;
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