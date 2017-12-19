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
	public sealed class LogEntryList
		: IReadOnlyLogEntries
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
		public IEnumerator<IReadOnlyLogEntry> GetEnumerator()
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
		public IReadOnlyLogEntry this[int index]
		{
			get
			{
				if (index < 0 || index >= _count)
					throw new ArgumentOutOfRangeException();

				return new LogEntryAccessor(this, index);
			}
		}

		/// <inheritdoc />
		public IReadOnlyList<ILogFileColumn> Columns => _columns;

		/// <inheritdoc />
		public void CopyTo<T>(ILogFileColumn<T> column, int sourceIndex, T[] destination, int destinationIndex, int length)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

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
		public void CopyTo<T>(ILogFileColumn<T> column, IReadOnlyList<int> sourceIndices, T[] destination, int destinationIndex, int length)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (sourceIndices == null)
				throw new ArgumentNullException(nameof(sourceIndices));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

			IColumnData data;
			if (_dataByColumn.TryGetValue(column, out data))
			{
				((ColumnData<T>)data).CopyTo(sourceIndices, destination, destinationIndex, length);
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

		private interface IColumnData
		{
			void Clear();
			void Add(IReadOnlyLogEntry logEntry);
			void RemoveAt(int index);
			void AddEmpty();
			void Insert(int index, IReadOnlyLogEntry logEntry);
			void RemoveRange(int index, int count);
			void InsertEmpty(int index);

			object this[int index] { get; }
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

			object IColumnData.this[int index] => _data[index];

			public T this[int index] => _data[index];

			public void Clear()
			{
				_data.Clear();
			}

			public void Add(IReadOnlyLogEntry logEntry)
			{
				var value = logEntry.GetColumnValue(_column);
				_data.Add(value);
			}

			public void RemoveAt(int index)
			{
				_data.RemoveAt(index);
			}

			public void AddEmpty()
			{
				_data.Add(default(T));
			}

			public void Insert(int index, IReadOnlyLogEntry logEntry)
			{
				var value = logEntry.GetColumnValue(_column);
				_data.Insert(index, value);
			}

			public void RemoveRange(int index, int count)
			{
				_data.RemoveRange(index, count);
			}

			public void InsertEmpty(int index)
			{
				_data.Insert(index, default(T));
			}

			public void CopyTo(int sourceIndex, T[] destination, int destinationIndex, int length)
			{
				// As usual, we allow access to invalid regions and return the default value
				// for that column.
				if (sourceIndex < 0)
				{
					destination.Fill(default(T), destinationIndex, -sourceIndex);
					length += sourceIndex;
					destinationIndex -= sourceIndex;
					sourceIndex = 0;
				}

				var tooMany = sourceIndex + length - _data.Count;
				if (tooMany > 0)
				{
					destination.Fill(default(T), destinationIndex + length - tooMany, tooMany);
					length -= tooMany;
				}

				_data.CopyTo(sourceIndex, destination, destinationIndex, length);
			}

			public void CopyTo(IReadOnlyList<int> sourceIndices, T[] destination, int destinationIndex, int length)
			{
				for (int i = 0; i < length; ++i)
				{
					var sourceIndex = sourceIndices[i];
					if (sourceIndex >= 0 && sourceIndex < _data.Count)
					{
						destination[destinationIndex + i] = _data[sourceIndex];
					}
					else
					{
						destination[destinationIndex + i] = default(T);
					}
				}
			}
		}

		private sealed class LogEntryAccessor
			: IReadOnlyLogEntry
		{
			private readonly int _index;
			private readonly LogEntryList _list;

			public LogEntryAccessor(LogEntryList list, int index)
			{
				_list = list;
				_index = index;
			}

			public string RawContent => GetColumnValue(LogFileColumns.RawContent);

			public LogLineIndex Index => GetColumnValue(LogFileColumns.Index);

			public LogLineIndex OriginalIndex => GetColumnValue(LogFileColumns.OriginalIndex);

			public LogEntryIndex LogEntryIndex => GetColumnValue(LogFileColumns.LogEntryIndex);

			public int LineNumber => GetColumnValue(LogFileColumns.LineNumber);

			public int OriginalLineNumber => GetColumnValue(LogFileColumns.OriginalLineNumber);

			public LevelFlags LogLevel => GetColumnValue(LogFileColumns.LogLevel);

			public DateTime? Timestamp => GetColumnValue(LogFileColumns.Timestamp);

			public TimeSpan? ElapsedTime => GetColumnValue(LogFileColumns.ElapsedTime);

			public TimeSpan? DeltaTime => GetColumnValue(LogFileColumns.DeltaTime);

			public T GetColumnValue<T>(ILogFileColumn<T> column)
			{
				IColumnData data;
				if (!_list._dataByColumn.TryGetValue(column, out data))
					throw new ColumnNotRetrievedException(column);

				return ((ColumnData<T>) data)[_index];
			}

			public object GetColumnValue(ILogFileColumn column)
			{
				IColumnData data;
				if (!_list._dataByColumn.TryGetValue(column, out data))
					throw new ColumnNotRetrievedException(column);

				return data[_index];
			}

			public IReadOnlyList<ILogFileColumn> Columns => _list._columns;
		}
	}
}