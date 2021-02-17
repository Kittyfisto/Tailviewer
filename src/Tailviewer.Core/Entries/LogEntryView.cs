using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Core.Columns;

namespace Tailviewer.Core.Entries
{
	/// <summary>
	///    This class offers a view onto a subset of columns of another <see cref="ILogEntry"/>.
	/// </summary>
	/// <remarks>
	///     This class implements a similar concept to what System.Span{T} implements: The data isn't copied over, instead
	///     it offers a view onto a sub-region (in this case limited by the set of columns given during construction) of log entries
	///     which still reside in another <see cref="ILogBuffer"/> object: When entries change in the source, then so do they
	///     change when viewed through this object.
	/// </remarks>
	public sealed class LogEntryView
		: ILogEntry
	{
		private readonly ILogEntry _logEntry;
		private readonly IReadOnlyList<IColumnDescriptor> _columns;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logEntry"></param>
		/// <param name="columns"></param>
		public LogEntryView(ILogEntry logEntry, params IColumnDescriptor[] columns)
			: this(logEntry, (IReadOnlyList<IColumnDescriptor>)columns)
		{}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logEntry"></param>
		/// <param name="columns"></param>
		public LogEntryView(ILogEntry logEntry, IReadOnlyList<IColumnDescriptor> columns)
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
			set
			{
				if (!_columns.Contains(LogColumns.RawContent))
					throw new NoSuchColumnException(LogColumns.RawContent);

				_logEntry.RawContent = value;
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
			set
			{
				if (!_columns.Contains(LogColumns.Index))
					throw new NoSuchColumnException(LogColumns.Index);

				_logEntry.Index = value;
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
			set
			{
				if (!_columns.Contains(LogColumns.OriginalIndex))
					throw new NoSuchColumnException(LogColumns.OriginalIndex);

				_logEntry.OriginalIndex = value;
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
			set
			{
				if (!_columns.Contains(LogColumns.LogEntryIndex))
					throw new NoSuchColumnException(LogColumns.LogEntryIndex);

				_logEntry.LogEntryIndex = value;
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
			set
			{
				if (!_columns.Contains(LogColumns.LineNumber))
					throw new NoSuchColumnException(LogColumns.LineNumber);

				_logEntry.LineNumber = value;
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
			set
			{
				if (!_columns.Contains(LogColumns.OriginalLineNumber))
					throw new NoSuchColumnException(LogColumns.OriginalLineNumber);

				_logEntry.OriginalLineNumber = value;
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
			set
			{
				if (!_columns.Contains(LogColumns.OriginalDataSourceName))
					throw new NoSuchColumnException(LogColumns.OriginalDataSourceName);

				_logEntry.OriginalDataSourceName = value;
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
			set
			{
				if (!_columns.Contains(LogColumns.SourceId))
					throw new NoSuchColumnException(LogColumns.SourceId);

				_logEntry.SourceId = value;
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
			set
			{
				if (!_columns.Contains(LogColumns.LogLevel))
					throw new NoSuchColumnException(LogColumns.LogLevel);

				_logEntry.LogLevel = value;
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
			set
			{
				if (!_columns.Contains(LogColumns.Timestamp))
					throw new NoSuchColumnException(LogColumns.Timestamp);

				_logEntry.Timestamp = value;
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
			set
			{
				if (!_columns.Contains(LogColumns.ElapsedTime))
					throw new NoSuchColumnException(LogColumns.ElapsedTime);

				_logEntry.ElapsedTime = value;
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
			set
			{
				if (!_columns.Contains(LogColumns.DeltaTime))
					throw new NoSuchColumnException(LogColumns.DeltaTime);

				_logEntry.DeltaTime = value;
			}
		}

		/// <inheritdoc />
		public string Message
		{
			get
			{
				if (!_columns.Contains(LogColumns.Message))
					throw new NoSuchColumnException(LogColumns.Message);

				return _logEntry.Message;
			}
			set
			{
				if (!_columns.Contains(LogColumns.Message))
					throw new NoSuchColumnException(LogColumns.Message);

				_logEntry.Message = value;
			}
		}

		/// <inheritdoc />
		public void SetValue(IColumnDescriptor column, object value)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_logEntry.SetValue(column, value);
		}

		/// <inheritdoc />
		public void SetValue<T>(IColumnDescriptor<T> column, T value)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_logEntry.SetValue(column, value);
		}

		/// <inheritdoc />
		public void CopyFrom(IReadOnlyLogEntry logEntry)
		{
			_logEntry.CopyFrom(logEntry);
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