using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using Metrolib;
using Tailviewer.Api;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Properties;

namespace Tailviewer.Core.Sources.Merged
{
	/// <summary>
	///     Responsible for merging two or more <see cref="ILogSource" /> instances into one coherent view.
	///     <see cref="IReadOnlyLogEntry" />s which have no <see cref="IReadOnlyLogEntry.Timestamp" /> set (or who's parent doesn't)
	///     are discarded from this representation.
	/// </summary>
	/// <remarks>
	///    Plugin authors are deliberately prevented from instantiating this type directly because it's constructor signature may change
	///    over time. In order to create an instance of this type, simply call <see cref="ILogSourceFactory.CreateMergedLogFile"/>
	///    who's signature is guaranteed to never change.
	/// </remarks>
	[DebuggerTypeProxy(typeof(LogSourceDebuggerVisualization))]
	internal sealed class MergedLogSource
		: AbstractLogSource
		, IMergedLogFile
		, ILogSourceListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private const int MaximumBatchSizePerSource = 1000;

		private readonly MergedLogSourceIndex _index;
		private readonly TimeSpan _maximumWaitTime;
		private readonly IReadOnlyList<IColumnDescriptor> _columns;

		private readonly ConcurrentQueue<MergedLogSourcePendingModification> _pendingModifications;
		private readonly ConcurrentPropertiesList _properties;
		private readonly PropertiesBufferList _propertiesBuffer;
		private readonly IReadOnlyList<ILogSource> _sources;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="scheduler"></param>
		/// <param name="maximumWaitTime"></param>
		/// <param name="sources"></param>
		internal MergedLogSource(ITaskScheduler scheduler, TimeSpan maximumWaitTime, IEnumerable<ILogSource> sources)
			: this(scheduler, maximumWaitTime, sources.ToArray())
		{
		}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <remarks>
		///    Plugin authors are deliberately prevented from calling this constructor directly because it's signature may change
		///    over time. In order to create an instance of this type, simply call <see cref="ILogSourceFactory.CreateMergedLogFile"/>.
		/// </remarks>
		/// <param name="scheduler"></param>
		/// <param name="maximumWaitTime"></param>
		/// <param name="sources"></param>
		internal MergedLogSource(ITaskScheduler scheduler, TimeSpan maximumWaitTime, params ILogSource[] sources)
			: base(scheduler)
		{
			if (sources == null) throw new ArgumentNullException(nameof(sources));
			if (sources.Any(x => x == null)) throw new ArgumentException("sources.Any(x => x == null)", nameof(sources));
			if (sources.Length > LogEntrySourceId.MaxSources) throw new ArgumentException(string.Format("Only up to {0} sources are supported ({1} were given)", LogEntrySourceId.MaxSources, sources.Length));

			_sources = sources;
			_index = new MergedLogSourceIndex(sources);
			_pendingModifications = new ConcurrentQueue<MergedLogSourcePendingModification>();
			_maximumWaitTime = maximumWaitTime;
			_columns = sources.SelectMany(x => x.Columns).Concat(new[] {GeneralColumns.SourceId}).Distinct().ToList();
			_propertiesBuffer = new PropertiesBufferList(GeneralProperties.Minimum);
			_properties = new ConcurrentPropertiesList(GeneralProperties.Minimum);

			foreach (var logFile in _sources)
			{
				logFile.AddListener(this, maximumWaitTime, MaximumBatchSizePerSource);
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
		public IReadOnlyList<ILogSource> Sources => _sources;

		/// <inheritdoc />
		public override IReadOnlyList<IColumnDescriptor> Columns => _columns;

		/// <inheritdoc />
		public override IReadOnlyList<IReadOnlyPropertyDescriptor> Properties => _properties.Properties;

		/// <inheritdoc />
		public override object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			_properties.TryGetValue(property, out var value);
			return value;
		}

		/// <inheritdoc />
		public override T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			_properties.TryGetValue(property, out var value);
			return value;
		}

		public override void SetProperty(IPropertyDescriptor property, object value)
		{
			foreach (var source in _sources)
			{
				source.SetProperty(property, value);
			}
		}

