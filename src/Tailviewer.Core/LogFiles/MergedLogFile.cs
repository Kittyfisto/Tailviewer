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
	/// <remarks>
	///    Plugin authors are deliberately prevented from instantiating this type directly because it's constructor signature may change
	///    over time. In order to create an instance of this type, simply call <see cref="IServiceContainer.CreateMergedLogFile"/>
	///    who's signature is guaranteed to never change.
	/// </remarks>
	[DebuggerTypeProxy(typeof(LogFileView))]
	internal sealed class MergedLogFile
		: AbstractLogFile
		, IMergedLogFile
		, ILogFileListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private const int MaximumBatchSizePerSource = 1000;

		private readonly MergedLogFileIndex _index;
		private readonly TimeSpan _maximumWaitTime;
		private readonly IReadOnlyList<ILogFileColumnDescriptor> _columns;

		private readonly ConcurrentQueue<MergedLogFilePendingModification> _pendingModifications;
		private readonly LogFilePropertyList _properties;
		private readonly IReadOnlyList<ILogFile> _sources;
		private int _maxCharactersPerLine;
		private Percentage _progress = Percentage.Zero;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="scheduler"></param>
		/// <param name="maximumWaitTime"></param>
		/// <param name="sources"></param>
		internal MergedLogFile(ITaskScheduler scheduler, TimeSpan maximumWaitTime, IEnumerable<ILogFile> sources)
			: this(scheduler, maximumWaitTime, sources.ToArray())
		{
		}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <remarks>
		///    Plugin authors are deliberately prevented from calling this constructor directly because it's signature may change
		///    over time. In order to create an instance of this type, simply call <see cref="IServiceContainer.CreateMergedLogFile"/>.
		/// </remarks>
		/// <param name="scheduler"></param>
		/// <param name="maximumWaitTime"></param>
		/// <param name="sources"></param>
		internal MergedLogFile(ITaskScheduler scheduler, TimeSpan maximumWaitTime, params ILogFile[] sources)
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
			_columns = sources.SelectMany(x => x.Columns).Concat(new[] {LogFileColumns.SourceId}).Distinct().ToList();
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

			// https://github.com/Kittyfisto/Tailviewer/issues/282
			_index.Clear();
			_properties.Clear();

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
		public override int MaxCharactersPerLine => _maxCharactersPerLine;

		/// <inheritdoc />
		public override IReadOnlyList<ILogFileColumnDescriptor> Columns => _columns;

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
		public override void GetAllValues(ILogFileProperties destination)
		{
			_properties.CopyAllValuesTo(destination);
		}

		/// <inheritdoc />
		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			if (Log.IsDebugEnabled)
				Log.DebugFormat("OnLogFileModified({0}, {1})", logFile, section);

			_pendingModifications.Enqueue(new MergedLogFilePendingModification(logFile, section));
			ResetEndOfSourceReached();
		}

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

			if (Equals(column, LogFileColumns.ElapsedTime))
			{
				GetElapsedTime(sourceIndices, (TimeSpan?[]) (object) destination, destinationIndex, queryOptions);
			}
			else if (Equals(column, LogFileColumns.DeltaTime))
			{
				GetDeltaTime(sourceIndices, (TimeSpan?[])(object)destination, destinationIndex, queryOptions);
			}
			else if (Equals(column, LogFileColumns.Index) ||
			         Equals(column, LogFileColumns.OriginalIndex))
			{
				_index.GetLogLineIndices(sourceIndices, (LogLineIndex[]) (object) destination, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LogEntryIndex))
			{
				_index.GetLogEntryIndices(sourceIndices, (LogEntryIndex[])(object)destination, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LineNumber) ||
			         Equals(column, LogFileColumns.OriginalLineNumber))
			{
				_index.GetLineNumbers(sourceIndices, (int[]) (object) destination, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.SourceId))
			{
				_index.GetSourceIds(sourceIndices, (LogLineSourceId[]) (object) destination, destinationIndex);
			}
			else
			{
				var actualSourceIndices = _index.GetOriginalLogLineIndices<T>(sourceIndices);
				GetSourceColumnValues(column, actualSourceIndices, queryOptions);
				CopyColumnValuesToBuffer(actualSourceIndices, destination, destinationIndex);
			}
		}

		#region Retrieving Column Values from source files

		private void GetSourceColumnValues<T>(ILogFileColumnDescriptor<T> column, Dictionary<int, Stuff<T>> originalBuffers, LogFileQueryOptions queryOptions)
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
					if (sourceLogFile.Columns.Contains(column))
						sourceLogFile.GetColumn(indices, column, columnBuffer, 0, queryOptions);
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
		///     Retrieves values for the "elapsed_time" column for the given rows denoted by <paramref name="indices"/>.
		/// </summary>
		/// <remarks>
		///     The values for this column aren't stored here, hence we have to compute
		///     them on-the-fly.
		/// </remarks>
		/// <param name="indices">The indices of the rows to retrieve</param>
		/// <param name="buffer">The buffer into which the values of the time delta column have to be written</param>
		/// <param name="destinationIndex">The index of the first value into <paramref name="buffer"/> where values have to be written</param>
		/// <param name="queryOptions"></param>
		private void GetElapsedTime(IReadOnlyList<LogLineIndex> indices, TimeSpan?[] buffer, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			var start = GetValue(LogFileProperties.StartTimestamp);
			var timestamps = new DateTime?[indices.Count];
			GetColumn(indices, LogFileColumns.Timestamp, timestamps, 0, queryOptions);
			for (int i = 0; i < indices.Count; ++i)
			{
				var current = timestamps[i];
				buffer[destinationIndex + i] = current - start;
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
		/// <param name="queryOptions"></param>
		private void GetDeltaTime(IReadOnlyList<LogLineIndex> indices, TimeSpan?[] buffer, int destinationIndex, LogFileQueryOptions queryOptions)
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
			GetColumn(timestampIndices, LogFileColumns.Timestamp, timestamps, 0, queryOptions);
			for (int i = 0; i < indices.Count; ++i)
			{
				var previous = timestamps[i * 2 + 0];
				var current = timestamps[i * 2 + 1];
				buffer[destinationIndex + i] = current - previous;
			}
		}

		#endregion

		/// <inheritdoc />
		public override void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogEntries destination, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			// TODO: This can probably be optimized (why are we translating indices each time for every column?!
			foreach (var column in destination.Columns)
			{
				destination.CopyFrom(column, destinationIndex, this, sourceIndices, queryOptions);
			}
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
			Percentage processed = Percentage.HundredPercent;
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

				var sourceProcessed = source.GetValue(LogFileProperties.PercentageProcessed);
				processed *= sourceProcessed;
			}

			_properties.SetValue(LogFileProperties.PercentageProcessed, processed);
			_properties.SetValue(LogFileProperties.LastModified, lastModified);
			_properties.SetValue(LogFileProperties.Size, size);
			_properties.SetValue(LogFileProperties.StartTimestamp, startTimestamp);
			_properties.SetValue(LogFileProperties.EndTimestamp, endTimestamp);
			_properties.SetValue(LogFileProperties.Duration, endTimestamp - startTimestamp);
			_maxCharactersPerLine = maxCharactersPerLine;
		}
	}
}
