using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     A fixed-size buffer which provides read/write access to <see cref="IReadOnlyLogEntry" />s.
	/// </summary>
	public sealed class LogBufferArray
		: ILogBuffer
	{
		private readonly int _length;
		private readonly IReadOnlyList<IColumnDescriptor> _columns;
		private readonly IReadOnlyDictionary<IColumnDescriptor, IColumnData> _dataByColumn;

		/// <summary>
		/// </summary>
		/// <param name="length"></param>
		/// <param name="columns"></param>
		public LogBufferArray(int length, IEnumerable<IColumnDescriptor> columns)
		{
			if (columns == null)
				throw new ArgumentNullException(nameof(columns));

			_length = length;
			_columns = columns.ToList();
			var dataByColumn = new Dictionary<IColumnDescriptor, IColumnData>(_columns.Count);
			foreach (var column in _columns)
			{
				dataByColumn.Add(column, CreateColumnData(column, length));
			}
			_dataByColumn = dataByColumn;
		}

		/// <summary>
		/// </summary>
		/// <param name="length"></param>
		/// <param name="columns"></param>
		public LogBufferArray(int length, params IColumnDescriptor[] columns)
			: this(length, (IEnumerable<IColumnDescriptor>)columns)
		{}

		/// <inheritdoc />
		public IReadOnlyList<IColumnDescriptor> Columns => _columns;

		/// <inheritdoc />
		public bool Contains(IColumnDescriptor column)
		{
			return _dataByColumn.ContainsKey(column);
		}

		/// <inheritdoc />
		public void CopyTo<T>(IColumnDescriptor<T> column, int sourceIndex, T[] destination, int destinationIndex, int length)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			if (_dataByColumn.TryGetValue(column, out var columnData))
			{
				((ColumnData<T>)columnData).CopyTo(sourceIndex, destination, destinationIndex, length);
			}
			else
			{
				throw new NoSuchColumnException(column);
			}
		}

		/// <inheritdoc />
		public void CopyTo<T>(IColumnDescriptor<T> column, IReadOnlyList<int> sourceIndices, T[] destination, int destinationIndex)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			if (_dataByColumn.TryGetValue(column, out var columnData))
			{
				((ColumnData<T>)columnData).CopyTo(sourceIndices, destination, destinationIndex);
			}
			else
			{
				throw new NoSuchColumnException(column);
			}
		}

		/// <inheritdoc />
		public void CopyTo<T>(IColumnDescriptor<T> column, IReadOnlyList<int> sourceIndices, IList<T> destination, int destinationIndex)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			if (_dataByColumn.TryGetValue(column, out var columnData))
			{
				((ColumnData<T>)columnData).CopyTo(sourceIndices, destination, destinationIndex);
			}
			else
			{
				throw new NoSuchColumnException(column);
			}
		}

		/// <inheritdoc />
		public void CopyFrom<T>(IColumnDescriptor<T> column, int destinationIndex, IReadOnlyList<T> source, int sourceIndex, int length)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			if (_dataByColumn.TryGetValue(column, out var columnData))
			{
				((ColumnData<T>)columnData).CopyFrom(destinationIndex, source, sourceIndex, length);
			}
			else
			{
				throw new NoSuchColumnException(column);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="column"></param>
		/// <param name="destinationIndex"></param>
		/// <param name="source"></param>
		/// <param name="sourceIndices"></param>
		public void CopyFrom<T>(IColumnDescriptor<T> column, int destinationIndex, IReadOnlyList<T> source, IReadOnlyList<int> sourceIndices)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			if (_dataByColumn.TryGetValue(column, out var columnData))
			{
				((ColumnData<T>)columnData).CopyFrom(destinationIndex, source, sourceIndices);
			}
			else
			{
				throw new NoSuchColumnException(column);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="column"></param>
		/// <param name="destinationIndex"></param>
		/// <param name="source"></param>
		/// <param name="section"></param>
		/// <param name="queryOptions"></param>
		public void CopyFrom(IColumnDescriptor column, int destinationIndex, ILogSource source, LogSourceSection section, LogSourceQueryOptions queryOptions)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			if (!_dataByColumn.TryGetValue(column, out var columnData))
				throw new NoSuchColumnException(column);

			columnData.CopyFrom(destinationIndex, source, section, queryOptions);
		}

		/// <inheritdoc />
		public void CopyFrom(IColumnDescriptor column, int destinationIndex, ILogSource source, IReadOnlyList<LogLineIndex> sourceIndices, LogSourceQueryOptions queryOptions)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			if (!_dataByColumn.TryGetValue(column, out var columnData))
				throw new NoSuchColumnException(column);

			columnData.CopyFrom(destinationIndex, source, sourceIndices, queryOptions);
		}

		/// <inheritdoc />
		public void CopyFrom(IColumnDescriptor column,
		                     int destinationIndex,
		                     IReadOnlyLogBuffer source,
		                     IReadOnlyList<int> sourceIndices)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			if (!_dataByColumn.TryGetValue(column, out var columnData))
				throw new NoSuchColumnException(column);

			columnData.CopyFrom(destinationIndex, source, sourceIndices);
		}

		/// <inheritdoc />
		public void FillDefault(int offset, int length)
		{
			foreach (var column in _dataByColumn.Values)
			{
				column.FillDefault(offset, length);
			}
		}

		/// <inheritdoc />
		public void FillDefault(IColumnDescriptor column, int destinationIndex, int length)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			if (!_dataByColumn.TryGetValue(column, out var columnData))
				throw new NoSuchColumnException(column);

			columnData.FillDefault(destinationIndex, length);
		}

		/// <inheritdoc />
		public void Fill<T>(IColumnDescriptor<T> column, T value, int destinationIndex, int length)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			if (!_dataByColumn.TryGetValue(column, out var columnData))
				throw new NoSuchColumnException(column);

			((ColumnData<T>)columnData).Fill(value, destinationIndex, length);
		}

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

		/// <inheritdoc />
		public int Count => _length;

		/// <inheritdoc />
		public ILogEntry this[int index]
		{
			get
			{
				if (index < 0)
					throw new IndexOutOfRangeException();
				if (index >= Count)
					throw new IndexOutOfRangeException();

				return new LogEntryAccessor(this, index);
			}
		}

		/// <summary>
		/// Returns an enumeration which iterates over all values in the given column.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="column"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">When this buffer doesn't hold the given column</exception>
		/// <exception cref="ArgumentNullException">When <paramref name="column"/> is null</exception>
		public IEnumerable<T> Column<T>(IColumnDescriptor<T> column)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			if (!_dataByColumn.TryGetValue(column, out var data))
				throw new ArgumentException(string.Format("No such column: {0}", column));

			return ((ColumnData<T>) data);
		}

		/// <summary>
		/// Tests if the specified region of the specified column contains one or more <see cref="IColumnDescriptor.DefaultValue"/> values.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="range"></param>
		/// <returns>When every value of the specified section contains default values, false otherwise</returns>
		[Pure]
		public bool ContainsAnyDefault(IColumnDescriptor column, Int32Range range)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			if (!_dataByColumn.TryGetValue(column, out var data))
				throw new ArgumentException(string.Format("No such column: {0}", column));

			return data.ContainsAnyDefault(range);
		}

		/// <summary>
		/// Tests if the specified region of the specified column contains one or more <see cref="IColumnDescriptor.DefaultValue"/> values.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="value"></param>
		/// <param name="range"></param>
		/// <returns>When every value of the specified section contains default values, false otherwise</returns>
		[Pure]
		public bool ContainsAny<T>(IColumnDescriptor<T> column, T value, Int32Range range)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			if (!_dataByColumn.TryGetValue(column, out var data))
				throw new ArgumentException(string.Format("No such column: {0}", column));

			return ((ColumnData<T>)data).ContainsAny(value, range);
		}

		IReadOnlyLogEntry IReadOnlyList<IReadOnlyLogEntry>.this[int index] => this[index];

		private sealed class LogEntryAccessor
			: AbstractLogEntry
		{
			private readonly LogBufferArray _array;
			private readonly int _index;

			public LogEntryAccessor(LogBufferArray array, int index)
			{
				_array = array ?? throw new ArgumentNullException(nameof(array));
				_index = index;
			}

			public override T GetValue<T>(IColumnDescriptor<T> column)
			{
				if (!TryGetValue(column, out var value))
					throw new ColumnNotRetrievedException(column);

				return value;
			}

			public override bool TryGetValue<T>(IColumnDescriptor<T> column, out T value)
			{
				if (!_array._dataByColumn.TryGetValue(column, out var data))
				{
					value = column.DefaultValue;
					return false;
				}

				value = ((ColumnData<T>)data)[_index];
				return true;
			}

			public override object GetValue(IColumnDescriptor column)
			{
				if (!TryGetValue(column, out var value))
					throw new ColumnNotRetrievedException(column);

				return value;
			}

			public override bool TryGetValue(IColumnDescriptor column, out object value)
			{
				if (!_array._dataByColumn.TryGetValue(column, out var data))
				{
					value = column.DefaultValue;
					return false;
				}

				value = data[_index];
				return true;
			}

			public override IReadOnlyList<IColumnDescriptor> Columns => _array._columns;

			public override void SetValue(IColumnDescriptor column, object value)
			{
				if (!_array._dataByColumn.TryGetValue(column, out var data))
					throw new ColumnNotRetrievedException(column);

				data[_index] = value;
			}

			public override void SetValue<T>(IColumnDescriptor<T> column, T value)
			{
				if (!_array._dataByColumn.TryGetValue(column, out var data))
					throw new ColumnNotRetrievedException(column);

				((ColumnData<T>)data)[_index] = value;
			}

			public override void CopyFrom(IReadOnlyLogEntry logEntry)
			{
				foreach (var column in logEntry.Columns)
				{
					if (_array._dataByColumn.TryGetValue(column, out var columnData))
					{
						columnData.CopyFrom(_index, logEntry);
					}
				}
			}

		}

		private static IColumnData CreateColumnData(IColumnDescriptor column, int length)
		{
			dynamic tmp = column;
			return CreateColumnData(tmp, length);
		}

		private static IColumnData CreateColumnData<T>(IColumnDescriptor<T> column, int length)
		{
			return new ColumnData<T>(column, length);
		}

		interface IColumnData
		{
			object this[int index] { get; set; }
			void CopyFrom(int destinationIndex, ILogSource source, LogSourceSection section, LogSourceQueryOptions queryOptions);
			void CopyFrom(int destinationIndex, ILogSource source, IReadOnlyList<LogLineIndex> indices, LogSourceQueryOptions queryOptions);
			void CopyFrom(int destinationIndex, IReadOnlyLogBuffer source, IReadOnlyList<int> sourceIndices);
			void FillDefault(int destinationIndex, int length);
			void CopyFrom(int destinationIndex, IReadOnlyLogEntry logEntry);
			bool ContainsAnyDefault(Int32Range range);
		}

		sealed class ColumnData<T>
			: IColumnData
			, IEnumerable<T>
		{
			private readonly IColumnDescriptor<T> _column;
			private readonly T[] _data;

			public ColumnData(IColumnDescriptor<T> column, int length)
			{
				_column = column ?? throw new ArgumentNullException(nameof(column));
				_data = new T[length];
				_data.Fill(column.DefaultValue);
			}

			public T this[int index]
			{
				get { return _data[index]; }
				set { _data[index] = value; }
			}

			object IColumnData.this[int index]
			{
				get { return _data[index]; }
				set { _data[index] = (T) value; }
			}

			public void CopyFrom(int destIndex, IReadOnlyList<T> source, int sourceIndex, int length)
			{
				if (source is T[] sourceArray)
				{
					Array.Copy(sourceArray, sourceIndex, _data, destIndex, length);
				}
				else
				{
					for (int i = 0; i < length; ++i)
					{
						_data[destIndex + i] = source[sourceIndex + i];
					}
				}
			}

			public void CopyFrom(int destinationIndex, IReadOnlyList<T> source, IReadOnlyList<int> sourceIndices)
			{
				for (int i = 0; i < sourceIndices.Count; ++i)
				{
					_data[destinationIndex + i] = source[i];
				}
			}

			public void CopyFrom(int destinationIndex, ILogSource source, LogSourceSection section, LogSourceQueryOptions queryOptions)
			{
				source.GetColumn(section, _column, _data, destinationIndex, queryOptions);
			}

			public void CopyFrom(int destinationIndex, ILogSource source, IReadOnlyList<LogLineIndex> indices, LogSourceQueryOptions queryOptions)
			{
				source.GetColumn(indices, _column, _data, destinationIndex, queryOptions);
			}

			public void CopyFrom(int destinationIndex, IReadOnlyLogBuffer source, IReadOnlyList<int> sourceIndices)
			{
				source.CopyTo(_column, sourceIndices, _data, destinationIndex);
			}

			public void CopyTo(int sourceIndex, T[] destination, int destinationIndex, int length)
			{
				if (destination == null)
					throw new ArgumentNullException(nameof(destination));
				if (destinationIndex + length > destination.Length)
					throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

				// As usual, we allow access to invalid regions and return the default value
				// for that column.
				if (sourceIndex < 0)
				{
					destination.Fill(_column.DefaultValue, destinationIndex, -sourceIndex);
					length += sourceIndex;
					destinationIndex -= sourceIndex;
					sourceIndex = 0;
				}

				var tooMany = sourceIndex + length - _data.Length;
				if (tooMany > 0)
				{
					destination.Fill(_column.DefaultValue, destinationIndex + length - tooMany, tooMany);
					length -= tooMany;
				}

				Array.Copy(_data, sourceIndex, destination, destinationIndex, length);
			}

			public void CopyTo(IReadOnlyList<int> sourceIndices, T[] destination, int destinationIndex)
			{
				if (sourceIndices == null)
					throw new ArgumentNullException(nameof(sourceIndices));
				if (destination == null)
					throw new ArgumentNullException(nameof(destination));
				if (destinationIndex + sourceIndices.Count > destination.Length)
					throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

				for (int i = 0; i < sourceIndices.Count; ++i)
				{
					var sourceIndex = sourceIndices[i];
					if (sourceIndex >= 0 && sourceIndex < _data.Length)
					{
						destination[destinationIndex + i] = _data[sourceIndex];
					}
					else
					{
						destination[destinationIndex + i] = _column.DefaultValue;
					}
				}
			}

			public void CopyTo(IReadOnlyList<int> sourceIndices, IList<T> destination, int destinationIndex)
			{
				if (sourceIndices == null)
					throw new ArgumentNullException(nameof(sourceIndices));
				if (destination == null)
					throw new ArgumentNullException(nameof(destination));
				if (destinationIndex + sourceIndices.Count > destination.Count)
					throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

				for (int i = 0; i < sourceIndices.Count; ++i)
				{
					var sourceIndex = sourceIndices[i];
					if (sourceIndex >= 0 && sourceIndex < _data.Length)
					{
						destination[destinationIndex + i] = _data[sourceIndex];
					}
					else
					{
						destination[destinationIndex + i] = _column.DefaultValue;
					}
				}
			}

			public void FillDefault(int destinationIndex, int length)
			{
				Fill(_column.DefaultValue, destinationIndex, length);
			}

			public void Fill(T value, int destinationIndex, int length)
			{
				_data.Fill(value, destinationIndex, length);
			}

			public void CopyFrom(int destinationIndex, IReadOnlyLogEntry logEntry)
			{
				_data[destinationIndex] = logEntry.GetValue(_column);
			}

			[Pure]
			public bool ContainsAnyDefault(Int32Range range)
			{
				return ContainsAny(_column.DefaultValue, range);
			}

			[Pure]
			public bool ContainsAny(T value, Int32Range range)
			{
				for(int i = 0; i < range.Count; ++i)
				{
					// TODO: Boxing/Unboxing is going to kill us here, we need a generic equality comparer here
					if (Equals(_data[range.Offset + i], value))
						return true;
				}

				return false;
			}

			public IEnumerator<T> GetEnumerator()
			{
				return ((IEnumerable<T>)_data).GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}