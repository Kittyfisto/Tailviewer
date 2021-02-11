using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///    This class offers a view onto a subset of columns of another <see cref="ILogEntry"/>.
	/// </summary>
	public sealed class ReadOnlyLogEntryView
		: IReadOnlyLogEntry
	{
		private readonly IReadOnlyLogEntry _logEntry;
		private readonly IReadOnlyList<ILogFileColumn> _columns;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logEntry"></param>
		/// <param name="columns"></param>
		public ReadOnlyLogEntryView(IReadOnlyLogEntry logEntry, params ILogFileColumn[] columns)
			: this(logEntry, (IReadOnlyList<ILogFileColumn>)columns)
		{}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logEntry"></param>
		/// <param name="columns"></param>
		public ReadOnlyLogEntryView(IReadOnlyLogEntry logEntry, IReadOnlyList<ILogFileColumn> columns)
		{
			_logEntry = logEntry;
			_columns = columns;
		}

		#region Implementation of IReadOnlyLogEntry

		/// <inheritdoc />
		public string RawContent
		{
			get
			{
				if (!_columns.Contains(LogFileColumns.RawContent))
					throw new NoSuchColumnException(LogFileColumns.RawContent);

				return _logEntry.RawContent;
			}
		}

		/// <inheritdoc />
		public LogLineIndex Index
		{
			get
			{
				if (!_columns.Contains(LogFileColumns.Index))
					throw new NoSuchColumnException(LogFileColumns.Index);

				return _logEntry.Index;
			}
		}

		/// <inheritdoc />
		public LogLineIndex OriginalIndex
		{
			get
			{
				if (!_columns.Contains(LogFileColumns.OriginalIndex))
					throw new NoSuchColumnException(LogFileColumns.OriginalIndex);

				return _logEntry.OriginalIndex;
			}
		}

		/// <inheritdoc />
		public LogEntryIndex LogEntryIndex
		{
			get
			{
				if (!_columns.Contains(LogFileColumns.LogEntryIndex))
					throw new NoSuchColumnException(LogFileColumns.LogEntryIndex);

				return _logEntry.LogEntryIndex;
			}
		}

		/// <inheritdoc />
		public int LineNumber
		{
			get
			{
				if (!_columns.Contains(LogFileColumns.LineNumber))
					throw new NoSuchColumnException(LogFileColumns.LineNumber);

				return _logEntry.LineNumber;
			}
		}

		/// <inheritdoc />
		public int OriginalLineNumber
		{
			get
			{
				if (!_columns.Contains(LogFileColumns.OriginalLineNumber))
					throw new NoSuchColumnException(LogFileColumns.OriginalLineNumber);

				return _logEntry.OriginalLineNumber;
			}
		}

		/// <inheritdoc />
		public LevelFlags LogLevel
		{
			get
			{
				if (!_columns.Contains(LogFileColumns.LogLevel))
					throw new NoSuchColumnException(LogFileColumns.LogLevel);

				return _logEntry.LogLevel;
			}
		}

		/// <inheritdoc />
		public DateTime? Timestamp
		{
			get
			{
				if (!_columns.Contains(LogFileColumns.Timestamp))
					throw new NoSuchColumnException(LogFileColumns.Timestamp);

				return _logEntry.Timestamp;
			}
		}

		/// <inheritdoc />
		public TimeSpan? ElapsedTime
		{
			get
			{
				if (!_columns.Contains(LogFileColumns.ElapsedTime))
					throw new NoSuchColumnException(LogFileColumns.ElapsedTime);

				return _logEntry.ElapsedTime;
			}
		}

		/// <inheritdoc />
		public TimeSpan? DeltaTime
		{
			get
			{
				if (!_columns.Contains(LogFileColumns.DeltaTime))
					throw new NoSuchColumnException(LogFileColumns.DeltaTime);

				return _logEntry.DeltaTime;
			}
		}

		/// <inheritdoc />
		public T GetValue<T>(ILogFileColumn<T> column)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			return _logEntry.GetValue(column);
		}

		/// <inheritdoc />
		public bool TryGetValue<T>(ILogFileColumn<T> column, out T value)
		{
			if (!_columns.Contains(column))
			{
				value = column.DefaultValue;
				return false;
			}

			return _logEntry.TryGetValue(column, out value);
		}

		/// <inheritdoc />
		public object GetValue(ILogFileColumn column)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			return _logEntry.GetValue(column);
		}

		/// <inheritdoc />
		public bool TryGetValue(ILogFileColumn column, out object value)
		{
			if (!_columns.Contains(column))
			{
				value = column.DefaultValue;
				return false;
			}

			return _logEntry.TryGetValue(column, out value);
		}

		/// <inheritdoc />
		public IReadOnlyList<ILogFileColumn> Columns
		{
			get { return _columns; }
		}

		#endregion
	}
}