using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		public void CopyTo<T>(ILogFileColumn<T> column, IReadOnlyList<int> sourceIndices, T[] destination, int destinationIndex, int length)
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
				((ColumnData<T>)columnData).CopyTo(sourceIndices, destination, destinationIndex, length);
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
		public void CopyFrom(ILogFileColumn column, int destinationIndex, ILogFile source, IReadOnlyList<LogLineIndex> indices)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			IColumnData columnData;
			if (!_dataByColumn.TryGetValue(column, out columnData))
				throw new NoSuchColumnException(column);

			columnData.CopyFrom(destinationIndex, source, indices);
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
		public IEnumerator<IReadOnlyLogEntry> GetEnumerator()
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

		IReadOnlyLogEntry IReadOnlyList<IReadOnlyLogEntry>.this[int index] => this[index];

		private sealed class LogEntryAccessor
			: ILogEntry
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
				if (!_buffer._dataByColumn.TryGetValue(column, out data))
					throw new ColumnNotRetrievedException(column);

				return ((ColumnData<T>) data)[_index];
			}

			public object GetColumnValue(ILogFileColumn column)
			{
				IColumnData data;
				if (!_buffer._dataByColumn.TryGetValue(column, out data))
					throw new ColumnNotRetrievedException(column);

				return data[_index];
			}

			public IReadOnlyList<ILogFileColumn> Columns => _buffer._columns;

			public void SetColumnValue(ILogFileColumn column, object value)
			{
				IColumnData data;
				if (!_buffer._dataByColumn.TryGetValue(column, out data))
					throw new ColumnNotRetrievedException(column);

				data[_index] = value;
			}

			public void SetColumnValue<T>(ILogFileColumn<T> column, T value)
			{
				IColumnData data;
				if (!_buffer._dataByColumn.TryGetValue(column, out data))
					throw new ColumnNotRetrievedException(column);

				((ColumnData<T>)data)[_index] = value;
			}

			public override string ToString()
			{
				var stringBuilder = new StringBuilder();
				foreach (var columnData in _buffer._dataByColumn.Values)
				{
					if (stringBuilder.Length > 0)
						stringBuilder.Append("|");
					stringBuilder.Append(columnData[_index]);
				}
				return stringBuilder.ToString();
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
		{
			private readonly ILogFileColumn<T> _column;
			private readonly T[] _data;

			public ColumnData(ILogFileColumn<T> column, int length)
			{
				if (column == null)
					throw new ArgumentNullException(nameof(column));

				_column = column;
				_data = new T[length];
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
				if (destinationIndex != 0)
					throw new NotImplementedException("The ILogFile interface needs to be changed for that!");

				source.GetColumn(section, _column, _data);
			}

			public void CopyFrom(int destinationIndex, ILogFile source, IReadOnlyList<LogLineIndex> indices)
			{
				if (destinationIndex != 0)
					throw new NotImplementedException("The ILogFile interface needs to be changed for that!");

				source.GetColumn(indices, _column, _data, destinationIndex);
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

				var tooMany = sourceIndex + length - _data.Length;
				if (tooMany > 0)
				{
					destination.Fill(default(T), destinationIndex + length - tooMany, tooMany);
					length -= tooMany;
				}

				Array.Copy(_data, sourceIndex, destination, destinationIndex, length);
			}

			public void CopyTo(IReadOnlyList<int> sourceIndices, T[] destination, int destinationIndex, int length)
			{
				for (int i = 0; i < length; ++i)
				{
					var sourceIndex = sourceIndices[i];
					if (sourceIndex >= 0 && sourceIndex < _data.Length)
					{
						destination[destinationIndex + i] = _data[sourceIndex];
					}
					else
					{
						destination[destinationIndex + i] = default(T);
					}
				}
			}

			public void FillDefault(int destinationIndex, int length)
			{
				_data.Fill(default(T), destinationIndex, length);
			}
		}
	}
}