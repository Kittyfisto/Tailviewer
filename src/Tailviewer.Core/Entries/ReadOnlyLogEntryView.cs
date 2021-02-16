using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Core.Columns;

namespace Tailviewer.Core.Entries
{
	/// <summary>
	///    This class offers a view onto a subset of columns of another <see cref="ILogEntry"/>.
	/// </summary>
	public sealed class ReadOnlyLogEntryView
		: IReadOnlyLogEntry
	{
		private readonly IReadOnlyLogEntry _logEntry;
		private readonly IReadOnlyList<IColumnDescriptor> _columns;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logEntry"></param>
		/// <param name="columns"></param>
		public ReadOnlyLogEntryView(IReadOnlyLogEntry logEntry, params IColumnDescriptor[] columns)
			: this(logEntry, (IReadOnlyList<IColumnDescriptor>)columns)
		{}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logEntry"></param>
		/// <param name="columns"></param>
		public ReadOnlyLogEntryView(IReadOnlyLogEntry logEntry, IReadOnlyList<IColumnDescriptor> columns)
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
				if (!_columns.Contains(LogColumns.RawContent))
					throw new NoSuchColumnException(LogColumns.RawContent);

				return _logEntry.RawContent;
			}
		}

		/// <inheritdoc />
		public LogLineIndex Index
		{
			get
			{
				if (!_columns.Contains(LogColumns.Index))
					throw new NoSuchColumnException(LogColumns.Index);

				return _logEntry.Index;
			}
		}

		/// <inheritdoc />
		public LogLineIndex OriginalIndex
		{
			get
			{
				if (!_columns.Contains(LogColumns.OriginalIndex))
					throw new NoSuchColumnException(LogColumns.OriginalIndex);

				return _logEntry.OriginalIndex;
			}
		}

		/// <inheritdoc />
		public LogEntryIndex LogEntryIndex
		{
			get
			{
				if (!_columns.Contains(LogColumns.LogEntryIndex))
					throw new NoSuchColumnException(LogColumns.LogEntryIndex);

				return _logEntry.LogEntryIndex;
			}
		}

		/// <inheritdoc />
		public int LineNumber
		{
			get
			{
				if (!_columns.Contains(LogColumns.LineNumber))
					throw new NoSuchColumnException(LogColumns.LineNumber);

				return _logEntry.LineNumber;
			}
		}

		/// <inheritdoc />
		public int OriginalLineNumber
		{
			get
			{
				if (!_columns.Contains(LogColumns.OriginalLineNumber))
					throw new NoSuchColumnException(LogColumns.OriginalLineNumber);

				return _logEntry.OriginalLineNumber;
			}
		}

		/// <inheritdoc />
		public string OriginalDataSourceName
		{
			get
			{
				if (!_columns.Contains(LogColumns.OriginalDataSourceName))
					throw new NoSuchColumnException(LogColumns.OriginalDataSourceName);

				return _logEntry.OriginalDataSourceName;
			}
		}

		/// <inheritdoc />
		public LogLineSourceId SourceId
		{
			get
			{
				if (!_columns.Contains(LogColumns.SourceId))
					throw new NoSuchColumnException(LogColumns.SourceId);

				return _logEntry.SourceId;
			}
		}

		/// <inheritdoc />
		public LevelFlags LogLevel
		{
			get
			{
				if (!_columns.Contains(LogColumns.LogLevel))
					throw new NoSuchColumnException(LogColumns.LogLevel);

				return _logEntry.LogLevel;
			}
		}

		/// <inheritdoc />
		public DateTime? Timestamp
		{
			get
			{
				if (!_columns.Contains(LogColumns.Timestamp))
					throw new NoSuchColumnException(LogColumns.Timestamp);

				return _logEntry.Timestamp;
			}
		}

		/// <inheritdoc />
		public TimeSpan? ElapsedTime
		{
			get
			{
				if (!_columns.Contains(LogColumns.ElapsedTime))
					throw new NoSuchColumnException(LogColumns.ElapsedTime);

				return _logEntry.ElapsedTime;
			}
		}

		/// <inheritdoc />
		public TimeSpan? DeltaTime
		{
			get
			{
				if (!_columns.Contains(LogColumns.DeltaTime))
					throw new NoSuchColumnException(LogColumns.DeltaTime);

				return _logEntry.DeltaTime;
			}
		}

		/// <inheritdoc />
		public T GetValue<T>(IColumnDescriptor<T> column)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			return _logEntry.GetValue(column);
		}

		/// <inheritdoc />
		public bool TryGetValue<T>(IColumnDescriptor<T> column, out T value)
		{
			if (!_columns.Contains(column))
			{
				value = column.DefaultValue;
				return false;
			}

			return _logEntry.TryGetValue(column, out value);
		}

		/// <inheritdoc />
		public object GetValue(IColumnDescriptor column)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			return _logEntry.GetValue(column);
		}

		/// <inheritdoc />
		public bool TryGetValue(IColumnDescriptor column, out object value)
		{
			if (!_columns.Contains(column))
			{
				value = column.DefaultValue;
				return false;
			}

			return _logEntry.TryGetValue(column, out value);
		}

		/// <inheritdoc />
		public IReadOnlyList<IColumnDescriptor> Columns
		{
			get { return _columns; }
		}

		#endregion
	}
}