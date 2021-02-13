using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Filters;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     A <see cref="ILogFile" /> implementation which offers a filtered view onto a log file.
	/// </summary>
	/// <remarks>
	///    Plugin authors are deliberately prevented from instantiating this type directly because it's constructor signature may change
	///    over time. In order to create an instance of this type, simply call <see cref="IServiceContainer.CreateFilteredLogFile"/>
	///    who's signature is guaranteed to never change.
	/// </remarks>
	[DebuggerTypeProxy(typeof(LogFileView))]
	internal sealed class FilteredLogFile
		: AbstractLogFile
		, ILogFileListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private const int BatchSize = 10000;

		private readonly ILogLineFilter _logLineFilter;
		private readonly ILogEntryFilter _logEntryFilter;
		private readonly List<int> _indices;
		private readonly Dictionary<int, int> _logEntryIndices;
		private readonly ConcurrentQueue<LogFileSection> _pendingModifications;
		private readonly ILogFile _source;
		private readonly LogEntryArray _array;
		private readonly TimeSpan _maximumWaitTime;

		private LogFileSection _fullSourceSection;
		private int _maxCharactersPerLine;
		private int _currentSourceIndex;
		private readonly LogEntryList _lastLogEntry;
		private int _currentLogEntryIndex;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="scheduler"></param>
		/// <param name="maximumWaitTime"></param>
		/// <param name="source"></param>
		/// <param name="logLineFilter"></param>
		/// <param name="logEntryFilter"></param>
		public FilteredLogFile(ITaskScheduler scheduler,
			TimeSpan maximumWaitTime,
			ILogFile source,
			ILogLineFilter logLineFilter,
			ILogEntryFilter logEntryFilter)
			: base(scheduler)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			_source = source;
			_logLineFilter = logLineFilter ?? new NoFilter();
			_logEntryFilter = logEntryFilter ?? new NoFilter();
			_pendingModifications = new ConcurrentQueue<LogFileSection>();
			_indices = new List<int>();
			_logEntryIndices = new Dictionary<int, int>();
			_array = new LogEntryArray(BatchSize, LogFileColumns.Minimum);
			_lastLogEntry = new LogEntryList(LogFileColumns.Minimum);
			_maximumWaitTime = maximumWaitTime;

			_source.AddListener(this, maximumWaitTime, BatchSize);
			StartTask();
		}

		/// <inheritdoc />
		protected override void DisposeAdditional()
		{
			_source.RemoveListener(this);
		}

		/// <inheritdoc />
		public override int OriginalCount => _source.Count;

		/// <inheritdoc />
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

		/// <inheritdoc />
		public override int MaxCharactersPerLine => _maxCharactersPerLine;

		/// <inheritdoc />
		public override IReadOnlyList<ILogFileColumnDescriptor> Columns => LogFileColumns.CombineWithMinimum(_source.Columns);

		/// <inheritdoc />
		public override IReadOnlyList<ILogFilePropertyDescriptor> Properties => _source.Properties;

		/// <inheritdoc />
		public override object GetValue(ILogFilePropertyDescriptor propertyDescriptor)
		{
			return _source.GetValue(propertyDescriptor);
		}

		/// <inheritdoc />
		public override T GetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor)
		{
			return _source.GetValue(propertyDescriptor);
		}

		/// <inheritdoc />
		public override void GetValues(ILogFileProperties properties)
		{
			_source.GetValues(properties);
		}

		/// <inheritdoc />
		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			Log.DebugFormat("OnLogFileModified({0})", section);

			_pendingModifications.Enqueue(section);
			ResetEndOfSourceReached();
		}

		/// <inheritdoc />
		public override void GetColumn<T>(LogFileSection sourceSection, ILogFileColumnDescriptor<T> column, T[] destination, int destinationIndex)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			if (destinationIndex + sourceSection.Count > destination.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

			if (Equals(column, LogFileColumns.Index))
			{
				GetIndex(sourceSection, (LogLineIndex[])(object)destination, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LogEntryIndex))
			{
				GetLogEntryIndex(sourceSection, (LogEntryIndex[])(object)destination, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.DeltaTime))
			{
				GetDeltaTime(sourceSection, (TimeSpan?[])(object)destination, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LineNumber))
			{
				GetLineNumber(sourceSection, (int[])(object)destination, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.OriginalIndex))
			{
				GetOriginalIndices(sourceSection, (LogLineIndex[]) (object) destination, destinationIndex);
			}
			else
			{
				var actualIndices = GetOriginalIndices(sourceSection);
				_source.GetColumn(actualIndices, column, destination, destinationIndex);
			}
		}

		/// <inheritdoc />
		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, ILogFileColumnDescriptor<T> column, T[] destination, int destinationIndex)
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

			if (Equals(column, LogFileColumns.Index))
			{
				GetIndex(sourceIndices, (LogLineIndex[])(object)destination, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LogEntryIndex))
			{
				GetLogEntryIndex(sourceIndices, (LogEntryIndex[])(object)destination, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.DeltaTime))
			{
				GetDeltaTime(sourceIndices, (TimeSpan?[])(object)destination, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LineNumber))
			{
				GetLineNumber(sourceIndices, (int[])(object)destination, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.OriginalIndex))
			{
				GetOriginalIndices(sourceIndices, (LogLineIndex[]) (object) destination, destinationIndex);
			}
			else
			{
				var actualIndices = GetOriginalIndices(sourceIndices);
				_source.GetColumn(actualIndices, column, destination, destinationIndex);
			}
		}

		private LogLineIndex[] GetOriginalIndices(IReadOnlyList<LogLineIndex> indices)
		{
			var actualIndices = new LogLineIndex[indices.Count];
			GetOriginalIndices(indices, actualIndices, 0);
			return actualIndices;
		}

		private void GetOriginalIndices(IReadOnlyList<LogLineIndex> indices, LogLineIndex[] destination, int destinationIndex)
		{
			lock (_indices)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					destination[destinationIndex + i] = ToSourceIndex(indices[i].Value);
				}
			}
		}

		/// <inheritdoc />
		public override void GetEntries(LogFileSection sourceSection, ILogEntries destination, int destinationIndex)
		{
			// TODO: This can probably be optimized (why are we translating indices each time for every column?!
			foreach (var column in destination.Columns)
			{
				destination.CopyFrom(column, destinationIndex, this, sourceSection);
			}
		}

		/// <inheritdoc />
		public override void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogEntries destination, int destinationIndex)
		{
			// TODO: This can probably be optimized (why are we translating indices each time for every column?!
			foreach (var column in destination.Columns)
			{
				destination.CopyFrom(column, destinationIndex, this, sourceIndices);
			}
		}

		private void GetIndex(LogFileSection sourceSection, LogLineIndex[] destination, int destinationIndex)
		{
			lock (_indices)
			{
				for (int i = 0; i < sourceSection.Count; ++i)
				{
					var sourceIndex = sourceSection.Index.Value + i;
					if (sourceIndex >= 0 && sourceIndex < _indices.Count)
					{
						destination[destinationIndex + i] = sourceIndex;
					}
					else
					{
						destination[destinationIndex + i] = LogFileColumns.Index.DefaultValue;
					}
				}
			}
		}

		private void GetIndex(IReadOnlyList<LogLineIndex> sourceIndices, LogLineIndex[] destination, int destinationIndex)
		{
			lock (_indices)
			{
				for (int i = 0; i < sourceIndices.Count; ++i)
				{
					var sourceIndex = sourceIndices[i].Value;
					if (sourceIndex >= 0 && sourceIndex < _indices.Count)
					{
						destination[destinationIndex + i] = sourceIndex;
					}
					else
					{
						destination[destinationIndex + i] = LogFileColumns.Index.DefaultValue;
					}
				}
			}
		}

		private void GetLogEntryIndex(LogFileSection sourceSection, LogEntryIndex[] destination, int destinationIndex)
		{
			lock (_indices)
			{
				for (int i = 0; i < sourceSection.Count; ++i)
				{
					var sourceIndex = sourceSection.Index.Value + i;
					if (sourceIndex >= 0 && sourceIndex < _indices.Count)
					{
						var originalIndex = _indices[sourceIndex];
						var logEntryIndex = _logEntryIndices[originalIndex];
						destination[destinationIndex + i] = logEntryIndex;
					}
					else
					{
						destination[destinationIndex + i] = LogFileColumns.LogEntryIndex.DefaultValue;
					}
				}
			}
		}

		private void GetLogEntryIndex(IReadOnlyList<LogLineIndex> sourceIndices, LogEntryIndex[] destination, int destinationIndex)
		{
			lock (_indices)
			{
				for (int i = 0; i < sourceIndices.Count; ++i)
				{
					var sourceIndex = sourceIndices[i].Value;
					if (sourceIndex >= 0 && sourceIndex < _indices.Count)
					{
						var originalIndex = _indices[sourceIndex];
						var logEntryIndex = _logEntryIndices[originalIndex];
						destination[destinationIndex + i] = logEntryIndex;
					}
					else
					{
						destination[destinationIndex + i] = LogFileColumns.LogEntryIndex.DefaultValue;
					}
				}
			}
		}

		private void GetLineNumber(IReadOnlyList<LogLineIndex> indices, int[] destination, int destinationIndex)
		{
			lock (_indices)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					if (index >= 0 && index < _indices.Count)
					{
						var lineNumber = (int) (index + 1);
						destination[destinationIndex + i] = lineNumber;
					}
					else
					{
						destination[destinationIndex + i] = LogFileColumns.LineNumber.DefaultValue;
					}
				}
			}
		}

		private void GetDeltaTime(LogFileSection section, TimeSpan?[] destination, int destinationIndex)
		{
			var actualIndices = new LogLineIndex[section.Count + 1];

			lock (_indices)
			{
				var startIndex = section.Index;
				if (startIndex > 0)
					actualIndices[0] = _indices[startIndex - 1];
				else
					actualIndices[0] = -1;

				for (int i = 0; i < section.Count; ++i)
				{
					var filteredIndex = (startIndex + i).Value;
					if (filteredIndex >= 0 && filteredIndex < _indices.Count)
						actualIndices[i + 1] = _indices[filteredIndex];
					else
						actualIndices[i + 1] = -1;
				}
			}

			var timestamps = _source.GetColumn(actualIndices, LogFileColumns.Timestamp);
			for (int i = 1; i <= section.Count; ++i)
			{
				var previousTimestamp = timestamps[i - 1];
				var currentTimestamp = timestamps[i];
				var delta = currentTimestamp - previousTimestamp;
				destination[destinationIndex + i - 1] = delta;
			}
		}

		private void GetDeltaTime(IReadOnlyList<LogLineIndex> indices, TimeSpan?[] destination, int destinationIndex)
		{
			// The easiest way to serve random access to this column is to simply retrieve
			// the timestamp for every requested index as well as for the preceeding index.
			var actualIndices = new LogLineIndex[indices.Count * 2];
			lock (_indices)
			{
				for(int i = 0; i < indices.Count; ++i)
				{
					var index = indices[0];
					actualIndices[i * 2 + 0] = ToSourceIndex(index - 1);
					actualIndices[i * 2 + 1] = ToSourceIndex(index);
				}
			}

			var timestamps = _source.GetColumn(actualIndices, LogFileColumns.Timestamp);
			for (int i = 0; i < indices.Count; ++i)
			{
				var previousTimestamp = timestamps[i * 2 + 0];
				var currentTimestamp = timestamps[i * 2 + 1];
				destination[destinationIndex + i] = currentTimestamp - previousTimestamp;
			}
		}

		private LogLineIndex ToSourceIndex(LogLineIndex index)
		{
			if (index >= 0 && index < _indices.Count)
				return _indices[(int) index];

			return LogLineIndex.Invalid;
		}

		/// <inheritdoc />
		public override double Progress
		{
			get
			{
				var sourceCount = _source.Count;
				var currentIndex = _currentSourceIndex;

				var relativeProgress = (double) currentIndex / sourceCount;
				var progress = MathEx.Saturate(relativeProgress) * _source.Progress;
				return progress;
			}
		}

		/// <inheritdoc />
		public override LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalSourceIndex)
		{
			lock (_indices)
			{
				for (int i = 0; i < _indices.Count; ++i)
				{
					if (_indices[i] == originalSourceIndex.Value)
					{
						return i;
					}
				}
			}

			return LogLineIndex.Invalid;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("{0} (Filtered)", _source);
		}

		/// <inheritdoc />
		protected override TimeSpan RunOnce(CancellationToken token)
		{
			bool performedWork = false;

			LogFileSection section;
			while (_pendingModifications.TryDequeue(out section) && !token.IsCancellationRequested)
			{
				if (section.IsReset)
				{
					Clear();
					_lastLogEntry.Clear();
					_currentSourceIndex = 0;
				}
				else if (section.IsInvalidate)
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

				performedWork = true;
			}

			if (!_fullSourceSection.IsEndOfSection(_currentSourceIndex))
			{
				int remaining = _fullSourceSection.Index + _fullSourceSection.Count - _currentSourceIndex;
				int nextCount = Math.Min(remaining, BatchSize);
				var nextSection = new LogFileSection(_currentSourceIndex, nextCount);
				_source.GetEntries(nextSection, _array);

				for (int i = 0; i < nextCount; ++i)
				{
					if (token.IsCancellationRequested)
						break;

					var logEntry = _array[i];
					if (Log.IsDebugEnabled)
						Log.DebugFormat("Processing: LineIndex={0}, OriginalLineIndex={1}, LogEntryIndex={2}, Message={3}",
						                logEntry.Index,
						                logEntry.OriginalIndex,
						                logEntry.LogEntryIndex,
						                logEntry.RawContent);

					if (_lastLogEntry.Count == 0 || _lastLogEntry[0].LogEntryIndex == logEntry.LogEntryIndex)
					{
						TryAddLogLine(logEntry);
					}
					else if (logEntry.LogEntryIndex != _lastLogEntry[0].LogEntryIndex)
					{
						TryAddLogEntry(_lastLogEntry);
						_lastLogEntry.Clear();
						TryAddLogLine(logEntry);
					}
				}

				_currentSourceIndex += nextCount;
			}

			if (_fullSourceSection.IsEndOfSection(_currentSourceIndex))
			{
				TryAddLogEntry(_lastLogEntry);
				Listeners.OnRead(_indices.Count);

				if (_source.EndOfSourceReached)
					SetEndOfSourceReached();
			}

			if (performedWork)
				return TimeSpan.Zero;

			return _maximumWaitTime;
		}

		private static void RemoveInvalidatedLines(LogEntryList lastLogEntry, int currentSourceIndex)
		{
			while (lastLogEntry.Count > 0)
			{
				int i = lastLogEntry.Count - 1;
				var logEntry = lastLogEntry[i];
				if (logEntry.Index >= currentSourceIndex)
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
						int previousLogEntryIndex;
						if (_logEntryIndices.TryGetValue(sourceIndex, out previousLogEntryIndex))
						{
							_currentLogEntryIndex = previousLogEntryIndex;
						}
						_logEntryIndices.Remove(sourceIndex);

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
				_logEntryIndices.Clear();
				_currentLogEntryIndex = 0;
			}
			Listeners.OnRead(-1);
		}

		private void TryAddLogLine(IReadOnlyLogEntry logEntry)
		{
			// We have a filter that operates on individual lines (regardless of log entry affiliation).
			// We therefore have to evaluate each line for itself before we can even begin to consider adding a log
			// entry.
			if (_logLineFilter.PassesFilter(logEntry))
			{
				_lastLogEntry.Add(logEntry);
			}
		}

		private bool TryAddLogEntry(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			if (_indices.Count > 0 && logEntry.Count > 0 &&
			    _indices[_indices.Count - 1] == logEntry[logEntry.Count - 1].Index)
				return true;

			if (_logEntryFilter.PassesFilter(logEntry))
			{
				lock (_indices)
				{
					if (logEntry.Count > 0)
					{
						foreach (var line in logEntry)
						{
							_indices.Add((int) line.Index);
							_logEntryIndices[(int) line.Index] = _currentLogEntryIndex;
							_maxCharactersPerLine = Math.Max(_maxCharactersPerLine, line.RawContent?.Length ?? 0);
						}
						++_currentLogEntryIndex;
					}
				}
				Listeners.OnRead(_indices.Count);
				return true;
			}
			
			return false;
		}
	}
}