using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Responsible for keeping a certain number of log entries in memory.
	/// </summary>
	internal sealed class LogEntryCache
	{
		private static readonly ILogFileColumn<DateTime> LastAccessed =
			new WellKnownLogFileColumn<DateTime>("LastAccessed", DateTime.MinValue);

		private readonly LogEntryList _buffer;
		private readonly Dictionary<LogLineIndex, int> _bufferIndexByLineIndex;
		private readonly int _maxSize;
		private readonly KeyValuePair<int, DateTime>[] _vacuumBuffer1;
		private readonly int[] _vacuumBuffer2;

		public LogEntryCache(params ILogFileColumn[] columns)
		{
			_maxSize = 10000;
			_vacuumBuffer1 = new KeyValuePair<int, DateTime>[_maxSize];
			_vacuumBuffer2 = new int[_maxSize];
			_buffer = new LogEntryList(columns.Concat(new[] {LastAccessed}));
			_bufferIndexByLineIndex = new Dictionary<LogLineIndex, int>();
		}

		public void Add(IReadOnlyLogEntry logEntry)
		{
			var bufferIndex = _buffer.Count;
			_buffer.Add(logEntry);
			_bufferIndexByLineIndex[logEntry.Index] = bufferIndex;
		}

		public bool TryCopyTo(IReadOnlyList<LogLineIndex> indices, ILogEntries buffer, int destinationIndex)
		{
			if (!TryTranslateToBufferIndices(indices, out var bufferIndices))
				return false;

			_buffer.CopyTo(bufferIndices, buffer, destinationIndex);
			return true;
		}

		public bool TryCopyTo<T>(IReadOnlyList<LogLineIndex> indices, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			if (!TryTranslateToBufferIndices(indices, out var bufferIndices))
				return false;

			_buffer.CopyTo(column, bufferIndices, buffer, destinationIndex);
			return true;
		}

		[Pure]
		private bool TryTranslateToBufferIndices(IReadOnlyList<LogLineIndex> indices,
		                                         out int[] bufferIndices)
		{
			bufferIndices = new int[indices.Count];
			for(int i = 0; i < indices.Count; ++i)
			{
				if (!_bufferIndexByLineIndex.TryGetValue(indices[i], out var bufferIndex))
					return false;

				bufferIndices[i] = bufferIndex;
			}

			return true;
		}

		public void RemoveRange(int index, int count)
		{
			_buffer.RemoveRange(index, count);
		}

		public void Clear()
		{
			_buffer.Clear();
			_bufferIndexByLineIndex.Clear();
		}

		public void Vacuum()
		{
			var tooMuch = _buffer.Count - _maxSize;
			if (tooMuch > 0)
			{
				// Let's remove half of its oldest entries...
				var toRemove = _maxSize / 2 + tooMuch;

				FindOldestEntries();
				SortOldestByLineIndex(toRemove);
				RemoveOldestEntries(toRemove);
			}
		}

		private void FindOldestEntries()
		{
			for (var i = 0; i < _buffer.Count; ++i)
			{
				var entry = _buffer[i];
				_vacuumBuffer1[i] = new KeyValuePair<int, DateTime>(i, entry.GetValue(LastAccessed));
			}

			Array.Sort(_vacuumBuffer1, index: 0, _buffer.Count, new SortByDateAccessedAscending());
		}

		private void SortOldestByLineIndex(int toRemove)
		{
			for (var i = 0; i < toRemove; ++i) _vacuumBuffer2[i] = _vacuumBuffer1[i].Key;

			Array.Sort(_vacuumBuffer2, index: 0, toRemove, new SortByLineIndexDescending());
		}

		private void RemoveOldestEntries(int toRemove)
		{
			for (var i = 0; i < toRemove; ++i)
			{
				var bufferIndex = _vacuumBuffer2[i];
				_buffer.RemoveAt(bufferIndex);
			}
		}

		private sealed class SortByDateAccessedAscending
			: IComparer<KeyValuePair<int, DateTime>>
		{
			#region Implementation of IComparer<in KeyValuePair<int,DateTime>>

			public int Compare(KeyValuePair<int, DateTime> x, KeyValuePair<int, DateTime> y)
			{
				return x.Value.CompareTo(y.Value);
			}

			#endregion
		}

		private sealed class SortByLineIndexDescending
			: IComparer<int>
		{
			#region Implementation of IComparer<in KeyValuePair<int,DateTime>>

			public int Compare(int x, int y)
			{
				return 1 - x.CompareTo(y);
			}

			#endregion
		}
	}
}