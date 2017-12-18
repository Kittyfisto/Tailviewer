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
			throw new NotImplementedException();
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
		///     Inserts a completely empty row to this list.
		///     Every cell will be filled with that column's default value.
		/// </summary>
		/// <param name="index"></param>
		public void InsertEmpty(int index)
		{
			throw new NotImplementedException();
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
		}

		private sealed class ColumnData<T>
			: IColumnData
		{
			private readonly ILogFileColumn<T> _column;
			private readonly List<T> _values;

			public ColumnData(ILogFileColumn<T> column)
			{
				if (column == null)
					throw new ArgumentNullException(nameof(column));

				_column = column;
				_values = new List<T>();
			}

			public T this[int index] => _values[index];

			public void Clear()
			{
				_values.Clear();
			}

			public void Add(IReadOnlyLogEntry logEntry)
			{
				var value = logEntry.GetColumnValue(_column);
				_values.Add(value);
			}

			public void RemoveAt(int index)
			{
				_values.RemoveAt(index);
			}

			public void AddEmpty()
			{
				_values.Add(default(T));
			}

			public void Insert(int index, IReadOnlyLogEntry logEntry)
			{
				var value = logEntry.GetColumnValue(_column);
				_values.Insert(index, value);
			}

			public void RemoveRange(int index, int count)
			{
				_values.RemoveRange(index, count);
			}
		}

		private sealed class LogEntryAccessor
			: IReadOnlyLogEntry
		{
			private readonly int _index;
			private readonly LogEntryList _list;

			public LogEntryAccessor(LogEntryList list, int index)
			{
				if (list == null)
					throw new ArgumentNullException(nameof(list));

				_list = list;
				_index = index;
			}

			public string RawContent => GetColumnValue(LogFileColumns.RawContent);

			public LogEntryIndex Index => GetColumnValue(LogFileColumns.Index);

			public LogEntryIndex OriginalIndex => GetColumnValue(LogFileColumns.OriginalIndex);

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

			public IReadOnlyList<ILogFileColumn> Columns => _list._columns;
		}
	}
}