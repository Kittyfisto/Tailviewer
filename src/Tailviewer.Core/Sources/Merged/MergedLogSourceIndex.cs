﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using log4net;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     Responsible for storing and updating the index structure of a <see cref="MergedLogSource" />.
	///     The index consists of a mapping between source and output lines (i.e. the n-th line
	///     of the merged log file is actually the m-th line of the o-th source).
	/// </summary>
	internal sealed class MergedLogSourceIndex
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly List<MergedLogLineIndex> _indices;
		private readonly Dictionary<ILogSource, byte> _logFileIndices;
		private readonly object _syncRoot;

		public MergedLogSourceIndex(params ILogSource[] sources)
		{
			if (sources.Length > byte.MaxValue)
				throw new NotImplementedException();

			_logFileIndices = new Dictionary<ILogSource, byte>();
			for(byte i = 0; i < sources.Length; ++i)
				_logFileIndices.Add(sources[i], i);
			_indices = new List<MergedLogLineIndex>();
			_syncRoot = new object();
		}

		public int Count
		{
			get
			{
				lock (_syncRoot)
				{
					return _indices.Count;
				}
			}
		}

		public void Clear()
		{
			// https://github.com/Kittyfisto/Tailviewer/issues/282
			lock (_syncRoot)
			{
				_indices.Clear();
				_indices.Capacity = 0;

				_logFileIndices.Clear();
			}
		}

		public MergedLogLineIndex this[LogLineIndex index]
		{
			get
			{
				if (index.IsInvalid)
					return MergedLogLineIndex.Invalid;

				lock (_syncRoot)
				{
					if (index >= _indices.Count)
						return MergedLogLineIndex.Invalid;

					return _indices[(int) index];
				}
			}
		}

		[Pure]
		public IReadOnlyList<MergedLogLineIndex> Get(LogSourceSection section)
		{
			var indices = new MergedLogLineIndex[section.Count];
			lock (_syncRoot)
			{
				if (section.Index.Value < _indices.Count)
				{
					var count = Math.Min((section.Index + section.Count).Value, _indices.Count);
					_indices.CopyTo(section.Index.Value, indices, 0, count);
					for (var i = 0; i < section.Count - count; ++i)
						indices[section.LastIndex + i] = MergedLogLineIndex.Invalid;
				}
				else
				{
					for (var i = 0; i < section.Count; ++i) indices[i] = MergedLogLineIndex.Invalid;
				}
			}

			return indices;
		}

		/// <summary>
		///     Processes several changes in one go.
		///     The more changes given in one go, the better the algorithm will perform overall.
		/// </summary>
		/// <param name="pendingModifications"></param>
		/// <returns></returns>
		public IEnumerable<LogSourceModification> Process(params MergedLogSourcePendingModification[] pendingModifications)
		{
			return Process((IEnumerable<MergedLogSourcePendingModification>) pendingModifications);
		}

		/// <summary>
		///     Processes several changes in one go.
		///     The more changes given in one go, the better the algorithm will perform overall.
		/// </summary>
		/// <param name="pendingModifications"></param>
		/// <returns></returns>
		public IEnumerable<LogSourceModification> Process(IEnumerable<MergedLogSourcePendingModification> pendingModifications)
		{
			var optimizedModifications = MergedLogSourcePendingModification.Optimize(pendingModifications);
			var entries = GetEntries(optimizedModifications);
			return Process(entries);
		}

		public void GetLogLineIndices(IReadOnlyList<LogLineIndex> indices, LogLineIndex[] destination, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					if (index >= 0 && index < _indices.Count)
					{
						destination[destinationIndex + i] = index;
					}
					else
					{
						destination[destinationIndex + i] = Columns.Index.DefaultValue;
					}
				}
			}
		}

		public void GetLogEntryIndices(IReadOnlyList<LogLineIndex> sourceIndices, LogEntryIndex[] destination, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < sourceIndices.Count; ++i)
				{
					var index = sourceIndices[i];
					if (index >= 0 && index < _indices.Count)
					{
						destination[destinationIndex + i] = _indices[index.Value].MergedLogEntryIndex;
					}
					else
					{
						destination[destinationIndex + i] = LogEntryIndex.Invalid;
					}
				}
			}
		}

		public void GetLineNumbers(IReadOnlyList<LogLineIndex> sourceIndices, int[] destination, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < sourceIndices.Count; ++i)
				{
					var index = sourceIndices[i];
					if (index >= 0 && index < _indices.Count)
					{
						destination[destinationIndex + i] = (int) (index + 1);
					}
					else
					{
						destination[destinationIndex + i] = Columns.LineNumber.DefaultValue;
					}
				}
			}
		}

		public void GetSourceIds(IReadOnlyList<LogLineIndex> sourceIndices, LogEntrySourceId[] destination, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < sourceIndices.Count; ++i)
				{
					var index = sourceIndices[i].Value;
					if (index >= 0 && index < _indices.Count)
					{
						destination[destinationIndex + i] = new LogEntrySourceId(_indices[index].SourceId);
					}
					else
					{
						destination[destinationIndex + i] = Columns.SourceId.DefaultValue;
					}
				}
			}
		}

		[Pure]
		public Dictionary<int, Stuff<T>> GetOriginalLogLineIndices<T>(IReadOnlyList<LogLineIndex> indices)
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
						if (!sourceIndices.TryGetValue(sourceIndex.SourceId, out stuff))
						{
							stuff = new Stuff<T>();
							sourceIndices.Add(sourceIndex.SourceId, stuff);
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

		private IReadOnlyList<LogSourceModification> Process(IReadOnlyList<MergedLogSourceSection> pendingModifications)
		{
			// This method keeps track of the changes made
			// to the index structure in this object and then returns
			// the list once it is done.
			// It will try to compress the changes to the bare minimum
			// (which it can do if given more pending modifications at once).

			lock (_syncRoot)
			{
				var stopwatch = Stopwatch.StartNew();

				var changes = new MergedLogSourceChanges(_indices.Count);

				ProcessResetsNoLock(pendingModifications, changes);

				ProcessInvalidationsNoLock(pendingModifications, changes);

				ProcessAppendsNoLock(pendingModifications, changes);

				UpdateLogEntryIndicesNoLock(changes);

				stopwatch.Stop();
				if (Log.IsDebugEnabled)
				{
					int lineCount = pendingModifications.Sum(x => x.Modification.IsAppended(out var appendedSection) ? appendedSection.Count : 0);
					Log.DebugFormat("MergedLogFileIndex::Process(#{0} modifications, #{1} lines): {2}ms", pendingModifications.Count, lineCount, stopwatch.ElapsedMilliseconds);
				}

				return changes.Sections;
			}
		}

		private void ProcessResetsNoLock(IReadOnlyList<MergedLogSourceSection> pendingModifications, MergedLogSourceChanges changes)
		{
			// DO NOT CALL EXTERNAL / VIRTUAL METHODS OF ANY KIND HERE

			foreach (var pendingModification in pendingModifications)
			{
				if (pendingModification.Modification.IsReset())
				{
					var logFileIndex = GetLogFileIndex(pendingModification.LogSource);
					bool Predicate(MergedLogLineIndex x) => x.SourceId == logFileIndex;
					var firstIndex = _indices.FindIndex(Predicate);
					if (firstIndex >= 0)
					{
						_indices.RemoveAll(Predicate);
						if (_indices.Count == 0)
						{
							changes.Reset();
						}
						else
						{
							changes.RemoveFrom(firstIndex);

							var appendCount = _indices.Count - firstIndex;
							if (appendCount > 0)
							{
								changes.Append(firstIndex, appendCount);
							}
						}
					}
				}
			}
		}
			
		private void ProcessInvalidationsNoLock(IReadOnlyList<MergedLogSourceSection> pendingModifications, MergedLogSourceChanges changes)
		{
			// DO NOT CALL EXTERNAL / VIRTUAL METHODS OF ANY KIND HERE

			foreach (var pendingModification in pendingModifications)
			{
				if (pendingModification.Modification.IsRemoved(out var removedSection))
				{
					var logFileIndex = GetLogFileIndex(pendingModification.LogSource);
					bool Predicate(MergedLogLineIndex x) => x.SourceId == logFileIndex && x.SourceLineIndex >= removedSection.Index;
					var firstIndex = _indices.FindIndex(Predicate);
					if (firstIndex >= 0)
					{
						_indices.RemoveAll(Predicate);
						if (_indices.Count == 0)
						{
							changes.Reset();
						}
						else
						{
							changes.RemoveFrom(firstIndex);

							var appendCount = _indices.Count - firstIndex;
							if (appendCount > 0)
							{
								changes.Append(firstIndex, appendCount);
							}
						}
					}
				}
			}
		}

		private void ProcessAppendsNoLock(IReadOnlyList<MergedLogSourceSection> pendingModifications, MergedLogSourceChanges changes)
		{
			// DO NOT CALL EXTERNAL / VIRTUAL METHODS OF ANY KIND HERE

			var indices = CreateIndices(pendingModifications);

			var removed = false;
			var appendStartingIndex = _indices.Count;

			foreach (var index in indices)
			{
				var insertionIndex = FindInsertionIndexNoLock(index);
				if (insertionIndex < _indices.Count)
					if (!removed)
					{
						// Here's the awesome thing: We're inserting a "pre-sorted" list of indices
						// into our index buffer. Therefore, the very first index which requires an invalidation
						// is also the LOWEST POSSIBLE index which could require an invalidation and therefore
						// there can only ever be one invalidation while this method is executing.

						// We do need to take into account that even though we're invalidating a region,
						// that doesn't mean we also didn't append something previously....
						var appendCount = _indices.Count - appendStartingIndex;
						if (appendCount > 0)
							changes.Append(appendStartingIndex, appendCount);

						changes.RemoveFrom(insertionIndex);
						removed = true;
					}

				var actualIndex = index;
				actualIndex.MergedLogEntryIndex = (int) CalculateLogEntryIndexFor(insertionIndex, index);
				_indices.Insert(insertionIndex, actualIndex);
			}

			var appendCount2 = _indices.Count - appendStartingIndex;
			if (appendCount2 > 0)
				changes.Append(appendStartingIndex, appendCount2);
		}

		private void UpdateLogEntryIndicesNoLock(MergedLogSourceChanges changes)
		{
			// DO NOT CALL EXTERNAL / VIRTUAL METHODS OF ANY KIND HERE

			// We only need to re-calculate those indices if there was an invalidation or a portion
			// of this index. If there wasn't, then ProcessAppendsNoLock() already does its job as required...
			if (changes.TryGetFirstRemovedIndex(out var firstRemovedIndex))
			{
				for (int i = firstRemovedIndex.Value; i < _indices.Count; ++i)
				{
					var index = _indices[i];
					index.MergedLogEntryIndex = (int) CalculateLogEntryIndexFor(i, index);
					_indices[i] = index;
				}
			}
		}

		[Pure]
		private LogEntryIndex CalculateLogEntryIndexFor(int insertionIndex, MergedLogLineIndex index)
		{
			if (insertionIndex > 0)
			{
				var previousIndex = _indices[insertionIndex - 1];
				if (IsSameLogEntry(previousIndex, index))
				{
					return previousIndex.MergedLogEntryIndex;
				}

				return previousIndex.MergedLogEntryIndex + 1;
			}

			return 0;
		}

		private static bool IsSameLogEntry(MergedLogLineIndex lhs, MergedLogLineIndex rhs)
		{
			// Two log lines refer to the same entry if they are from the SAME source
			// AND if they have the same ORIGINAL log entry index.
			if (lhs.SourceId != rhs.SourceId)
				return false;

			return lhs.OriginalLogEntryIndex == rhs.OriginalLogEntryIndex;
		}

		private byte GetLogFileIndex(ILogSource logSource)
		{
			// DO NOT CALL EXTERNAL / VIRTUAL METHODS OF ANY KIND HERE

			if (!_logFileIndices.TryGetValue(logSource, out var index))
				throw new NotImplementedException();

			return index;
		}

		[Pure]
		public int FindInsertionIndexNoLock(MergedLogLineIndex index)
		{
			if (_indices.Count == 0)
				return 0;

			// Changes are we need to insert that index AFTER the last, so let's
			// optimize for that...
			if (index.Timestamp >= _indices[_indices.Count - 1].Timestamp)
				return _indices.Count;

			var binary = FindInsertionIndexBinary(index.Timestamp);
			/*var actual = FindInsertionIndexLinear(index.Timestamp);
			if (actual != binary)
				throw new NotImplementedException($"Mismatch detected: linear search wants to insert at {actual} and binary wants to insert at {binary} (the latter is incorrect)");
			return actual*/
			return binary;
		}

		public int FindInsertionIndexBinary(DateTime timestamp)
		{
			var lower = 0;
			var upper = _indices.Count - 1;
			while (lower <= upper)
			{
				var mid = (lower + upper) / 2;
				var currentTimestamp = _indices[mid].Timestamp;
				if (timestamp < currentTimestamp)
				{
					upper = mid - 1;
				}
				else if (timestamp >= currentTimestamp)
				{
					lower = mid + 1;
				}
				else
				{
					return mid + 1;
				}
			}

			return lower;
		}

		public int FindInsertionIndexLinear(DateTime timestamp)
		{
			for (var i = _indices.Count - 1; i >= 0; --i)
				if (timestamp >= _indices[i].Timestamp)
					return i + 1;

			return 0;
		}

		/// <summary>
		///     Retrieves the content the given modifications concern.
		///     Only retrieves the <see cref="Columns.Index" />, <see cref="Columns.LogEntryIndex" />
		///     and <see cref="Columns.Timestamp" /> columns (as these are the only ones required in order to
		///     merge stuff).
		/// </summary>
		/// <param name="pendingModifications"></param>
		/// <returns></returns>
		[Pure]
		private static IReadOnlyList<MergedLogSourceSection> GetEntries(
			IEnumerable<MergedLogSourcePendingModification> pendingModifications)
		{
			var columns = new IColumnDescriptor[]
			{
				Columns.Index,
				Columns.LogEntryIndex,
				Columns.Timestamp
			};

			var sections = new List<MergedLogSourceSection>();

			foreach (var pendingModification in pendingModifications)
			{
				var logFile = pendingModification.LogSource;
				var modification = pendingModification.Modification;
				if (modification.IsAppended(out var appendSection))
				{
					var entries = logFile.GetEntries(appendSection, columns);
					sections.Add(new MergedLogSourceSection(logFile, modification, entries));
				}
				else
				{
					sections.Add(new MergedLogSourceSection(logFile, modification));
				}
			}

			return sections;
		}

		/// <summary>
		///     Creates a sorted list of <see cref="MergedLogLineIndex" />s concerning the given modifications.
		/// </summary>
		/// <remarks>
		///     This method skips both <see cref="LogSourceModification.IsReset()" /> and <see cref="LogSourceModification.IsRemoved" />.
		/// </remarks>
		/// <param name="pendingModifications"></param>
		/// <returns></returns>
		[Pure]
		private IReadOnlyList<MergedLogLineIndex> CreateIndices(
			IReadOnlyList<MergedLogSourceSection> pendingModifications)
		{
			// DO NOT CALL EXTERNAL / VIRTUAL METHODS OF ANY KIND HERE

			var indices = new List<MergedLogLineIndex>();
			foreach (var pendingModification in pendingModifications)
			{
				var entries = pendingModification.Buffer;
				var logFileIndex = GetLogFileIndex(pendingModification.LogSource);
				indices.AddRange(CreateIndices(entries, logFileIndex));
			}

			// TODO: Create unit test which fails if we don't use a stable sort...
			return indices.OrderBy(x => x.Timestamp).ToList();
		}

		[Pure]
		private IEnumerable<MergedLogLineIndex> CreateIndices(IReadOnlyLogBuffer buffer, byte logFileIndex)
		{
			// DO NOT CALL EXTERNAL / VIRTUAL METHODS OF ANY KIND HERE

			if (buffer == null)
				return Enumerable.Empty<MergedLogLineIndex>();

			var indices = new List<MergedLogLineIndex>();

			foreach (var entry in buffer)
			{
				var index = entry.GetValue(Columns.Index);
				var entryIndex = entry.GetValue(Columns.LogEntryIndex);
				var timestamp = entry.GetValue(Columns.Timestamp);

				if (index.IsValid &&
				    entryIndex.IsValid && //< Invalid values are possible if the file has been invalidated in between it sending us a change and us having retrieved the corresponding data
				    timestamp != null) //< Not every line has a timestamp
				{
					indices.Add(new MergedLogLineIndex(index.Value,
						-1, //< We don't know the LogEntryIndex until insertion, hence we use a place-holder value here
						entryIndex.Value,
						logFileIndex,
						timestamp.Value));
				}
			}

			return indices;
		}
	}
}