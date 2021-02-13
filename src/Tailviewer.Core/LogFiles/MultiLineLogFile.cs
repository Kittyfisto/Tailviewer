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
		private readonly HashSet<ILogFileColumnDescriptor> _specialColumns;
		private readonly List<LogEntryInfo> _indices;
		private readonly TimeSpan _maximumWaitTime;
		private readonly ConcurrentQueue<LogFileSection> _pendingModifications;
		private readonly ILogFile _source;
		private readonly ILogFileProperties _properties;
		private LogEntryInfo _currentLogEntry;
		private LogLineIndex _currentSourceIndex;

		private LogFileSection _fullSourceSection;
		private int _maxCharactersPerLine;

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
			_specialColumns = new HashSet<ILogFileColumnDescriptor>{LogFileColumns.LogEntryIndex, LogFileColumns.Timestamp, LogFileColumns.LogLevel};
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
		public override IReadOnlyList<ILogFileColumnDescriptor> Columns => _source.Columns;

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
		public override void GetAllValues(ILogFileProperties destination)
		{
			_properties.CopyAllValuesTo(destination);
		}

		/// <inheritdoc />
		public override int Count => (int) _currentSourceIndex;

		/// <inheritdoc />
		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			Log.DebugFormat("OnLogFileModified({0})", section);

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
		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, ILogFileColumnDescriptor<T> column, T[] destination, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			if (sourceIndices == null)
				throw new ArgumentNullException(nameof(sourceIndices));
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			if (destinationIndex + sourceIndices.Count > destination.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

			if (!TryGetSpecialColumn(sourceIndices, column, destination, destinationIndex, queryOptions))
			{
				_source.GetColumn(sourceIndices, column, destination, destinationIndex, queryOptions);
			}
		}

		/// <inheritdoc />
		public override void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogEntries destination, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			var remainingColumns = new List<ILogFileColumnDescriptor>();
			bool partiallyRetrieved = false;
			foreach (var column in destination.Columns)
			{
				if (_specialColumns.Contains(column))
				{
					destination.CopyFrom(column, destinationIndex, this, sourceIndices, queryOptions);
					partiallyRetrieved = true;
				}
				else
				{
					remainingColumns.Add(column);
				}
			}

			if (remainingColumns.Count > 0)
			{
				if (partiallyRetrieved)
				{
					var view = new LogEntriesView(destination, remainingColumns);
					_source.GetEntries(sourceIndices, view, destinationIndex, queryOptions);
				}
				else
				{
					_source.GetEntries(sourceIndices, destination, destinationIndex, queryOptions);
				}
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"MultiLineLogFile({_source})";
		}

		/// <inheritdoc />
		protected override TimeSpan RunOnce(CancellationToken token)
		{
			bool performedWork = false;

			// Every Process() invocation locks the sync root until
			// the changes have been processed. The goal is to minimize
			// total process time and to prevent locking for too long.
			// The following number has been empirically determined
			// via testing and it felt alright :P
			const int maxLineCount = 10000;
			if (_pendingModifications.TryDequeueUpTo(maxLineCount, out var modifications))
			{
				foreach (var nextSection in modifications)
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
			}

			// Now we can perform a block-copy of all properties.
			_source.GetAllValues(_properties);

			_maxCharactersPerLine = _source.MaxCharactersPerLine;

			if (_indices.Count != _currentSourceIndex)
			{
				Log.ErrorFormat("Inconsistency detected: We have {0} indices for {1} lines", _indices.Count,
				                _currentSourceIndex);
			}

			Listeners.OnRead((int)_currentSourceIndex);

			if (_source.EndOfSourceReached && _source.Count == Count)
			{
				SetEndOfSourceReached();
			}

			if (performedWork)
				return TimeSpan.Zero;

			return _maximumWaitTime;
		}

		private void Append(LogFileSection section)
		{
			var buffer = new LogEntryArray(section.Count, LogFileColumns.Index, LogFileColumns.Timestamp, LogFileColumns.LogLevel);
			_source.GetEntries(section, buffer);

			lock (_syncRoot)
			{
				for (var i = 0; i < section.Count; ++i)
				{
					var line = buffer[i];

					if (_currentLogEntry.EntryIndex.IsInvalid ||
					    !AppendToCurrentLogEntry(line))
					{
						_currentLogEntry = _currentLogEntry.NextEntry(line.Index);
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

		private bool TryGetSpecialColumn<T>(IReadOnlyList<LogLineIndex> indices, ILogFileColumnDescriptor<T> column, T[] buffer, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			if (Equals(column, LogFileColumns.Timestamp) ||
			    Equals(column, LogFileColumns.LogLevel))
			{
				var firstLineIndices = GetFirstLineIndices(indices);
				_source.GetColumn(firstLineIndices, column, buffer, destinationIndex, queryOptions);
				return true;
			}

			if (Equals(column, LogFileColumns.LogEntryIndex))
			{
				GetLogEntryIndex(indices, (LogEntryIndex[])(object)buffer, destinationIndex);
				return true;
			}

			return false;
		}

		private bool AppendToCurrentLogEntry(IReadOnlyLogEntry logLine)
		{
			if (logLine.Timestamp != null)
				return false; //< A line with a timestamp is never added to a previous log entry
			if (logLine.LogLevel != LevelFlags.None && logLine.LogLevel != LevelFlags.Other)
				return false; //< A line with a log level is never added to a previous log entry

			return true;
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