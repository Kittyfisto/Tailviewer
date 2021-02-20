using System;
using System.Collections;
using System.Collections.Generic;
using Tailviewer.Core.Entries;

namespace Tailviewer.Core.Buffers
{
	/// <summary>
	///    This class represents a combined view onto multiple source <see cref="ILogBuffer"/>s, where each source contributes
	///    its set of columns to the combined one.
	/// </summary>
	/// <remarks>
	///    This buffer exposes a combined set of columns which is obtained at the moment of construction from the source buffers.
	///    If the buffers, after construction, start to expose more columns, then this view will still only expose the original
	///    set and not know about those additional columns, by design.
	/// </remarks>
	/// <remarks>
	///    This buffer exposes a combined set of log entries which is obtained at the moment of construction from the source buffers.
	///    If the buffers, after construction, start to expose more log entries, then this view will still only expose the original
	///    set and not know about those additional log entries, by design.
	/// </remarks>
	public sealed class CombinedLogBufferView
		: ILogBuffer
	{
		private readonly IReadOnlyList<IColumnDescriptor> _columns;
		private readonly IReadOnlyList<ILogBuffer> _sourceBuffers;
		private readonly IReadOnlyDictionary<IColumnDescriptor, ILogBuffer> _sourceBufferByColumn;
		private readonly int _count;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceBuffers"></param>
		public CombinedLogBufferView(IReadOnlyList<ILogBuffer> sourceBuffers)
		{
			_sourceBuffers = sourceBuffers ?? throw new ArgumentNullException(nameof(sourceBuffers));
			if (sourceBuffers.Count <= 0)
				throw new ArgumentException("A view must be constructed onto at least one source buffer");

			int? count = null;
			var columns = new List<IColumnDescriptor>();
			var sourceBufferByColumn = new Dictionary<IColumnDescriptor, ILogBuffer>(sourceBuffers.Count);
			foreach (var sourceBuffer in sourceBuffers)
			{
				var sourceCount = sourceBuffer.Count;
				if (count == null)
					count = sourceCount;
				else if (count != sourceCount)
					throw new ArgumentException($"All buffers must have the same length, but found both {count} and {sourceCount}");

				foreach (var column in sourceBuffer.Columns)
				{
					if (sourceBufferByColumn.TryGetValue(column, out var otherBuffer))
						throw new
							ArgumentException($"The buffers {sourceBuffer} and {otherBuffer} have an overlapping column, this is not supported: {column}");

					columns.Add(column);
					sourceBufferByColumn.Add(column, sourceBuffer);
				}
			}

			_columns = columns;
			_count = count.Value;
			_sourceBufferByColumn = sourceBufferByColumn;
		}

		#region Implementation of IEnumerable

		/// <inheritdoc />
		public IEnumerator<ILogEntry> GetEnumerator()
		{
			return new LogBufferEnumerator(this);
		}

		IEnumerator<IReadOnlyLogEntry> IEnumerable<IReadOnlyLogEntry>.GetEnumerator()
		{
			return new ReadOnlyLogBufferEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation of IReadOnlyCollection<out IReadOnlyLogEntry>

		/// <inheritdoc />
		public int Count => _count;

		#endregion

		#region Implementation of IReadOnlyList<out IReadOnlyLogEntry>

		/// <inheritdoc />
		public void CopyFrom<T>(IColumnDescriptor<T> column, int destinationIndex, IReadOnlyList<T> source, int sourceIndex, int length)
		{
			if (!_sourceBufferByColumn.TryGetValue(column, out var sourceBuffer))
				throw new NoSuchColumnException(column);

			sourceBuffer.CopyFrom(column, destinationIndex, source, sourceIndex, length);
		}

		/// <inheritdoc />
		public void CopyFrom(IColumnDescriptor column,
		                     int destinationIndex,
		                     ILogSource source,
		                     IReadOnlyList<LogLineIndex> sourceIndices,
		                     LogSourceQueryOptions queryOptions)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public void CopyFrom(IColumnDescriptor column, int destinationIndex, IReadOnlyLogBuffer source, IReadOnlyList<int> sourceIndices)
		{
			if (!_sourceBufferByColumn.TryGetValue(column, out var sourceBuffer))
				throw new NoSuchColumnException(column);

			sourceBuffer.CopyFrom(column, destinationIndex, source, sourceIndices);
		}

		/// <inheritdoc />
		public void FillDefault(int offset, int length)
		{
			if (offset < 0)
				throw new ArgumentOutOfRangeException(nameof(offset), offset, "The offset must be equal or greater to 0");
			if (offset + length > _count)
				throw new ArgumentException("The combined offset + count is greater than the count of this buffer");

			foreach (var source in _sourceBuffers)
			{
				source.FillDefault(offset, length);
			}
		}

		/// <inheritdoc />
		public void FillDefault(IColumnDescriptor column, int destinationIndex, int length)
		{
			if (!_sourceBufferByColumn.TryGetValue(column, out var source))
				throw new NoSuchColumnException(column);

			source.FillDefault(column, destinationIndex, length);
		}

		/// <inheritdoc />
		public void Fill<T>(IColumnDescriptor<T> column, T value, int destinationIndex, int length)
		{
			if (!_sourceBufferByColumn.TryGetValue(column, out var source))
				throw new NoSuchColumnException(column);

			source.Fill(column, value, destinationIndex, length);
		}

		/// <inheritdoc />
		public ILogEntry this[int index]
		{
			get
			{
				return new CombinedLogEntryView(this, index);
			}
		}

		IReadOnlyLogEntry IReadOnlyList<IReadOnlyLogEntry>.this[int index]
		{
			get
			{
				return new CombinedReadOnlyLogEntryView(this, index);
			}
		}

		#endregion

		#region Implementation of IReadOnlyLogBuffer

		/// <inheritdoc />
		public IReadOnlyList<IColumnDescriptor> Columns
		{
			get
			{
				return _columns;
			}
		}

		/// <inheritdoc />
		public bool Contains(IColumnDescriptor column)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public void CopyTo<T>(IColumnDescriptor<T> column, int sourceIndex, T[] destination, int destinationIndex, int length)
		{
			if (!_sourceBufferByColumn.TryGetValue(column, out var source))
				throw new ColumnNotRetrievedException(column);

			source.CopyTo(column, sourceIndex, destination, destinationIndex, length);
		}

		/// <inheritdoc />
		public void CopyTo<T>(IColumnDescriptor<T> column, IReadOnlyList<int> sourceIndices, T[] destination, int destinationIndex)
		{
			if (!_sourceBufferByColumn.TryGetValue(column, out var source))
				throw new ColumnNotRetrievedException(column);

			source.CopyTo(column, sourceIndices, destination, destinationIndex);
		}

		/// <inheritdoc />
		public void CopyTo<T>(IColumnDescriptor<T> column, IReadOnlyList<int> sourceIndices, IList<T> destination, int destinationIndex)
		{
			throw new System.NotImplementedException();
		}

		#endregion

		sealed class CombinedReadOnlyLogEntryView
			: AbstractReadOnlyLogEntry
		{
			private readonly CombinedLogBufferView _buffer;
			private readonly int _index;

			public CombinedReadOnlyLogEntryView(CombinedLogBufferView buffer, int index)
			{
				_buffer = buffer;
				_index = index;
			}

			#region Overrides of AbstractReadOnlyLogEntry

			public override T GetValue<T>(IColumnDescriptor<T> column)
			{
				if (!_buffer._sourceBufferByColumn.TryGetValue(column, out var source))
					throw new ColumnNotRetrievedException(column);

				return source[_index].GetValue(column);
			}

			public override bool TryGetValue<T>(IColumnDescriptor<T> column, out T value)
			{
				if (!_buffer._sourceBufferByColumn.TryGetValue(column, out var source))
				{
					value = column.DefaultValue;
					return false;
				}

				return source[_index].TryGetValue(column, out value);
			}

			public override object GetValue(IColumnDescriptor column)
			{
				if (!_buffer._sourceBufferByColumn.TryGetValue(column, out var source))
					throw new ColumnNotRetrievedException(column);

				return source[_index].GetValue(column);
			}

			public override bool TryGetValue(IColumnDescriptor column, out object value)
			{
				if (!_buffer._sourceBufferByColumn.TryGetValue(column, out var source))
				{
					value = column.DefaultValue;
					return false;
				}

				return source[_index].TryGetValue(column, out value);
			}

			public override IReadOnlyList<IColumnDescriptor> Columns => _buffer._columns;

			#endregion
		}

		sealed class CombinedLogEntryView
			: AbstractLogEntry
		{
			private readonly CombinedLogBufferView _buffer;
			private readonly int _index;

			public CombinedLogEntryView(CombinedLogBufferView buffer, int index)
			{
				_buffer = buffer;
				_index = index;
			}

			#region Overrides of AbstractLogEntry
			
			public override T GetValue<T>(IColumnDescriptor<T> column)
			{
				if (!_buffer._sourceBufferByColumn.TryGetValue(column, out var source))
					throw new ColumnNotRetrievedException(column);

				return source[_index].GetValue(column);
			}

			public override bool TryGetValue<T>(IColumnDescriptor<T> column, out T value)
			{
				if (!_buffer._sourceBufferByColumn.TryGetValue(column, out var source))
				{
					value = column.DefaultValue;
					return false;
				}

				return source[_index].TryGetValue(column, out value);
			}

			public override object GetValue(IColumnDescriptor column)
			{
				if (!_buffer._sourceBufferByColumn.TryGetValue(column, out var source))
					throw new ColumnNotRetrievedException(column);

				return source[_index].GetValue(column);
			}

			public override bool TryGetValue(IColumnDescriptor column, out object value)
			{
				if (!_buffer._sourceBufferByColumn.TryGetValue(column, out var source))
				{
					value = column.DefaultValue;
					return false;
				}

				return source[_index].TryGetValue(column, out value);
			}

			public override IReadOnlyList<IColumnDescriptor> Columns => _buffer._columns;

			public override void SetValue(IColumnDescriptor column, object value)
			{
				if (!_buffer._sourceBufferByColumn.TryGetValue(column, out var source))
					throw new ColumnNotRetrievedException(column);

				source[_index].SetValue(column, value);
			}

			public override void SetValue<T>(IColumnDescriptor<T> column, T value)
			{
				if (!_buffer._sourceBufferByColumn.TryGetValue(column, out var source))
					throw new ColumnNotRetrievedException(column);

				source[_index].SetValue(column, value);
			}

			#endregion
		}
	}
}