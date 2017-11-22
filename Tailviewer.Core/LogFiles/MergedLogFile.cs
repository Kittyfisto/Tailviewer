using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
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
		private readonly object _syncRoot;
		private Size _fileSize;
		private DateTime _lastModified;
		private int _maxCharactersPerLine;
		private Percentage _progress;

		private DateTime? _startTimestamp;
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

		/// <summary>
		/// </summary>
		/// <remarks>
		///     This should return false in order to show a detailed error
		///     message as to why a view is empty, however I'm not sure if stating
		///     "All data sources do not exist" is such an improvement over
		///     "The data source is empty".
		/// </remarks>
		public override ErrorFlags Error => ErrorFlags.None;

		/// <inheritdoc />
		public override bool EndOfSourceReached
		{
			get { return _sources.All(x => x.EndOfSourceReached) & base.EndOfSourceReached; }
		}

		/// <inheritdoc />
		public override DateTime? StartTimestamp => _startTimestamp;

		/// <inheritdoc />
		public override DateTime LastModified => _lastModified;

		/// <inheritdoc />
		public override DateTime Created => DateTime.MinValue;

		/// <inheritdoc />
		public override Size Size => _fileSize;

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
		public override double Progress => _progress.RelativeValue;

		/// <inheritdoc />
		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			_pendingModifications.Enqueue(new PendingModification(logFile, section));
			ResetEndOfSourceReached();
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
								_maxCharactersPerLine = Math.Max(_maxCharactersPerLine, newLogLine.Message.Length);
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
			var fileSize = Size.Zero;
			var lastModified = DateTime.MinValue;
			DateTime? startTimestamp = null;
			for (int n = 0; n < _sources.Count; ++n)
			{
				var source = _sources[n];

				fileSize += source.Size;
				var last = source.LastModified;
				if (last > lastModified)
					lastModified = last;
				var start = source.StartTimestamp;
				if (start != null && (start < startTimestamp || startTimestamp == null))
					startTimestamp = start;
			}
			_fileSize = fileSize;
			_lastModified = lastModified;
			_startTimestamp = startTimestamp;
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
			lock (_indices)
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