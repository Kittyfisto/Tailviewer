using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     A variable-length buffer which provides read/write access to <see cref="IReadOnlyLogEntry" />s:
	///     New rows can be added, existing rows modified and removed.
	/// </summary>
	/// <remarks>
	///     Log entries are stored in the order they are added (regardless of their <see cref="IReadOnlyLogEntry.Index"/>, <see cref="IReadOnlyLogEntry.LogEntryIndex"/>, etc...).
	/// </remarks>
	public sealed class LogEntryList
		: ILogEntries
	{
		private readonly IReadOnlyDictionary<ILogFileColumn, IColumnData> _dataByColumn;
		private readonly IReadOnlyList<ILogFileColumn> _columns;

		private int _count;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="columns"></param>
		public LogEntryList(IEnumerable<ILogFileColumn> columns)
		{
			_columns = new List<ILogFileColumn>(columns);
			var dataByColumn = new Dictionary<ILogFileColumn, IColumnData>(_columns.Count);
			foreach (var column in _columns)
				dataByColumn.Add(column, CreateColumnData(column));
			_dataByColumn = dataByColumn;
			_count = 0;
		}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="columns"></param>
		public LogEntryList(params ILogFileColumn[] columns)
			: this((IEnumerable<ILogFileColumn>) columns)
		{
		}

		/// <inheritdoc />
		public IEnumerator<ILogEntry> GetEnumerator()
		{
			return new LogEntriesEnumerator(this);
		}

		/// <inheritdoc />
		IEnumerator<IReadOnlyLogEntry> IEnumerable<IReadOnlyLogEntry>.GetEnumerator()
		{
			return new ReadOnlyLogEntriesEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc />
		public int Count => _count;

		/// <inheritdoc />
		public ILogEntry this[int index]
		{
			get
			{
				if (index < 0 || index >= _count)
					throw new ArgumentOutOfRangeException();

				return new LogEntryAccessor(this, index);
			}
		}

		IReadOnlyLogEntry IReadOnlyList<IReadOnlyLogEntry>.this[int index]
		{
			get
			{
				if (index < 0 || index >= _count)
					throw new ArgumentOutOfRangeException();

				return new ReadOnlyLogEntryAccessor(this, index);
			}
		}

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
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			if (destinationIndex + length > destination.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

			IColumnData data;
			if (_dataByColumn.TryGetValue(column, out data))
			{
				((ColumnData<T>)data).CopyTo(sourceIndex, destination, destinationIndex, length);
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
			if (destinationIndex + sourceIndices.Count > destination.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

			IColumnData data;
			if (_dataByColumn.TryGetValue(column, out data))
			{
				((ColumnData<T>)data).CopyTo(sourceIndices, destination, destinationIndex);
			}
			else
			{
				throw new NoSuchColumnException(column);
			}
		}

		[Pure]
		private static IColumnData CreateColumnData(ILogFileColumn column)
		{
			dynamic tmp = column;
			return CreateColumnData(tmp);
		}

		[Pure]
		private static IColumnData CreateColumnData<T>(ILogFileColumn<T> column)
		{
			return new ColumnData<T>(column);
		}

		/// <summary>
		///     Adds the given log entry to this list.
		/// </summary>
		/// <remarks>
		///     The log entry must support the same columns as this list, but they do
		///     not need to be in the same order.
		/// </remarks>
		/// <param name="logEntry"></param>
		public void Add(IReadOnlyLogEntry logEntry)
		{
			foreach (var column in _dataByColumn.Values)
			{
				column.Add(logEntry);
			}
			++_count;
		}

		/// <summary>
		///     Adds the given log entry to this list.
		///     It is expected that as many valeus as there are columns are passed
		///     and that the values match the column's data type.
		/// </summary>
		/// <param name="values"></param>
		public void Add(params object[] values)
		{
			var logEntry = ReadOnlyLogEntry.Create(_columns,
			                                       values);
			Add(logEntry);
		}

		/// <summary>
		///     Adds a completely empty row to this list.
		///     Every cell will be filled with that column's default value.
		/// </summary>
		public void AddEmpty()
		{
			foreach (var column in _dataByColumn.Values)
			{
				column.AddEmpty();
			}
			++_count;
		}

		/// <summary>
		///   
		/// </summary>
		/// <param name="entries"></param>
		public void AddRange(IReadOnlyLogEntries entries)
		{
			foreach (var logEntry in entries)
			{
				Add(logEntry);
			}
		}

		/// <summary>
		///   
		/// </summary>
		/// <param name="count"></param>
		public void AddRange(int count)
		{
			foreach(var column in _dataByColumn.Values)
			{
				column.AddRange(count);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="index"></param>
		/// <param name="logEntry"></param>
		public void Insert(int index, IReadOnlyLogEntry logEntry)
		{
			foreach (var column in _dataByColumn.Values)
			{
				column.Insert(index, logEntry);
			}
			++_count;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="values"></param>
		public void Insert(int index, params object[] values)
		{
			Insert(index, ReadOnlyLogEntry.Create(_columns, values));
		}

		/// <summary>
		///     Inserts a completely empty row to this list.
		///     Every cell will be filled with that column's default value.
		/// </summary>
		/// <param name="index"></param>
		public void InsertEmpty(int index)
		{
			foreach (var column in _dataByColumn.Values)
			{
				column.InsertEmpty(index);
			}
			++_count;
		}

		/// <summary>
		///     Removes the log entry at the given index.
		/// </summary>
		/// <param name="index"></param>
		/// <exception cref="NotImplementedException"></exception>
		public void RemoveAt(int index)
		{
			foreach (var column in _dataByColumn.Values)
			{
				column.RemoveAt(index);
			}
			--_count;
		}

		/// <summary>
		///     Removes the log entries in the given range.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <exception cref="NotImplementedException"></exception>
		public void RemoveRange(int index, int count)
		{
			foreach (var column in _dataByColumn.Values)
			{
				column.RemoveRange(index, count);
			}
			_count -= count;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="count"></param>
		public void Resize(int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException($"Resize to {count} not allowed");

			
			if (count < _count)
			{
				var startIndex = count;
				var toRemove = _count - count;
				RemoveRange(startIndex, toRemove);
			}
			else if (count > _count)
			{
				var toAdd = count - _count;
				AddRange(toAdd);
			}
			_count = count;
		}

		/// <summary>
		///     Removes all log entries.
		/// </summary>
		public void Clear()
		{
			foreach (var column in _dataByColumn.Values)
			{
				column.Clear();
			}
			_count = 0;
		}

		/// <inheritdoc />
		public void CopyFrom<T>(ILogFileColumn<T> column, int destinationIndex, T[] source, int sourceIndex, int length)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			if (destinationIndex + length > _count)
				throw new ArgumentOutOfRangeException();

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

		private sealed class ReadOnlyLogEntryAccessor
			: AbstractReadOnlyLogEntry
		{
			private readonly int _index;
			private readonly LogEntryList _list;

			public ReadOnlyLogEntryAccessor(LogEntryList list, int index)
			{
				_list = list;
				_index = index;
			}

			public override T GetValue<T>(ILogFileColumn<T> column)
			{
				T data;
				if (!TryGetValue(column, out data))
					throw new ColumnNotRetrievedException(column);

				return data;
			}

			public override bool TryGetValue<T>(ILogFileColumn<T> column, out T value)
			{
				IColumnData data;
				if (!_list._dataByColumn.TryGetValue(column, out data))
				{
					value = column.DefaultValue;
					return false;
				}

				value = ((ColumnData<T>)data)[_index];
				return true;
			}

			public override object GetValue(ILogFileColumn column)
			{
				object data;
				if (!TryGetValue(column, out data))
					throw new ColumnNotRetrievedException(column);

				return data;
			}

			public override bool TryGetValue(ILogFileColumn column, out object value)
			{
				IColumnData data;
				if (!_list._dataByColumn.TryGetValue(column, out data))
				{
					value = column.DefaultValue;
					return false;
				}

				value = data[_index];
				return true;
			}

			public override IReadOnlyList<ILogFileColumn> Columns => _list._columns;
		}

		private sealed class LogEntryAccessor
			: AbstractLogEntry
		{
			private readonly int _index;
			private readonly LogEntryList _list;

			public LogEntryAccessor(LogEntryList list, int index)
			{
				_list = list;
				_index = index;
			}

			public override T GetValue<T>(ILogFileColumn<T> column)
			{
				T data;
				if (!TryGetValue(column, out data))
					throw new ColumnNotRetrievedException(column);

				return data;
			}

			public override bool TryGetValue<T>(ILogFileColumn<T> column, out T value)
			{
				IColumnData data;
				if (!_list._dataByColumn.TryGetValue(column, out data))
				{
					value = column.DefaultValue;
					return false;
				}

				value = ((ColumnData<T>)data)[_index];
				return true;
			}

			public override object GetValue(ILogFileColumn column)
			{
				if (!TryGetValue(column, out var data))
					throw new ColumnNotRetrievedException(column);

				return data;
			}

			public override bool TryGetValue(ILogFileColumn column, out object value)
			{
				if (!_list._dataByColumn.TryGetValue(column, out var data))
				{
					value = column.DefaultValue;
					return false;
				}

				value = data[_index];
				return true;
			}

			public override void SetValue(ILogFileColumn column, object value)
			{
				if (!_list._dataByColumn.TryGetValue(column, out var data))
					throw new NoSuchColumnException(column);

				data[_index] = value;
			}

			public override void SetValue<T>(ILogFileColumn<T> column, T value)
			{
				if (!_list._dataByColumn.TryGetValue(column, out var data))
					throw new NoSuchColumnException(column);

				((ColumnData<T>) data)[_index] = value;
			}

			public override IReadOnlyList<ILogFileColumn> Columns => _list._columns;
		}

		private interface IColumnData
		{
			object this[int index] { get; set; }

			void Clear();

			void Add(IReadOnlyLogEntry logEntry);

			void RemoveAt(int index);

			void AddEmpty();

			void Insert(int index, IReadOnlyLogEntry logEntry);
			void RemoveRange(int index, int count);

			void InsertEmpty(int index);

			void FillDefault(int destinationIndex, int length);

			void CopyFrom(int destinationIndex, ILogFile source, LogFileSection section);
			void CopyFrom(int destinationIndex, ILogFile source, IReadOnlyList<LogLineIndex> indices);
			void AddRange(int count);
		}

		private sealed class ColumnData<T>
			: IColumnData
		{
			private readonly ILogFileColumn<T> _column;
			private readonly List<T> _data;

			public ColumnData(ILogFileColumn<T> column)
			{
				_column = column;
				_data = new List<T>();
			}

			object IColumnData.this[int index]
			{
				get { return _data[index]; }
				set { _data[index] = (T) value; }
			}

			public T this[int index]
			{
				get { return _data[index]; }
				set { _data[index] = value; }
			}

			public void Clear()
			{
				_data.Clear();
			}

			public void Add(IReadOnlyLogEntry logEntry)
			{
				T value;
				_data.Add(logEntry.TryGetValue(_column, out value)
					? value
					: _column.DefaultValue);
			}

			public void RemoveAt(int index)
			{
				_data.RemoveAt(index);
			}

			public void AddEmpty()
			{
				_data.Add(_column.DefaultValue);
			}

			public void Insert(int index, IReadOnlyLogEntry logEntry)
			{
				var value = logEntry.GetValue(_column);
				_data.Insert(index, value);
			}

			public void RemoveRange(int index, int count)
			{
				_data.RemoveRange(index, count);
			}

			public void InsertEmpty(int index)
			{
				_data.Insert(index, _column.DefaultValue);
			}

			public void FillDefault(int destinationIndex, int length)
			{
				_data.Fill(_column.DefaultValue, destinationIndex, length);
			}

			public void CopyFrom(int destinationIndex, ILogFile source, LogFileSection section)
			{
				// TODO: Write custom List implementation which allows access to its internal buffer so we can void the allocation and additional copy here
				var maxCount = destinationIndex + section.Count;
				while (maxCount > _data.Count)
					_data.Add(default);

				var buffer = new T[section.Count];
				source.GetColumn(section, _column, buffer, 0);
				for (int i = 0; i < buffer.Length; ++i)
				{
					_data[destinationIndex + i] = buffer[i];
				}
			}

			public void CopyFrom(int destinationIndex, ILogFile source, IReadOnlyList<LogLineIndex> indices)
			{
				// TODO: Write custom List implementation which allows access to its internal buffer so we can void the allocation and additional copy here
				var maxCount = destinationIndex + indices.Count;
				while (maxCount > _data.Count)
					_data.Add(default);

				var buffer = new T[indices.Count];
				source.GetColumn(indices, _column, buffer, 0);
				for (int i = 0; i < buffer.Length; ++i)
				{
					_data[destinationIndex + i] = buffer[i];
				}
			}

			public void AddRange(int count)
			{
				for (int i = 0; i < count; ++i)
				{
					_data.Add(_column.DefaultValue);
				}
			}

			public void CopyTo(int sourceIndex, T[] destination, int destinationIndex, int length)
			{
				if (length <= 0)
					return;

				// As usual, we allow access to invalid regions and return the default value
				// for that column.
				if (sourceIndex < 0)
				{
					destination.Fill(_column.DefaultValue, destinationIndex, -sourceIndex);
					length += sourceIndex;
					destinationIndex -= sourceIndex;
					sourceIndex = 0;
				}

				if (length > 0 && sourceIndex + length > _data.Count)
				{
					int tooMany;
					if (sourceIndex < _data.Count)
					{
						tooMany = sourceIndex + length - _data.Count;
					}
					else
					{
						tooMany = length;
					}

					destination.Fill(_column.DefaultValue, destinationIndex + length - tooMany, tooMany);
					length -= tooMany;
				}

				if (length > 0)
				{
					_data.CopyTo(sourceIndex, destination, destinationIndex, length);
				}
			}

			public void CopyTo(IReadOnlyList<int> sourceIndices, T[] destination, int destinationIndex)
			{
				for (int i = 0; i < sourceIndices.Count; ++i)
				{
					var sourceIndex = sourceIndices[i];
					if (sourceIndex >= 0 && sourceIndex < _data.Count)
					{
						destination[destinationIndex + i] = _data[sourceIndex];
					}
					else
					{
						destination[destinationIndex + i] = _column.DefaultValue;
					}
				}
			}

			public void CopyFrom(int destinationIndex, T[] source, int sourceIndex, int length)
			{
				for (int i = 0; i < length; ++i)
				{
					_data[destinationIndex + i] = source[sourceIndex + i];
				}
			}
		}
	}
}