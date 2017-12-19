using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogTables;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// TODO: Rename to LogEntry once <see cref="LogEntry"/> is removed.
	/// </remarks>
	public sealed class LogEntry2
		: ILogEntry
	{
		private readonly Dictionary<ILogFileColumn, object> _values;
		private readonly List<ILogFileColumn> _columns;

		/// <summary>
		/// 
		/// </summary>
		public LogEntry2()
		{
			_values = new Dictionary<ILogFileColumn, object>();
			_columns = new List<ILogFileColumn>();
		}

		/// <summary>
		///     Adds a new column to this log entry.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="value"></param>
		/// <typeparam name="T"></typeparam>
		public void Add<T>(ILogFileColumn<T> column, T value)
		{
			_values.Add(column, value);
			_columns.Add(column);
		}

		/// <inheritdoc />
		public string RawContent => GetColumnValue(LogFileColumns.RawContent);

		/// <inheritdoc />
		public LogLineIndex Index => GetColumnValue(LogFileColumns.Index);

		/// <inheritdoc />
		public LogLineIndex OriginalIndex => GetColumnValue(LogFileColumns.OriginalIndex);

		/// <inheritdoc />
		public LogEntryIndex LogEntryIndex => GetColumnValue(LogFileColumns.LogEntryIndex);

		/// <inheritdoc />
		public int LineNumber => GetColumnValue(LogFileColumns.LineNumber);

		/// <inheritdoc />
		public int OriginalLineNumber => GetColumnValue(LogFileColumns.OriginalLineNumber);

		/// <inheritdoc />
		public LevelFlags LogLevel => GetColumnValue(LogFileColumns.LogLevel);

		/// <inheritdoc />
		public DateTime? Timestamp => GetColumnValue(LogFileColumns.Timestamp);

		/// <inheritdoc />
		public TimeSpan? ElapsedTime => GetColumnValue(LogFileColumns.ElapsedTime);

		/// <inheritdoc />
		public TimeSpan? DeltaTime => GetColumnValue(LogFileColumns.DeltaTime);

		/// <inheritdoc />
		public T GetColumnValue<T>(ILogFileColumn<T> column)
		{
			return (T) GetColumnValue((ILogFileColumn) column);
		}

		/// <inheritdoc />
		public object GetColumnValue(ILogFileColumn column)
		{
			object value;
			if (!_values.TryGetValue(column, out value))
				throw new NoSuchColumnException(column);

			return value;
		}

		/// <inheritdoc />
		public IReadOnlyList<ILogFileColumn> Columns => _columns;

		/// <inheritdoc />
		public void SetColumnValue(ILogFileColumn column, object value)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public void SetColumnValue<T>(ILogFileColumn<T> column, T value)
		{
			throw new NotImplementedException();
		}
	}
}