using System;
using System.Collections;
using System.Collections.Generic;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     A buffer which holds and provides read/write access to a consecutive section of <see cref="ILogEntry" />s.
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
		public LogEntryBuffer(int length, params ILogFileColumn[] columns)
		{
			if (columns == null)
				throw new ArgumentNullException(nameof(columns));

			_length = length;
			_columns = columns;
			var dataByColumn = new Dictionary<ILogFileColumn, IColumnData>(columns.Length);
			foreach (var column in columns)
			{
				dataByColumn.Add(column, CreateColumnData(column, length));
			}
			_dataByColumn = dataByColumn;
		}

		/// <inheritdoc />
		public IReadOnlyList<ILogFileColumn> Columns => _columns;

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
			if (_dataByColumn.TryGetValue(column, out columnData))
			{
				columnData.CopyFrom(destinationIndex, source, section);
			}
			else
			{
				throw new NoSuchColumnException(column);
			}
		}

		/// <inheritdoc />
		public void CopyFrom(ILogFileColumn column, int destinationIndex, ILogFile source, IReadOnlyList<LogLineIndex> indices)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			IColumnData columnData;
			if (_dataByColumn.TryGetValue(column, out columnData))
			{
				columnData.CopyFrom(destinationIndex, source, indices);
			}
			else
			{
				throw new NoSuchColumnException(column);
			}
		}

		/// <inheritdoc />
		public IEnumerator<ILogEntry> GetEnumerator()
		{
			for (var i = 0; i < _length; ++i)
				yield return new LogEntryAccessor(this, i);
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

			public LogEntryIndex Index => GetColumnValue(LogFileColumns.Index);

			public LogEntryIndex OriginalIndex => GetColumnValue(LogFileColumns.OriginalIndex);

			public DateTime? Timestamp => GetColumnValue(LogFileColumns.TimeStamp);

			public string RawContent => GetColumnValue(LogFileColumns.RawContent);

			public T GetColumnValue<T>(ILogFileColumn<T> column)
			{
				IColumnData data;
				if (_buffer._dataByColumn.TryGetValue(column, out data))
				{
					return ((ColumnData<T>) data)[_index];
				}
				else
				{
					throw new NotImplementedException();
				}
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
			void CopyFrom(int destinationIndex, ILogFile source, LogFileSection section);
			void CopyFrom(int destinationIndex, ILogFile source, IReadOnlyList<LogLineIndex> indices);
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

			public T this[int index] => _data[index];

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

				source.GetColumn(indices, _column, _data);
			}
		}
	}
}