		public override void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			foreach (var source in _sources)
			{
				source.SetProperty(property, value);
			}
		}

		/// <inheritdoc />
		public override void GetAllProperties(IPropertiesBuffer destination)
		{
			_properties.CopyAllValuesTo(destination);
		}

		/// <inheritdoc />
		public void OnLogFileModified(ILogSource logSource, LogSourceModification modification)
		{
			if (Log.IsDebugEnabled)
				Log.DebugFormat("OnLogFileModified({0}, {1})", logSource, modification);

			_pendingModifications.Enqueue(new MergedLogSourcePendingModification(logSource, modification));
		}

		/// <inheritdoc />
		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, IColumnDescriptor<T> column, T[] destination, int destinationIndex, LogSourceQueryOptions queryOptions)
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

			if (Equals(column, GeneralColumns.ElapsedTime))
			{
				GetElapsedTime(sourceIndices, (TimeSpan?[]) (object) destination, destinationIndex, queryOptions);
			}
			else if (Equals(column, GeneralColumns.DeltaTime))
			{
				GetDeltaTime(sourceIndices, (TimeSpan?[])(object)destination, destinationIndex, queryOptions);
			}
			else if (Equals(column, GeneralColumns.Index) ||
			         Equals(column, GeneralColumns.OriginalIndex))
			{
				_index.GetLogLineIndices(sourceIndices, (LogLineIndex[]) (object) destination, destinationIndex);
			}
			else if (Equals(column, GeneralColumns.LogEntryIndex))
			{
				_index.GetLogEntryIndices(sourceIndices, (LogEntryIndex[])(object)destination, destinationIndex);
			}
			else if (Equals(column, GeneralColumns.LineNumber) ||
			         Equals(column, GeneralColumns.OriginalLineNumber))
			{
				_index.GetLineNumbers(sourceIndices, (int[]) (object) destination, destinationIndex);
			}
			else if (Equals(column, GeneralColumns.SourceId))
			{
				_index.GetSourceIds(sourceIndices, (LogEntrySourceId[]) (object) destination, destinationIndex);
			}
			else
			{
				var actualSourceIndices = _index.GetOriginalLogLineIndices<T>(sourceIndices);
				GetSourceColumnValues(column, actualSourceIndices, queryOptions);
				CopyColumnValuesToBuffer(actualSourceIndices, destination, destinationIndex);
			}
		}

		#region Retrieving Column Values from source files

		private void GetSourceColumnValues<T>(IColumnDescriptor<T> column, Dictionary<int, Stuff<T>> originalBuffers, LogSourceQueryOptions queryOptions)
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
		private void GetElapsedTime(IReadOnlyList<LogLineIndex> indices, TimeSpan?[] buffer, int destinationIndex, LogSourceQueryOptions queryOptions)
		{
			var start = GetProperty(GeneralProperties.StartTimestamp);
			var timestamps = new DateTime?[indices.Count];
			GetColumn(indices, GeneralColumns.Timestamp, timestamps, 0, queryOptions);
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
		private void GetDeltaTime(IReadOnlyList<LogLineIndex> indices, TimeSpan?[] buffer, int destinationIndex, LogSourceQueryOptions queryOptions)
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
			GetColumn(timestampIndices, GeneralColumns.Timestamp, timestamps, 0, queryOptions);
			for (int i = 0; i < indices.Count; ++i)
			{
				var previous = timestamps[i * 2 + 0];
				var current = timestamps[i * 2 + 1];
				buffer[destinationIndex + i] = current - previous;
			}
		}

		#endregion

		/// <inheritdoc />
		public override void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, int destinationIndex, LogSourceQueryOptions queryOptions)
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
			bool performedWork = false;
			while (TryDequeueUpTo(maxLineCount, out var modifications))
			{
				performedWork = true;

				var changes = _index.Process(modifications);
				UpdateProperties();
				NotifyListeners(changes);
			}

			if (!performedWork)
				UpdateProperties();

			if (_pendingModifications.IsEmpty && _properties.GetValue(GeneralProperties.PercentageProcessed) == Percentage.HundredPercent)
				Listeners.Flush();

			return _maximumWaitTime;
		}

		private bool TryDequeueUpTo(int maxLineCount, out IEnumerable<MergedLogSourcePendingModification> sections)
		{
			var tmp = new List<MergedLogSourcePendingModification>();
			int count = 0;

			while (_pendingModifications.TryDequeue(out var modification))
			{
				tmp.Add(modification);
				if (modification.Modification.IsAppended(out var section))
					count += section.Count;

				if (count >= maxLineCount)
					break;
			}

			sections = tmp;
			return tmp.Count > 0;
		}

		private void NotifyListeners(IEnumerable<LogSourceModification> changes)
		{
			foreach (var section in changes)
			{
				if (section.IsRemoved(out var removedSection))
				{
					Listeners.Remove((int) removedSection.Index, removedSection.Count);
				}
				else if (section.IsReset())
				{
					Listeners.Reset();
				}
				else if (section.IsAppended(out var appendedSection))
				{
					Listeners.OnRead((int) (appendedSection.Index + appendedSection.Count));
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
				source.GetAllProperties(_propertiesBuffer);

				var sourceSize = _propertiesBuffer.GetValue(GeneralProperties.Size);
				if (size == null)
					size = sourceSize;
				else if (sourceSize != null)
					size += sourceSize;

				var last = _propertiesBuffer.GetValue(GeneralProperties.LastModified);
				if (last != null && (last > lastModified || lastModified == null))
					lastModified = last;
				var start = _propertiesBuffer.GetValue(GeneralProperties.StartTimestamp);
				if (start != null && (start < startTimestamp || startTimestamp == null))
					startTimestamp = start;
				var end = _propertiesBuffer.GetValue(GeneralProperties.EndTimestamp);
				if (end != null && (end > endTimestamp || endTimestamp == null))
					endTimestamp = end;
				maxCharactersPerLine = Math.Max(maxCharactersPerLine, _propertiesBuffer.GetValue(TextProperties.MaxCharactersInLine));

				var sourceProcessed = _propertiesBuffer.GetValue(GeneralProperties.PercentageProcessed);
				processed *= sourceProcessed;
			}

			_propertiesBuffer.SetValue(GeneralProperties.LogEntryCount, _index.Count);
			_propertiesBuffer.SetValue(TextProperties.MaxCharactersInLine, maxCharactersPerLine);
			_propertiesBuffer.SetValue(GeneralProperties.PercentageProcessed, processed);
			_propertiesBuffer.SetValue(GeneralProperties.LastModified, lastModified);
			_propertiesBuffer.SetValue(GeneralProperties.Size, size);
			_propertiesBuffer.SetValue(GeneralProperties.StartTimestamp, startTimestamp);
			_propertiesBuffer.SetValue(GeneralProperties.EndTimestamp, endTimestamp);
			_propertiesBuffer.SetValue(GeneralProperties.Duration, endTimestamp - startTimestamp);

			// We want to ensure that we modify all properties at once so that users of this log file don't
			// see an inconsistent state of properties when they retrieve them.
			_properties.CopyFrom(_propertiesBuffer);
		}
	}
}
