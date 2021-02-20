using System;
using System.Collections;
using System.Collections.Generic;
using Tailviewer.Core.Entries;

namespace Tailviewer.Core.Buffers
{
	/// <summary>
	///    Provides a view onto a buffer of a single column.
	/// </summary>
	public sealed class SingleColumnLogBufferView<T>
		: ILogBuffer
	{
		private readonly IColumnDescriptor<T> _column;
		private readonly IReadOnlyList<IColumnDescriptor> _columns;
		private readonly T[] _buffer;
		private readonly int _offset;
		private readonly int _count;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="column"></param>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		public SingleColumnLogBufferView(IColumnDescriptor<T> column, T[] buffer, int offset, int count)
		{
			_column = column ?? throw new ArgumentNullException(nameof(column));
			_columns = new[] {column};
			_buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));

			if (offset < 0)
				throw new ArgumentOutOfRangeException(nameof(offset), offset,
				                                      "The offset into the buffer must not be negative");
			if (buffer.Length < offset + count)
				throw new
					ArgumentException($"The count {count} and offset {offset} given are greater than the buffer's length {buffer.Length}!");

			_offset = offset;
			_count = count;
		}

		#region Implementation of IEnumerable

		IEnumerator<ILogEntry> IEnumerable<ILogEntry>.GetEnumerator()
		{
			throw new System.NotImplementedException();
		}

		IEnumerator<IReadOnlyLogEntry> IEnumerable<IReadOnlyLogEntry>.GetEnumerator()
		{
			throw new System.NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new System.NotImplementedException();
		}

		#endregion

		#region Implementation of IReadOnlyCollection<out IReadOnlyLogEntry>

		/// <inheritdoc />
		public int Count => _count;

		#endregion

		#region Implementation of IReadOnlyList<out IReadOnlyLogEntry>

		/// <inheritdoc />
		public void CopyFrom<TColumn>(IColumnDescriptor<TColumn> column, int destinationIndex, IReadOnlyList<TColumn> source, int sourceIndex, int length)
		{
			if (!Equals(column, _column))
				throw new NoSuchColumnException(column);

			if (source is T[] sourceArray)
			{
				Array.Copy(sourceArray, sourceIndex, _buffer, _offset + destinationIndex, length);
			}
			else
			{
				for (int i = 0; i < length; ++i)
				{
					// TODO: Can we get rid of this boxing/unboxing of value types here? This sucks
					_buffer[_offset + destinationIndex + i] = (T)(object)source[sourceIndex + i];
				}
			}
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
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			if (!Equals(column, _column))
				throw new NoSuchColumnException(column);

			// This view may be constructed with a specific offset into the _buffer and therefore, when
			// instructing the other buffer to copy its data into ours, we have to take into account
			// both the internal offset as well as the destination index into the visible portion of this
			// buffer upon construction.
			source.CopyTo(_column, sourceIndices, _buffer, _offset + destinationIndex);
		}

		/// <inheritdoc />
		public void FillDefault(int offset, int length)
		{
			_buffer.Fill(_column.DefaultValue, _offset + offset, length);
		}

		/// <inheritdoc />
		public void FillDefault(IColumnDescriptor column, int destinationIndex, int length)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public void Fill<T1>(IColumnDescriptor<T1> column, T1 value, int destinationIndex, int length)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public ILogEntry this[int index]
		{
			get
			{
				return new SingleColumnLogEntry(this, index);
			}
		}

		IReadOnlyLogEntry IReadOnlyList<IReadOnlyLogEntry>.this[int index]
		{
			get
			{
				return new SingleColumnReadOnlyLogEntry(this, index);
			}
		}

		#endregion

		#region Implementation of IReadOnlyLogEntries

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
			return Equals(_column, column);
		}

		/// <inheritdoc />
		public void CopyTo<TColumn>(IColumnDescriptor<TColumn> column, int sourceIndex, TColumn[] destination, int destinationIndex, int length)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public void CopyTo<TColumn>(IColumnDescriptor<TColumn> column, IReadOnlyList<int> sourceIndices, TColumn[] destination, int destinationIndex)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public void CopyTo<TColumn>(IColumnDescriptor<TColumn> column, IReadOnlyList<int> sourceIndices, IList<TColumn> destination, int destinationIndex)
		{
			throw new System.NotImplementedException();
		}

		#endregion

		sealed class SingleColumnLogEntry
			: AbstractLogEntry
		{
			private readonly SingleColumnLogBufferView<T> _buffer;
			private readonly int _index;

			public SingleColumnLogEntry(SingleColumnLogBufferView<T> buffer, int index)
			{
				_buffer = buffer;
				_index = index;
			}

			#region Overrides of AbstractLogEntry

			public override T1 GetValue<T1>(IColumnDescriptor<T1> column)
			{
				if (!Equals(_buffer._column, column))
					throw new ColumnNotRetrievedException(column);

				// TODO: What can we do about boxing/unboxing of value types here? This sucks
				return (T1)(object)_buffer._buffer[_buffer._offset + _index];
			}

			public override bool TryGetValue<T1>(IColumnDescriptor<T1> column, out T1 value)
			{
				if (!Equals(_buffer._column, column))
				{
					value = column.DefaultValue;
					return false;
				}

				// TODO: What can we do about boxing/unboxing of value types here? This sucks
				value = (T1)(object)_buffer._buffer[_buffer._offset + _index];
				return true;
			}

			public override object GetValue(IColumnDescriptor column)
			{
				if (!Equals(_buffer._column, column))
					throw new ColumnNotRetrievedException(column);

				return _buffer._buffer[_buffer._offset + _index];
			}

			public override bool TryGetValue(IColumnDescriptor column, out object value)
			{
				if (!Equals(_buffer._column, column))
				{
					value = column.DefaultValue;
					return false;
				}

				value = _buffer._buffer[_buffer._offset + _index];
				return true;
			}

			public override IReadOnlyList<IColumnDescriptor> Columns => _buffer._columns;

			public override void SetValue(IColumnDescriptor column, object value)
			{
				if (!Equals(_buffer._column, column))
					throw new NoSuchColumnException(column);

				_buffer._buffer[_buffer._offset + _index] = (T) value;
			}

			public override void SetValue<T1>(IColumnDescriptor<T1> column, T1 value)
			{
				if (!Equals(_buffer._column, column))
					throw new ColumnNotRetrievedException(column);

				// TODO: What can we do about boxing/unboxing of value types here? This sucks
				_buffer._buffer[_buffer._offset + _index] = (T)(object)value;
			}

			#endregion
		}

		sealed class SingleColumnReadOnlyLogEntry
			: AbstractReadOnlyLogEntry
		{
			private readonly SingleColumnLogBufferView<T> _buffer;
			private readonly int _index;

			public SingleColumnReadOnlyLogEntry(SingleColumnLogBufferView<T> buffer, int index)
			{
				_buffer = buffer;
				_index = index;
			}

			#region Overrides of AbstractReadOnlyLogEntry

			public override T1 GetValue<T1>(IColumnDescriptor<T1> column)
			{
				if (!Equals(_buffer._column, column))
					throw new ColumnNotRetrievedException(column);

				// TODO: What can we do about boxing/unboxing of value types here? This sucks
				return (T1)(object)_buffer._buffer[_buffer._offset + _index];
			}

			public override bool TryGetValue<T1>(IColumnDescriptor<T1> column, out T1 value)
			{
				if (!Equals(_buffer._column, column))
				{
					value = column.DefaultValue;
					return false;
				}

				// TODO: What can we do about boxing/unboxing of value types here? This sucks
				value = (T1)(object)_buffer._buffer[_buffer._offset + _index];
				return true;
			}

			public override object GetValue(IColumnDescriptor column)
			{
				if (!Equals(_buffer._column, column))
					throw new ColumnNotRetrievedException(column);

				return _buffer._buffer[_buffer._offset + _index];
			}

			public override bool TryGetValue(IColumnDescriptor column, out object value)
			{
				if (!Equals(_buffer._column, column))
				{
					value = column.DefaultValue;
					return false;
				}

				value = _buffer._buffer[_buffer._offset + _index];
				return true;
			}

			public override IReadOnlyList<IColumnDescriptor> Columns => _buffer._columns;

			#endregion
		}
	}
}
