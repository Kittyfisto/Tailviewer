using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles.Merged;

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
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private const int MaximumBatchSizePerSource = 1000;

		private readonly MergedLogFileIndex _index;
		private readonly TimeSpan _maximumWaitTime;

		private readonly ConcurrentQueue<MergedLogFilePendingModification> _pendingModifications;
		private readonly ILogFileProperties _properties;
		private readonly IReadOnlyList<ILogFile> _sources;
		private int _maxCharactersPerLine;
		private Percentage _progress = Percentage.Zero;

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
			if (sources.Length > LogLineSourceId.MaxSources) throw new ArgumentException(string.Format("Only up to {0} sources are supported ({1} were given)", LogLineSourceId.MaxSources, sources.Length));

			_sources = sources;
			_index = new MergedLogFileIndex(sources);
			_pendingModifications = new ConcurrentQueue<MergedLogFilePendingModification>();
			var logFileIndices = new Dictionary<ILogFile, byte>();
			_maximumWaitTime = maximumWaitTime;
			_properties = new LogFilePropertyList(LogFileProperties.Minimum);

			byte idx = 0;
			foreach (var logFile in _sources)
			{
				logFile.AddListener(this, maximumWaitTime, MaximumBatchSizePerSource);
				logFileIndices.Add(logFile, idx);

				++idx;
			}
			StartTask();
		}

		/// <inheritdoc />
		protected override void DisposeAdditional()
		{
			foreach (var source in _sources)
			{
				source.RemoveListener(this);
			}

			base.DisposeAdditional();
		}

		/// <inheritdoc />
		public IReadOnlyList<ILogFile> Sources => _sources;

		/// <inheritdoc />
		public override bool EndOfSourceReached
		{
			get { return Sources.All(x => x.EndOfSourceReached) & base.EndOfSourceReached; }
		}

		/// <inheritdoc />
		public override int Count
		{
			get { return _index.Count; }
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
			_properties.TryGetValue(propertyDescriptor, out var value);
			return value;
		}

		/// <inheritdoc />
		public override T GetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor)
		{
			_properties.TryGetValue(propertyDescriptor, out var value);
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
			if (Log.IsDebugEnabled)
				Log.DebugFormat("OnLogFileModified({0}, {1})", logFile, section);

			_pendingModifications.Enqueue(new MergedLogFilePendingModification(logFile, section));
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
				_index.GetLogLineIndices(section, (LogLineIndex[]) (object) buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LogEntryIndex))
			{
				_index.GetLogEntryIndices(section, (LogEntryIndex[]) (object) buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LineNumber) ||
			         Equals(column, LogFileColumns.OriginalLineNumber))
			{
				_index.GetLineNumbers(section, (int[]) (object) buffer, destinationIndex);
			}
			else
			{
				// We want to minimize the amount of GetColumn calls to our source files.
				// The best we can achieve is up to one call per source, which is what the following
				// code achieves:
				// At first, we want to build the list of indices we need to retrieve per source
				var sourceIndices = _index.GetOriginalLogLineIndices<T>(section);
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
				_index.GetLogLineIndices(indices, (LogLineIndex[]) (object) buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LogEntryIndex))
			{
				_index.GetLogEntryIndices(indices, (LogEntryIndex[])(object)buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LineNumber) ||
			         Equals(column, LogFileColumns.OriginalLineNumber))
			{
				_index.GetLineNumbers(indices, (int[]) (object) buffer, destinationIndex);
			}
			else
			{
				var sourceIndices = _index.GetOriginalLogLineIndices<T>(indices);
				GetSourceColumnValues(column, sourceIndices);
				CopyColumnValuesToBuffer(sourceIndices, buffer, destinationIndex);
			}
		}

		#region Retrieving Column Values from source files

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
			MergedLogLineIndex idx = _index[index];

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
			// Every Process() invocation locks the sync root until
			// the changes have been processed. The goal is to minimize
			// total process time and to prevent locking for too long.
			// The following number has been empirically determined
			// via testing and it felt alright :P
			const int maxLineCount = 5 * MaximumBatchSizePerSource;
			while (TryDequeueUpTo(maxLineCount, out var modifications))
			{
				var changes = _index.Process(modifications);
				UpdateProperties();
				NotifyListeners(changes);
			}

			_progress = Percentage.HundredPercent;
			SetEndOfSourceReached();

			return _maximumWaitTime;
		}

		private bool TryDequeueUpTo(int maxLineCount, out IEnumerable<MergedLogFilePendingModification> sections)
		{
			var tmp = new List<MergedLogFilePendingModification>();
			int count = 0;

			while (_pendingModifications.TryDequeue(out var modification))
			{
				tmp.Add(modification);
				if (!modification.Section.IsInvalidate)
					count += modification.Section.Count;

				if (count >= maxLineCount)
					break;
			}

			sections = tmp;
			return tmp.Count > 0;
		}

		private void NotifyListeners(IEnumerable<LogFileSection> changes)
		{
			foreach (var section in changes)
			{
				if (section.IsInvalidate)
				{
					Listeners.Invalidate((int) section.Index, section.Count);
				}
				else if (section.IsReset)
				{
					Listeners.Reset();
				}
				else
				{
					Listeners.OnRead((int) (section.Index + section.Count));
				}
			}
		}

		private void UpdateProperties()
		{
			Size? size = null;
			DateTime? lastModified = null;
			DateTime? startTimestamp = null;
			DateTime? endTimestamp = null;
			int maxCharactersPerLine = 0;
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
				maxCharactersPerLine = Math.Max(maxCharactersPerLine, source.MaxCharactersPerLine);
			}

			_properties.SetValue(LogFileProperties.LastModified, lastModified);
			_properties.SetValue(LogFileProperties.Size, size);
			_properties.SetValue(LogFileProperties.StartTimestamp, startTimestamp);
			_properties.SetValue(LogFileProperties.EndTimestamp, endTimestamp);
			_maxCharactersPerLine = maxCharactersPerLine;
		}
	}
}
