using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
		private const int BatchSize = 10000;

		private readonly ILogLineFilter _logLineFilter;
		private readonly ILogEntryFilter _logEntryFilter;
		private readonly List<int> _indices;
		private readonly Dictionary<int, int> _logEntryIndices;
		private readonly ConcurrentQueue<LogFileSection> _pendingModifications;
		private readonly ILogFile _source;
		private readonly LogLine[] _buffer;
		private readonly TimeSpan _maximumWaitTime;

		private LogFileSection _fullSourceSection;
		private int _maxCharactersPerLine;
		private int _currentSourceIndex;
		private readonly List<LogLine> _lastLogEntry;
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
			_buffer = new LogLine[BatchSize];
			_lastLogEntry = new List<LogLine>();
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
		public override IReadOnlyList<ILogFileColumn> Columns => LogFileColumns.Minimum;

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
			_pendingModifications.Enqueue(section);
			ResetEndOfSourceReached();
		}

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

			if (Equals(column, LogFileColumns.DeltaTime))
			{
				GetDeltaTime(section, (TimeSpan?[])(object)buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LineNumber))
			{
				GetLineNumber(section, (int[])(object)buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.OriginalIndex))
			{
				GetOriginalIndices(section, (LogLineIndex[]) (object) buffer, destinationIndex);
			}
			else
			{
				var actualIndices = GetOriginalIndices(section);
				_source.GetColumn(actualIndices, column, buffer, destinationIndex);
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

			if (Equals(column, LogFileColumns.DeltaTime))
			{
				GetDeltaTime(indices, (TimeSpan?[])(object)buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LineNumber))
			{
				GetLineNumber(indices, (int[])(object)buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.OriginalIndex))
			{
				GetOriginalIndices(indices, (LogLineIndex[]) (object) buffer, destinationIndex);
			}
			else
			{
				var actualIndices = GetOriginalIndices(indices);
				_source.GetColumn(actualIndices, column, buffer, destinationIndex);
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
		public override void GetEntries(LogFileSection section, ILogEntries buffer, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override void GetEntries(IReadOnlyList<LogLineIndex> indices, ILogEntries buffer, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		private void GetLineNumber(IReadOnlyList<LogLineIndex> indices, int[] buffer, int destinationIndex)
		{
			lock (_indices)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					if (index >= 0 && index < _indices.Count)
					{
						var lineNumber = (int) (index + 1);
						buffer[destinationIndex + i] = lineNumber;
					}
					else
					{
						buffer[destinationIndex + i] = 0;
					}
				}
			}
		}

		private void GetDeltaTime(LogFileSection section, TimeSpan?[] buffer, int destinationIndex)
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
				buffer[destinationIndex + i - 1] = delta;
			}
		}

		private void GetDeltaTime(IReadOnlyList<LogLineIndex> indices, TimeSpan?[] buffer, int destinationIndex)
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
				buffer[destinationIndex + i] = currentTimestamp - previousTimestamp;
			}
		}

		private LogLineIndex ToSourceIndex(LogLineIndex index)
		{
			if (index >= 0 && index < _indices.Count)
				return _indices[(int) index];

			return LogLineIndex.Invalid;
		}

		/// <inheritdoc />
		public override void GetSection(LogFileSection section, LogLine[] dest)
		{
			if (section.Index < 0)
				throw new ArgumentOutOfRangeException(nameof(section), string.Format("Index '{0}' is expected to be greater or equal to 0", section.Index));
			if (section.Count < 0)
				throw new ArgumentOutOfRangeException(nameof(section), string.Format("Count '{0}' is expected to be greater or equal to 0", section.Count));
			if (dest == null)
				throw new ArgumentNullException(nameof(dest));
			if (dest.Length < section.Count)
				throw new ArgumentOutOfRangeException(nameof(section), string.Format("The provided buffer (length '{0}') should hold at least as many items as requested '{1}", dest.Length, section.Count));

			lock (_indices)
			{
				var count = section.Index + section.Count;
				if (count > _indices.Count)
					throw new ArgumentOutOfRangeException(nameof(section), string.Format("Cannot request more than '{0}' items: '{1}'", _indices.Count, count));

				for (int i = 0; i < section.Count; ++i)
				{
					var index = section.Index + i;
					dest[i] = GetLineNoLock((int) index);
				}
			}
		}

		/// <inheritdoc />
		public override LogLine GetLine(int index)
		{
			lock (_indices)
			{
				return GetLineNoLock(index);
			}
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

		private LogLine GetLineNoLock(int lineIndex)
		{
			int sourceLineIndex = _indices[lineIndex];
			int logEntryIndex;
			_logEntryIndices.TryGetValue(sourceLineIndex, out logEntryIndex);
			var line = _source.GetLine(sourceLineIndex);
			return new LogLine(lineIndex, sourceLineIndex, logEntryIndex, line.Message, line.Level, line.Timestamp);
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
				_source.GetSection(nextSection, _buffer);

				for (int i = 0; i < nextCount; ++i)
				{
					if (token.IsCancellationRequested)
						break;

					LogLine line = _buffer[i];
					if (_lastLogEntry.Count == 0 || _lastLogEntry[0].LogEntryIndex == line.LogEntryIndex)
					{
						TryAddLogLine(line);
					}
					else if (line.LogEntryIndex != _lastLogEntry[0].LogEntryIndex)
					{
						TryAddLogEntry(_lastLogEntry);
						_lastLogEntry.Clear();
						TryAddLogLine(line);
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

		private void TryAddLogLine(LogLine line)
		{
			// We have a filter that operates on individual lines (regardless of log entry affiliation).
			// We therefore have to evaluate each line for itself before we can even begin to consider adding a log
			// entry.
			if (_logLineFilter.PassesFilter(line))
			{
				_lastLogEntry.Add(line);
			}
		}

		private bool TryAddLogEntry(List<LogLine> logEntry)
		{
			if (_indices.Count > 0 && logEntry.Count > 0 &&
			    _indices[_indices.Count - 1] == logEntry[logEntry.Count - 1].LineIndex)
				return true;

			if (_logEntryFilter.PassesFilter(logEntry))
			{
				lock (_indices)
				{
					if (logEntry.Count > 0)
					{
						foreach (LogLine line in logEntry)
						{
							_indices.Add(line.LineIndex);
							_logEntryIndices[line.LineIndex] = _currentLogEntryIndex;
							_maxCharactersPerLine = Math.Max(_maxCharactersPerLine, line.Message?.Length ?? 0);
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