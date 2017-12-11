using System;
using System.Collections;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     A buffer which holds and provides read/write access to a consecutive section of <see cref="ILogEntry" />s.
	/// </summary>
	public sealed class LogEntrySectionBuffer
		: ILogEntrySection
	{
		private readonly IReadOnlyList<ILogFileColumn> _columns;
		private readonly IReadOnlyDictionary<ILogFileColumn, IColumnData> _dataByColumn;

		/// <summary>
		/// </summary>
		/// <param name="section"></param>
		/// <param name="columns"></param>
		public LogEntrySectionBuffer(LogFileSection section, params ILogFileColumn[] columns)
		{
			if (columns == null)
				throw new ArgumentNullException(nameof(columns));

			Section = section;
			_columns = columns;
			var dataByColumn = new Dictionary<ILogFileColumn, IColumnData>(columns.Length);
			foreach (var column in columns)
			{
				dataByColumn.Add(column, CreateColumnData(column, section));
			}
			_dataByColumn = dataByColumn;
		}

		/// <inheritdoc />
		public LogFileSection Section { get; }

		/// <inheritdoc />
		public IReadOnlyList<ILogFileColumn> Columns => _columns;

		/// <inheritdoc />
		public void CopyFrom<T>(ILogFileColumn column, int destinationIndex, T[] source, int sourceIndex, int length)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			IColumnData data;
			if (_dataByColumn.TryGetValue(column, out data))
			{
				((ColumnData<T>)data).CopyFrom(destinationIndex, source, sourceIndex, length);
			}
			else
			{
				throw new NoSuchColumnException(column);
			}
		}

		/// <inheritdoc />
		public IEnumerator<ILogEntry> GetEnumerator()
		{
			for (var i = 0; i < Section.Count; ++i)
				yield return new LogEntryAccessor(this, i);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc />
		public int Count => Section.Count;

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
			private readonly LogEntrySectionBuffer _buffer;
			private readonly int _index;

			public LogEntryAccessor(LogEntrySectionBuffer buffer, int index)
			{
				if (buffer == null)
					throw new ArgumentNullException(nameof(buffer));

				_buffer = buffer;
				_index = index;
			}

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

		private static IColumnData CreateColumnData(ILogFileColumn column, LogFileSection section)
		{
			dynamic tmp = column;
			return CreateColumnData(tmp, section);
		}

		private static IColumnData CreateColumnData<T>(ILogFileColumn<T> column, LogFileSection section)
		{
			return new ColumnData<T>(section);
		}

		interface IColumnData
		{
		}

		sealed class ColumnData<T>
			: IColumnData
		{
			private readonly LogFileSection _section;
			private readonly T[] _data;

			public ColumnData(LogFileSection section)
			{
				_section = section;
				_data = new T[section.Count];
			}

			public void CopyFrom(int destIndex, T[] source, int sourceIndex, int length)
			{
				Array.Copy(source, sourceIndex, _data, destIndex, length);
			}

			public T this[int index] => _data[index];
		}
	}
}