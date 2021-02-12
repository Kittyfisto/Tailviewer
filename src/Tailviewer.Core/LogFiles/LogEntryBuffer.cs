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
	public sealed class LogEntryBuffer
		: ILogEntries
	{
		private readonly int _length;
		private readonly IReadOnlyList<ILogFileColumn> _columns;
		private readonly IReadOnlyDictionary<ILogFileColumn, IColumnData> _dataByColumn;

		/// <summary>
		/// </summary>
		/// <param name="length"></param>
		/// <param name="columns"></param>
		public LogEntryBuffer(int length, IEnumerable<ILogFileColumn> columns)
		{
			if (columns == null)
				throw new ArgumentNullException(nameof(columns));

			_length = length;
			_columns = columns.ToList();
			var dataByColumn = new Dictionary<ILogFileColumn, IColumnData>(_columns.Count);
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
		public LogEntryBuffer(int length, params ILogFileColumn[] columns)
			: this(length, (IEnumerable<ILogFileColumn>)columns)
		{}

		/// <inheritdoc />
		public IReadOnlyList<ILogFileColumn> Columns => _columns;

		/// <inheritdoc />
		public bool Contains(ILogFileColumn column)
		{
			return _dataByColumn.ContainsKey(column);
		}

		/// <inheritdoc />
		public void CopyTo<T>(ILogFileColumn<T> column, int sourceIndex, T[] destination, int destinationIndex, int length)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

			IColumnData columnData;
			if (_dataByColumn.TryGetValue(column, out columnData))
			{
				((ColumnData<T>)columnData).CopyTo(sourceIndex, destination, destinationIndex, length);
			}
			else
			{
				throw new NoSuchColumnException(column);
			}
		}

		/// <inheritdoc />
		public void CopyTo<T>(ILogFileColumn<T> column, IReadOnlyList<int> sourceIndices, T[] destination, int destinationIndex)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (sourceIndices == null)
				throw new ArgumentNullException(nameof(sourceIndices));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

			IColumnData columnData;
			if (_dataByColumn.TryGetValue(column, out columnData))
			{
				((ColumnData<T>)columnData).CopyTo(sourceIndices, destination, destinationIndex);
			}
			else
			{
				throw new NoSuchColumnException(column);
			}
		}

		/// <inheritdoc />
		public void CopyFrom<T>(ILogFileColumn<T> column, int destinationIndex, T[] source, int sourceIndex, int length)
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
		public void CopyFrom(ILogFileColumn column, int destinationIndex, ILogFile source, LogFileSection section)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			IColumnData columnData;
			if (!_dataByColumn.TryGetValue(column, out columnData))
				throw new NoSuchColumnException(column);

			columnData.CopyFrom(destinationIndex, source, section);
		}

		/// <inheritdoc />
		public void CopyFrom(ILogFileColumn column, int destinationIndex, ILogFile source, IReadOnlyList<LogLineIndex> sourceIndices)
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
		public void FillDefault(ILogFileColumn column, int destinationIndex, int length)
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
		public IEnumerable<T> Column<T>(ILogFileColumn<T> column)
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
			private readonly LogEntryBuffer _buffer;
			private readonly int _index;

			public LogEntryAccessor(LogEntryBuffer buffer, int index)
			{
				if (buffer == null)
					throw new ArgumentNullException(nameof(buffer));

				_buffer = buffer;
				_index = index;
			}

			public override T GetValue<T>(ILogFileColumn<T> column)
			{
				T value;
				if (!TryGetValue(column, out value))
					throw new ColumnNotRetrievedException(column);

				return value;
			}

			public override bool TryGetValue<T>(ILogFileColumn<T> column, out T value)
			{
				IColumnData data;
				if (!_buffer._dataByColumn.TryGetValue(column, out data))
				{
					value = column.DefaultValue;
					return false;
				}

				value = ((ColumnData<T>)data)[_index];
				return true;
			}

			public override object GetValue(ILogFileColumn column)
			{
				object value;
				if (!TryGetValue(column, out value))
					throw new ColumnNotRetrievedException(column);

				return value;
			}

			public override bool TryGetValue(ILogFileColumn column, out object value)
			{
				IColumnData data;
				if (!_buffer._dataByColumn.TryGetValue(column, out data))
				{
					value = column.DefaultValue;
					return false;
				}

				value = data[_index];
				return true;
			}

			public override IReadOnlyList<ILogFileColumn> Columns => _buffer._columns;

			public override void SetValue(ILogFileColumn column, object value)
			{
				IColumnData data;
				if (!_buffer._dataByColumn.TryGetValue(column, out data))
					throw new ColumnNotRetrievedException(column);

				data[_index] = value;
			}

			public override void SetValue<T>(ILogFileColumn<T> column, T value)
			{
				IColumnData data;
				if (!_buffer._dataByColumn.TryGetValue(column, out data))
					throw new ColumnNotRetrievedException(column);

				((ColumnData<T>)data)[_index] = value;
			}
		}

		private static IColumnData CreateColumnData(ILogFileColumn column, int length)
		{
			dynamic tmp = column;
			return CreateColumnData(tmp, length);
		}

		private static IColumnData CreateColumnData<T>(ILogFileColumn<T> column, int length)
		{
			return new ColumnData<T>(column, length);
		}

		interface IColumnData
		{
			object this[int index] { get; set; }
			void CopyFrom(int destinationIndex, ILogFile source, LogFileSection section);
			void CopyFrom(int destinationIndex, ILogFile source, IReadOnlyList<LogLineIndex> indices);
			void FillDefault(int destinationIndex, int length);
		}

		sealed class ColumnData<T>
			: IColumnData
			, IEnumerable<T>
		{
			private readonly ILogFileColumn<T> _column;
			private readonly T[] _data;

			public ColumnData(ILogFileColumn<T> column, int length)
			{
				if (column == null)
					throw new ArgumentNullException(nameof(column));

				_column = column;
				_data = new T[length];
				_data.Fill(column.DefaultValue);
			}

			public void CopyFrom(int destIndex, T[] source, int sourceIndex, int length)
			{
				Array.Copy(source, sourceIndex, _data, destIndex, length);
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

			public void CopyFrom(int destinationIndex, ILogFile source, LogFileSection section)
			{
				source.GetColumn(section, _column, _data, destinationIndex);
			}

			public void CopyFrom(int destinationIndex, ILogFile source, IReadOnlyList<LogLineIndex> indices)
			{
				source.GetColumn(indices, _column, _data, destinationIndex);
			}

			public void CopyTo(int sourceIndex, T[] destination, int destinationIndex, int length)
			{
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