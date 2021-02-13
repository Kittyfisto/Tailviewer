using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     A fixed-size buffer which provides read/write access to <see cref="IReadOnlyLogEntry" />s.
	/// </summary>
	public sealed class LogEntryArray
		: ILogEntries
	{
		private readonly int _length;
		private readonly IReadOnlyList<ILogFileColumnDescriptor> _columns;
		private readonly IReadOnlyDictionary<ILogFileColumnDescriptor, IColumnData> _dataByColumn;

		/// <summary>
		/// </summary>
		/// <param name="length"></param>
		/// <param name="columns"></param>
		public LogEntryArray(int length, IEnumerable<ILogFileColumnDescriptor> columns)
		{
			if (columns == null)
				throw new ArgumentNullException(nameof(columns));

			_length = length;
			_columns = columns.ToList();
			var dataByColumn = new Dictionary<ILogFileColumnDescriptor, IColumnData>(_columns.Count);
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
		public LogEntryArray(int length, params ILogFileColumnDescriptor[] columns)
			: this(length, (IEnumerable<ILogFileColumnDescriptor>)columns)
		{}

		/// <inheritdoc />
		public IReadOnlyList<ILogFileColumnDescriptor> Columns => _columns;

		/// <inheritdoc />
		public bool Contains(ILogFileColumnDescriptor column)
		{
			return _dataByColumn.ContainsKey(column);
		}

		/// <inheritdoc />
		public void CopyTo<T>(ILogFileColumnDescriptor<T> column, int sourceIndex, T[] destination, int destinationIndex, int length)
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
		public void CopyTo<T>(ILogFileColumnDescriptor<T> column, IReadOnlyList<int> sourceIndices, T[] destination, int destinationIndex)
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
		public void CopyTo<T>(ILogFileColumnDescriptor<T> column, IReadOnlyList<int> sourceIndices, IList<T> destination, int destinationIndex)
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
		public void CopyFrom<T>(ILogFileColumnDescriptor<T> column, int destinationIndex, T[] source, int sourceIndex, int length)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			IColumnData columnData;
			if (_dataByColumn.TryGetValue(column, out columnData))
			{
				((ColumnData<T>)columnData).CopyFrom(destinationIndex, source, sourceIndex, length);
			}
			else
			{
				throw new NoSuchColumnException(column);
			}
		}

		/// <inheritdoc />
		public void CopyFrom<T>(ILogFileColumnDescriptor<T> column, int destinationIndex, IReadOnlyList<T> source, IReadOnlyList<int> sourceIndices)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			IColumnData columnData;
			if (_dataByColumn.TryGetValue(column, out columnData))
			{
				((ColumnData<T>)columnData).CopyFrom(destinationIndex, source, sourceIndices);
			}
			else
			{
				throw new NoSuchColumnException(column);
			}
		}

		/// <inheritdoc />
		public void CopyFrom(ILogFileColumnDescriptor column, int destinationIndex, ILogFile source, LogFileSection section)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			IColumnData columnData;
			if (!_dataByColumn.TryGetValue(column, out columnData))
				throw new NoSuchColumnException(column);

			columnData.CopyFrom(destinationIndex, source, section);
		}

		/// <inheritdoc />
		public void CopyFrom(ILogFileColumnDescriptor column, int destinationIndex, ILogFile source, IReadOnlyList<LogLineIndex> sourceIndices)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			IColumnData columnData;
			if (!_dataByColumn.TryGetValue(column, out columnData))
				throw new NoSuchColumnException(column);

			columnData.CopyFrom(destinationIndex, source, sourceIndices);
		}

		/// <inheritdoc />
		public void CopyFrom(ILogFileColumnDescriptor column,
		                     int destinationIndex,
		                     IReadOnlyLogEntries source,
		                     IReadOnlyList<int> sourceIndices)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			IColumnData columnData;
			if (!_dataByColumn.TryGetValue(column, out columnData))
				throw new NoSuchColumnException(column);

			columnData.CopyFrom(destinationIndex, source, sourceIndices);
		}

		/// <inheritdoc />
		public void FillDefault(int destinationIndex, int length)
		{
			foreach (var column in _dataByColumn.Values)
			{
				column.FillDefault(destinationIndex, length);
			}
		}

		/// <inheritdoc />
		public void FillDefault(ILogFileColumnDescriptor column, int destinationIndex, int length)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			IColumnData columnData;
			if (!_dataByColumn.TryGetValue(column, out columnData))
				throw new NoSuchColumnException(column);

			columnData.FillDefault(destinationIndex, length);
		}

		/// <inheritdoc />
		public IEnumerator<ILogEntry> GetEnumerator()
		{
			return new LogEntriesEnumerator(this);
		}

		IEnumerator<IReadOnlyLogEntry> IEnumerable<IReadOnlyLogEntry>.GetEnumerator()
		{
			return new ReadOnlyLogEntriesEnumerator(this);
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
		public IEnumerable<T> Column<T>(ILogFileColumnDescriptor<T> column)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			if (!_dataByColumn.TryGetValue(column, out var data))
				throw new ArgumentException(string.Format("No such column: {0}", column));

			return ((ColumnData<T>) data);
		}

		IReadOnlyLogEntry IReadOnlyList<IReadOnlyLogEntry>.this[int index] => this[index];

		private sealed class LogEntryAccessor
			: AbstractLogEntry
		{
			private readonly LogEntryArray _array;
			private readonly int _index;

			public LogEntryAccessor(LogEntryArray array, int index)
			{
				if (array == null)
					throw new ArgumentNullException(nameof(array));

				_array = array;
				_index = index;
			}

			public override T GetValue<T>(ILogFileColumnDescriptor<T> column)
			{
				T value;
				if (!TryGetValue(column, out value))
					throw new ColumnNotRetrievedException(column);

				return value;
			}

			public override bool TryGetValue<T>(ILogFileColumnDescriptor<T> column, out T value)
			{
				IColumnData data;
				if (!_array._dataByColumn.TryGetValue(column, out data))
				{
					value = column.DefaultValue;
					return false;
				}

				value = ((ColumnData<T>)data)[_index];
				return true;
			}

			public override object GetValue(ILogFileColumnDescriptor column)
			{
				object value;
				if (!TryGetValue(column, out value))
					throw new ColumnNotRetrievedException(column);

				return value;
			}

			public override bool TryGetValue(ILogFileColumnDescriptor column, out object value)
			{
				IColumnData data;
				if (!_array._dataByColumn.TryGetValue(column, out data))
				{
					value = column.DefaultValue;
					return false;
				}

				value = data[_index];
				return true;
			}

			public override IReadOnlyList<ILogFileColumnDescriptor> Columns => _array._columns;

			public override void SetValue(ILogFileColumnDescriptor column, object value)
			{
				IColumnData data;
				if (!_array._dataByColumn.TryGetValue(column, out data))
					throw new ColumnNotRetrievedException(column);

				data[_index] = value;
			}

			public override void SetValue<T>(ILogFileColumnDescriptor<T> column, T value)
			{
				IColumnData data;
				if (!_array._dataByColumn.TryGetValue(column, out data))
					throw new ColumnNotRetrievedException(column);

				((ColumnData<T>)data)[_index] = value;
			}
		}

		private static IColumnData CreateColumnData(ILogFileColumnDescriptor column, int length)
		{
			dynamic tmp = column;
			return CreateColumnData(tmp, length);
		}

		private static IColumnData CreateColumnData<T>(ILogFileColumnDescriptor<T> column, int length)
		{
			return new ColumnData<T>(column, length);
		}

		interface IColumnData
		{
			object this[int index] { get; set; }
			void CopyFrom(int destinationIndex, ILogFile source, LogFileSection section);
			void CopyFrom(int destinationIndex, ILogFile source, IReadOnlyList<LogLineIndex> indices);
			void CopyFrom(int destinationIndex, IReadOnlyLogEntries source, IReadOnlyList<int> sourceIndices);
			void FillDefault(int destinationIndex, int length);
		}

		sealed class ColumnData<T>
			: IColumnData
			, IEnumerable<T>
		{
			private readonly ILogFileColumnDescriptor<T> _column;
			private readonly T[] _data;

			public ColumnData(ILogFileColumnDescriptor<T> column, int length)
			{
				if (column == null)
					throw new ArgumentNullException(nameof(column));

				_column = column;
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

			public void CopyFrom(int destIndex, T[] source, int sourceIndex, int length)
			{
				Array.Copy(source, sourceIndex, _data, destIndex, length);
			}

			public void CopyFrom(int destinationIndex, IReadOnlyList<T> source, IReadOnlyList<int> sourceIndices)
			{
				for (int i = 0; i < sourceIndices.Count; ++i)
				{
					_data[destinationIndex + i] = source[i];
				}
			}

			public void CopyFrom(int destinationIndex, ILogFile source, LogFileSection section)
			{
				source.GetColumn(section, _column, _data, destinationIndex);
			}

			public void CopyFrom(int destinationIndex, ILogFile source, IReadOnlyList<LogLineIndex> indices)
			{
				source.GetColumn(indices, _column, _data, destinationIndex);
			}

			public void CopyFrom(int destinationIndex, IReadOnlyLogEntries source, IReadOnlyList<int> sourceIndices)
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
				_data.Fill(_column.DefaultValue, destinationIndex, length);
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