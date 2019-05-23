using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Responsible for merging two or more <see cref="ILogFile" /> instances into one coherent view.
	///     <see cref="LogLine" />s which have no <see cref="LogLine.Timestamp" /> set (or who's parent doesn't)
	///     are discarded from this representation.
	/// </summary>
	[DebuggerTypeProxy(typeof(LogFileView))]
	public sealed class MergedLogFile
		: AbstractLogFile
		, IMergedLogFile
		, ILogFileListener
	{
		private const int BatchSize = 1000;

		private readonly List<Index> _indices;
		private readonly IReadOnlyDictionary<ILogFile, byte> _logFileIndices;
		private readonly TimeSpan _maximumWaitTime;

		private readonly ConcurrentQueue<PendingModification> _pendingModifications;
		private readonly IReadOnlyList<ILogFile> _sources;
		private readonly ILogFileProperties _properties;
		private readonly object _syncRoot;
		private int _maxCharactersPerLine;
		private Percentage _progress;

		private int _totalLineCount;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="scheduler"></param>
		/// <param name="maximumWaitTime"></param>
		/// <param name="sources"></param>
		public MergedLogFile(ITaskScheduler scheduler, TimeSpan maximumWaitTime, IEnumerable<ILogFile> sources)
			: this(scheduler, maximumWaitTime, sources.ToArray())
		{
		}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="scheduler"></param>
		/// <param name="maximumWaitTime"></param>
		/// <param name="sources"></param>
		public MergedLogFile(ITaskScheduler scheduler, TimeSpan maximumWaitTime, params ILogFile[] sources)
			: base(scheduler)
		{
			if (sources == null) throw new ArgumentNullException(nameof(sources));
			if (sources.Any(x => x == null)) throw new ArgumentException("sources.Any(x => x == null)", nameof(sources));
			if (sources.Length > LogLineSourceId.MaxSources) throw new ArgumentException(string.Format("Only up to {0} sources are supported", sources.Length));

			_sources = sources;
			_pendingModifications = new ConcurrentQueue<PendingModification>();
			_indices = new List<Index>();
			var logFileIndices = new Dictionary<ILogFile, byte>();
			_logFileIndices = logFileIndices;
			_syncRoot = new object();
			_maximumWaitTime = maximumWaitTime;
			_properties = new LogFilePropertyList(LogFileProperties.Minimum);

			byte idx = 0;
			foreach (var logFile in _sources)
			{
				logFile.AddListener(this, maximumWaitTime, BatchSize);
				logFileIndices.Add(logFile, idx);

				++idx;
			}
			StartTask();
		}

		/// <inheritdoc />
		public IReadOnlyList<ILogFile> Sources => _sources;

		/// <inheritdoc />
		public override bool EndOfSourceReached
		{
			get { return _sources.All(x => x.EndOfSourceReached) & base.EndOfSourceReached; }
		}

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
		public override int OriginalCount => Count;

		/// <inheritdoc />
		public override int MaxCharactersPerLine => _maxCharactersPerLine;

		/// <inheritdoc />
		public override IReadOnlyList<ILogFileColumn> Columns => LogFileColumns.Minimum;

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
		public override double Progress => _progress.RelativeValue;

		/// <inheritdoc />
		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			_pendingModifications.Enqueue(new PendingModification(logFile, section));
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
				GetDeltaTime(section, (TimeSpan?[]) (object) buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.Index) ||
			         Equals(column, LogFileColumns.OriginalIndex))
			{
				GetIndices(section, (LogLineIndex[]) (object) buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LogEntryIndex))
			{
				GetLogEntryIndices(section, (LogEntryIndex[]) (object) buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LineNumber) ||
			         Equals(column, LogFileColumns.OriginalLineNumber))
			{
				GetLineNumbers(section, (int[]) (object) buffer, destinationIndex);
			}
			else
			{
				// We want to minimize the amount of GetColumn calls to our source files.
				// The best we can achieve is up to one call per source, which is what the following
				// code achieves:
				// At first, we want to build the list of indices we need to retrieve per source
				var sourceIndices = GetOriginalLogLineIndices<T>(section);
				// Then we want to retrieve the column values per source
				GetSourceColumnValues(column, sourceIndices);
				// And finally we want to copy those column values back to the destination
				// buffer IN THEIR CORRECT ORDER.
				CopyColumnValuesToBuffer(sourceIndices, buffer, destinationIndex);
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
			else if (Equals(column, LogFileColumns.Index) ||
			         Equals(column, LogFileColumns.OriginalIndex))
			{
				GetIndices(indices, (LogLineIndex[]) (object) buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LogEntryIndex))
			{
				GetLogEntryIndices(indices, (LogEntryIndex[])(object)buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LineNumber) ||
			         Equals(column, LogFileColumns.OriginalLineNumber))
			{
				GetLineNumbers(indices, (int[]) (object) buffer, destinationIndex);
			}
			else
			{
				var sourceIndices = GetOriginalLogLineIndices<T>(indices);
				GetSourceColumnValues(column, sourceIndices);
				CopyColumnValuesToBuffer(sourceIndices, buffer, destinationIndex);
			}
		}

		#region Retrieving Column Values from source files

		private Dictionary<int, Stuff<T>> GetOriginalLogLineIndices<T>(IReadOnlyList<LogLineIndex> indices)
		{
			var sourceIndices = new Dictionary<int, Stuff<T>>();

			lock (_syncRoot)
			{
				// Do NOT call any virtual methods
				// Do NOT block for any amount of time

				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					if (index >= 0 && index < _indices.Count)
					{
						var sourceIndex = _indices[index.Value];
						Stuff<T> stuff;
						if (!sourceIndices.TryGetValue(sourceIndex.LogFileIndex, out stuff))
						{
							stuff = new Stuff<T>();
							sourceIndices.Add(sourceIndex.LogFileIndex, stuff);
						}
						stuff.Add(i, sourceIndex.SourceLineIndex);
					}
					else
					{
						const int invalidIndex = -1;
						Stuff<T> stuff;
						if (!sourceIndices.TryGetValue(invalidIndex, out stuff))
						{
							stuff = new Stuff<T>();
							sourceIndices.Add(invalidIndex, stuff);
						}
						stuff.Add(i, invalidIndex);
					}
				}
			}

			return sourceIndices;
		}

		private void GetSourceColumnValues<T>(ILogFileColumn<T> column, Dictionary<int, Stuff<T>> originalBuffers)
		{
			foreach (var pair in originalBuffers)
			{
				var sourceLogFileIndex = pair.Key;
				var stuff = pair.Value;
				var indices = stuff.OriginalLogLineIndices;
				var columnBuffer = stuff.Buffer;

				if (sourceLogFileIndex >= 0 &&
				    sourceLogFileIndex < _sources.Count)
				{
					var sourceLogFile = _sources[sourceLogFileIndex];
					sourceLogFile.GetColumn(indices, column, columnBuffer, 0);
				}
				else
				{
					// Someone should be on the naughty list for trying to access a portion
					// which cannot be accessed! Anyhow, we fill the buffer with default values.
					columnBuffer.Fill(column.DefaultValue, 0, indices.Count);
				}
			}
		}

		private void CopyColumnValuesToBuffer<T>(Dictionary<int, Stuff<T>> indices, T[] buffer, int destinationIndex)
		{
			foreach (var pair in indices)
			{
				var stuff = pair.Value;
				var sourceColumnValues = stuff.Buffer;

				for (int i = 0; i < stuff.DestinationIndices.Count; ++i)
				{
					var destIndex = destinationIndex + stuff.DestinationIndices[i];
					buffer[destIndex] = sourceColumnValues[i];
				}
			}
		}

		/// <summary>
		///     Keeps track of which indices of a source log file are being requested,
		///     as well as their index into a destination buffer.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		sealed class Stuff<T>
		{
			private readonly List<LogLineIndex> _originalLogLineIndices;
			private readonly List<int> _destinationIndices;

			public IReadOnlyList<int> DestinationIndices => _destinationIndices;

			private T[] _buffer;

			public Stuff()
			{
				_destinationIndices = new List<int>();
				_originalLogLineIndices = new List<LogLineIndex>();
			}

			public IReadOnlyList<LogLineIndex> OriginalLogLineIndices => _originalLogLineIndices;

			public T[] Buffer
			{
				get
				{
					if (_buffer == null)
					{
						_buffer = new T[_originalLogLineIndices.Count];
					}
					return _buffer;
				}
			}

			public void Add(int destIndex, LogLineIndex index)
			{
				_destinationIndices.Add(destIndex);
				_originalLogLineIndices.Add(index);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="indices"></param>
		/// <param name="buffer"></param>
		/// <param name="destinationIndex"></param>
		private void GetIndices(IReadOnlyList<LogLineIndex> indices, LogLineIndex[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					if (index >= 0 && index < _indices.Count)
					{
						buffer[destinationIndex + i] = index;
					}
					else
					{
						buffer[destinationIndex + i] = LogFileColumns.Index.DefaultValue;
					}
				}
			}
		}

		private void GetLogEntryIndices(IReadOnlyList<LogLineIndex> indices, LogEntryIndex[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					if (index >= 0 && index < _indices.Count)
					{
						buffer[destinationIndex + i] = _indices[index.Value].MergedLogEntryIndex;
					}
					else
					{
						buffer[destinationIndex + i] = LogEntryIndex.Invalid;
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="indices"></param>
		/// <param name="buffer"></param>
		/// <param name="destinationIndex"></param>
		private void GetLineNumbers(IReadOnlyList<LogLineIndex> indices, int[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					if (index >= 0 && index < _indices.Count)
					{
						buffer[destinationIndex + i] = (int) (index + 1);
					}
					else
					{
						buffer[destinationIndex + i] = LogFileColumns.LineNumber.DefaultValue;
					}
				}
			}
		}

		/// <summary>
		///     Retrieves values for the "delta_time" column for the given rows denoted by <paramref name="section"/>.
		/// </summary>
		/// <remarks>
		///     The values for this column aren't stored here, hence we have to compute
		///     them on-the-fly.
		/// </remarks>
		/// <param name="section">The section of rows to retrieve</param>
		/// <param name="buffer"></param>
		/// <param name="destinationIndex"></param>
		private void GetDeltaTime(LogFileSection section, TimeSpan?[] buffer, int destinationIndex)
		{
			var timestamps = new DateTime?[section.Count + 1];
			GetColumn(new LogFileSection(section.Index - 1, section.Count + 1), LogFileColumns.Timestamp, timestamps, 0);
			for (int i = 0; i < section.Count; ++i)
			{
				var previous = timestamps[i];
				var current = timestamps[i + 1];
				buffer[destinationIndex + i] = current - previous;
			}
		}

		/// <summary>
		///     Retrieves values for the "delta_time" column for the given rows denoted by <paramref name="indices"/>.
		/// </summary>
		/// <remarks>
		///     The values for this column aren't stored here, hence we have to compute
		///     them on-the-fly.
		/// </remarks>
		/// <param name="indices">The indices of the rows to retrieve</param>
		/// <param name="buffer">The buffer into which the values of the time delta column have to be written</param>
		/// <param name="destinationIndex">The index of the first value into <paramref name="buffer"/> where values have to be written</param>
		private void GetDeltaTime(IReadOnlyList<LogLineIndex> indices, TimeSpan?[] buffer, int destinationIndex)
		{
			// The easiest way to compute the time delta for (very possibly non-consecutive rows)
			// is to retrieve the timestamp for every desired row and its previous one, hence
			// we have to retrieve twice as many timestamps.
			var timestamps = new DateTime?[indices.Count * 2];
			var timestampIndices = new LogLineIndex[indices.Count * 2];
			for (int i = 0; i < indices.Count; ++i)
			{
				timestampIndices[i * 2 + 0] = indices[i] - 1;
				timestampIndices[i * 2 + 1] = indices[i];
			}
			GetColumn(timestampIndices, LogFileColumns.Timestamp, timestamps, 0);
			for (int i = 0; i < indices.Count; ++i)
			{
				var previous = timestamps[i * 2 + 0];
				var current = timestamps[i * 2 + 1];
				buffer[destinationIndex + i] = current - previous;
			}
		}

		#endregion

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

		/// <inheritdoc />
		public override void GetSection(LogFileSection section, LogLine[] dest)
		{
			for (var i = 0; i < section.Count; ++i)
				// TODO: This seems rubbish - maybe I should change the interface to SourceLineIndex altogether?
				dest[i] = GetLine((int) (section.Index + i));
		}

		/// <inheritdoc />
		public override LogLine GetLine(int index)
		{
			Index idx;
			lock (_indices)
			{
				idx = _indices[index];
			}

			var logFileIndex = idx.LogFileIndex;
			var logFile = _sources[logFileIndex];

			var line = logFile.GetLine(idx.SourceLineIndex);
			var actualLine = new LogLine(index,
				idx.MergedLogEntryIndex,
				new LogLineSourceId(logFileIndex),
				line);
			return actualLine;
		}

		/// <inheritdoc />
		protected override TimeSpan RunOnce(CancellationToken token)
		{
			PendingModification modification;
			while (_pendingModifications.TryDequeue(out modification))
			{
				if (token.IsCancellationRequested)
					return TimeSpan.Zero;

				_totalLineCount = CalculateTotalLogLineCount();
				_progress = Percentage.Of(_indices.Count, _totalLineCount);

				if (modification.Section.IsReset)
				{
					Clear(modification.LogFile);
				}
				else if (modification.Section.IsInvalidate)
				{
					throw new NotImplementedException();
				}
				else
				{
					for (var i = 0; i < modification.Section.Count; ++i)
					{
						var sourceIndex = modification.Section.Index + i;
						var newLogLine = modification.LogFile.GetLine((int) sourceIndex);
						if (newLogLine.Timestamp != null)
						{
							// We need to find out where this new entry (or entries) is/are to be inserted.
							var insertionIndex = _indices.Count;
							byte logFileIndex;
							_logFileIndices.TryGetValue(modification.LogFile, out logFileIndex);
							for (var n = _indices.Count - 1; n >= 0; --n)
							{
								var idx = _indices[n];
								var logFile = _sources[idx.LogFileIndex];
								var entry = logFile.GetLine(idx.SourceLineIndex);
								if (entry.Timestamp <= newLogLine.Timestamp)
								{
									insertionIndex = n + 1;
									break;
								}
								if (entry.Timestamp > newLogLine.Timestamp)
									insertionIndex = n;
							}

							var mergedLogEntryIndex = GetMergedLogEntryIndex(modification.LogFile, insertionIndex, newLogLine);
							var index = new Index((int) sourceIndex,
								mergedLogEntryIndex,
								newLogLine.LogEntryIndex,
								logFileIndex);
							if (insertionIndex < _indices.Count)
								InvalidateOnward(insertionIndex, modification.LogFile, newLogLine);

							lock (_syncRoot)
							{
								_indices.Insert(insertionIndex, index);
								_maxCharactersPerLine = Math.Max(_maxCharactersPerLine, newLogLine.Message?.Length ?? 0);
							}

							Listeners.OnRead(_indices.Count);

							// There's no need to frantically update every time, but every
							// once in a while would be nice...
							if (i % 100 == 0)
								UpdateProperties();
						}
					}
				}
			}

			UpdateProperties();

			Listeners.OnRead(_indices.Count);
			SetEndOfSourceReached();

			return _maximumWaitTime;
		}

		private void UpdateProperties()
		{
			Size? size = null;
			DateTime? lastModified = null;
			DateTime? startTimestamp = null;
			DateTime? endTimestamp = null;
			for (int n = 0; n < _sources.Count; ++n)
			{
				var source = _sources[n];

				var sourceSize = source.GetValue(LogFileProperties.Size);
				if (size == null)
					size = sourceSize;
				else if (sourceSize != null)
					size += sourceSize;

				var last = source.GetValue(LogFileProperties.LastModified);
				if (last != null && (last > lastModified || lastModified == null))
					lastModified = last;
				var start = source.GetValue(LogFileProperties.StartTimestamp);
				if (start != null && (start < startTimestamp || startTimestamp == null))
					startTimestamp = start;
				var end = source.GetValue(LogFileProperties.EndTimestamp);
				if (end != null && (end > endTimestamp || endTimestamp == null))
					endTimestamp = end;
			}

			_properties.SetValue(LogFileProperties.LastModified, lastModified);
			_properties.SetValue(LogFileProperties.Size, size);
			_properties.SetValue(LogFileProperties.StartTimestamp, startTimestamp);
			_properties.SetValue(LogFileProperties.EndTimestamp, endTimestamp);
			_progress = Percentage.Of(_indices.Count, _totalLineCount);
		}

		[Pure]
		private int CalculateTotalLogLineCount()
		{
			var count = 0;
			foreach (var logFile in _sources)
				// TODO: Introduce separate property that counts the number of lines with a timestamp as only those are of interest to us
				count += logFile.Count;
			return count;
		}

		/// <summary>
		///     Finds the log entry index for the given log line in this merged data structure.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="insertionIndex"></param>
		/// <param name="newLogLine"></param>
		/// <returns></returns>
		[Pure]
		private int GetMergedLogEntryIndex(ILogFile logFile, int insertionIndex, LogLine newLogLine)
		{
			if (insertionIndex > 0)
			{
				var previousLine = _indices[insertionIndex - 1];
				var previousLineLogFile = _sources[previousLine.LogFileIndex];

				if (previousLineLogFile == logFile &&
				    previousLine.OriginalLogEntryIndex == newLogLine.LogEntryIndex)
					return previousLine.MergedLogEntryIndex;

				return previousLine.MergedLogEntryIndex + 1;
			}

			return 0;
		}

		private void InvalidateOnward(int insertionIndex, ILogFile source, LogLine newLogLine)
		{
			// If the new entry is to be inserted anywhere else, then we need to invalidate
			// everything from that index on, insert the new line at the given index and then
			// issue another modification that includes everything from the newly inserted index
			// to the end.
			var count = _indices.Count - insertionIndex;
			Listeners.Invalidate(insertionIndex, count);

			// This is really interesting.
			// We're inserting a line somewhere in the middle which means that the logentry index of all following
			// entries MAY increase by 1, depending on whether or not the inserted log line is a new entry
			// or belongs to the previous line's entry
			var patchFollowingIndices = true;
			if (insertionIndex > 0)
			{
				var previousLine = _indices[insertionIndex - 1];
				var previousLineLogFile = _sources[previousLine.LogFileIndex];

				if (previousLineLogFile == source &&
				    previousLine.OriginalLogEntryIndex == newLogLine.LogEntryIndex)
					patchFollowingIndices = false;
			}

			if (patchFollowingIndices)
				for (var i = 0; i < count; ++i)
				{
					var idx = _indices[insertionIndex + i];
					idx.MergedLogEntryIndex++;
					_indices[insertionIndex + i] = idx;
				}
		}

		private void Clear(ILogFile logFile)
		{
			var numRemoved = 0;
			lock (_syncRoot)
			{
				for (var i = _indices.Count - 1; i >= 0; --i)
				{
					var index = _indices[i];
					var indexLogFile = _sources[index.LogFileIndex];
					if (indexLogFile == logFile)
					{
						_indices.RemoveAt(i);
						++numRemoved;
					}
				}
			}

			if (numRemoved > 0)
			{
				Listeners.Reset();
				Listeners.OnRead(_indices.Count);
			}
		}

		/// <summary>
		///     Represents an index in the merged data-structure.
		///     Points towards a particular <see cref="LogLine" /> of a particular
		///     <see cref="ILogFile" />.
		/// </summary>
		private struct Index
		{
			public readonly int OriginalLogEntryIndex;
			public readonly int SourceLineIndex;
			public readonly byte LogFileIndex;
			public int MergedLogEntryIndex;

			public Index(int sourceLineIndex,
				int mergedLogEntryIndex,
				int originalLogEntryIndex,
				byte logFileIndex)
			{
				SourceLineIndex = sourceLineIndex;
				MergedLogEntryIndex = mergedLogEntryIndex;
				OriginalLogEntryIndex = originalLogEntryIndex;
				LogFileIndex = logFileIndex;
			}

			public override string ToString()
			{
				return string.Format("SourceLineIndex: {0}, OriginalLogEntryIndex: {1}, LogFile: {2}, MergedLogEntryIndex: {3}",
					SourceLineIndex, OriginalLogEntryIndex, LogFileIndex, MergedLogEntryIndex);
			}
		}

		private struct PendingModification
		{
			public readonly ILogFile LogFile;
			public readonly LogFileSection Section;

			public PendingModification(ILogFile logFile, LogFileSection section)
			{
				LogFile = logFile;
				Section = section;
			}

			public override string ToString()
			{
				return string.Format("{0} ({1})", Section, LogFile);
			}
		}
	}
}
