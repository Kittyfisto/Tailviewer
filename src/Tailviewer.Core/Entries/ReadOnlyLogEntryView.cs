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
				if (!_columns.Contains(GeneralColumns.RawContent))
					throw new NoSuchColumnException(GeneralColumns.RawContent);

				return _logEntry.RawContent;
			}
		}

		/// <inheritdoc />
		public LogLineIndex Index
		{
			get
			{
				if (!_columns.Contains(GeneralColumns.Index))
					throw new NoSuchColumnException(GeneralColumns.Index);

				return _logEntry.Index;
			}
		}

		/// <inheritdoc />
		public LogLineIndex OriginalIndex
		{
			get
			{
				if (!_columns.Contains(GeneralColumns.OriginalIndex))
					throw new NoSuchColumnException(GeneralColumns.OriginalIndex);

				return _logEntry.OriginalIndex;
			}
		}

		/// <inheritdoc />
		public LogEntryIndex LogEntryIndex
		{
			get
			{
				if (!_columns.Contains(GeneralColumns.LogEntryIndex))
					throw new NoSuchColumnException(GeneralColumns.LogEntryIndex);

				return _logEntry.LogEntryIndex;
			}
		}

		/// <inheritdoc />
		public int LineNumber
		{
			get
			{
				if (!_columns.Contains(GeneralColumns.LineNumber))
					throw new NoSuchColumnException(GeneralColumns.LineNumber);

				return _logEntry.LineNumber;
			}
		}

		/// <inheritdoc />
		public int OriginalLineNumber
		{
			get
			{
				if (!_columns.Contains(GeneralColumns.OriginalLineNumber))
					throw new NoSuchColumnException(GeneralColumns.OriginalLineNumber);

				return _logEntry.OriginalLineNumber;
			}
		}

		/// <inheritdoc />
		public string OriginalDataSourceName
		{
			get
			{
				if (!_columns.Contains(GeneralColumns.OriginalDataSourceName))
					throw new NoSuchColumnException(GeneralColumns.OriginalDataSourceName);

				return _logEntry.OriginalDataSourceName;
			}
		}

		/// <inheritdoc />
		public LogLineSourceId SourceId
		{
			get
			{
				if (!_columns.Contains(GeneralColumns.SourceId))
					throw new NoSuchColumnException(GeneralColumns.SourceId);

				return _logEntry.SourceId;
			}
		}

		/// <inheritdoc />
		public LevelFlags LogLevel
		{
			get
			{
				if (!_columns.Contains(GeneralColumns.LogLevel))
					throw new NoSuchColumnException(GeneralColumns.LogLevel);

				return _logEntry.LogLevel;
			}
		}

		/// <inheritdoc />
		public DateTime? Timestamp
		{
			get
			{
				if (!_columns.Contains(GeneralColumns.Timestamp))
					throw new NoSuchColumnException(GeneralColumns.Timestamp);

				return _logEntry.Timestamp;
			}
		}

		/// <inheritdoc />
		public TimeSpan? ElapsedTime
		{
			get
			{
				if (!_columns.Contains(GeneralColumns.ElapsedTime))
					throw new NoSuchColumnException(GeneralColumns.ElapsedTime);

				return _logEntry.ElapsedTime;
			}
		}

		/// <inheritdoc />
		public TimeSpan? DeltaTime
		{
			get
			{
				if (!_columns.Contains(GeneralColumns.DeltaTime))
					throw new NoSuchColumnException(GeneralColumns.DeltaTime);

				return _logEntry.DeltaTime;
			}
		}

		/// <inheritdoc />
		public string Message
		{
			get
			{
				if (!_columns.Contains(GeneralColumns.Message))
					throw new NoSuchColumnException(GeneralColumns.Message);

				return _logEntry.Message;
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