using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
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
				if (!_columns.Contains(Core.Columns.RawContent))
					throw new NoSuchColumnException(Core.Columns.RawContent);

				return _logEntry.RawContent;
			}
			set
			{
				if (!_columns.Contains(Core.Columns.RawContent))
					throw new NoSuchColumnException(Core.Columns.RawContent);

				_logEntry.RawContent = value;
			}
		}

		/// <inheritdoc />
		public LogLineIndex Index
		{
			get
			{
				if (!_columns.Contains(Core.Columns.Index))
					throw new NoSuchColumnException(Core.Columns.Index);

				return _logEntry.Index;
			}
			set
			{
				if (!_columns.Contains(Core.Columns.Index))
					throw new NoSuchColumnException(Core.Columns.Index);

				_logEntry.Index = value;
			}
		}

		/// <inheritdoc />
		public LogLineIndex OriginalIndex
		{
			get
			{
				if (!_columns.Contains(Core.Columns.OriginalIndex))
					throw new NoSuchColumnException(Core.Columns.OriginalIndex);

				return _logEntry.OriginalIndex;
			}
			set
			{
				if (!_columns.Contains(Core.Columns.OriginalIndex))
					throw new NoSuchColumnException(Core.Columns.OriginalIndex);

				_logEntry.OriginalIndex = value;
			}
		}

		/// <inheritdoc />
		public LogEntryIndex LogEntryIndex
		{
			get
			{
				if (!_columns.Contains(Core.Columns.LogEntryIndex))
					throw new NoSuchColumnException(Core.Columns.LogEntryIndex);

				return _logEntry.LogEntryIndex;
			}
			set
			{
				if (!_columns.Contains(Core.Columns.LogEntryIndex))
					throw new NoSuchColumnException(Core.Columns.LogEntryIndex);

				_logEntry.LogEntryIndex = value;
			}
		}

		/// <inheritdoc />
		public int LineNumber
		{
			get
			{
				if (!_columns.Contains(Core.Columns.LineNumber))
					throw new NoSuchColumnException(Core.Columns.LineNumber);

				return _logEntry.LineNumber;
			}
			set
			{
				if (!_columns.Contains(Core.Columns.LineNumber))
					throw new NoSuchColumnException(Core.Columns.LineNumber);

				_logEntry.LineNumber = value;
			}
		}

		/// <inheritdoc />
		public int OriginalLineNumber
		{
			get
			{
				if (!_columns.Contains(Core.Columns.OriginalLineNumber))
					throw new NoSuchColumnException(Core.Columns.OriginalLineNumber);

				return _logEntry.OriginalLineNumber;
			}
			set
			{
				if (!_columns.Contains(Core.Columns.OriginalLineNumber))
					throw new NoSuchColumnException(Core.Columns.OriginalLineNumber);

				_logEntry.OriginalLineNumber = value;
			}
		}

		/// <inheritdoc />
		public string OriginalDataSourceName
		{
			get
			{
				if (!_columns.Contains(Core.Columns.OriginalDataSourceName))
					throw new NoSuchColumnException(Core.Columns.OriginalDataSourceName);

				return _logEntry.OriginalDataSourceName;
			}
			set
			{
				if (!_columns.Contains(Core.Columns.OriginalDataSourceName))
					throw new NoSuchColumnException(Core.Columns.OriginalDataSourceName);

				_logEntry.OriginalDataSourceName = value;
			}
		}

		/// <inheritdoc />
		public LogEntrySourceId SourceId
		{
			get
			{
				if (!_columns.Contains(Core.Columns.SourceId))
					throw new NoSuchColumnException(Core.Columns.SourceId);

				return _logEntry.SourceId;
			}
			set
			{
				if (!_columns.Contains(Core.Columns.SourceId))
					throw new NoSuchColumnException(Core.Columns.SourceId);

				_logEntry.SourceId = value;
			}
		}

		/// <inheritdoc />
		public LevelFlags LogLevel
		{
			get
			{
				if (!_columns.Contains(Core.Columns.LogLevel))
					throw new NoSuchColumnException(Core.Columns.LogLevel);

				return _logEntry.LogLevel;
			}
			set
			{
				if (!_columns.Contains(Core.Columns.LogLevel))
					throw new NoSuchColumnException(Core.Columns.LogLevel);

				_logEntry.LogLevel = value;
			}
		}

		/// <inheritdoc />
		public DateTime? Timestamp
		{
			get
			{
				if (!_columns.Contains(Core.Columns.Timestamp))
					throw new NoSuchColumnException(Core.Columns.Timestamp);

				return _logEntry.Timestamp;
			}
			set
			{
				if (!_columns.Contains(Core.Columns.Timestamp))
					throw new NoSuchColumnException(Core.Columns.Timestamp);

				_logEntry.Timestamp = value;
			}
		}

		/// <inheritdoc />
		public TimeSpan? ElapsedTime
		{
			get
			{
				if (!_columns.Contains(Core.Columns.ElapsedTime))
					throw new NoSuchColumnException(Core.Columns.ElapsedTime);

				return _logEntry.ElapsedTime;
			}
			set
			{
				if (!_columns.Contains(Core.Columns.ElapsedTime))
					throw new NoSuchColumnException(Core.Columns.ElapsedTime);

				_logEntry.ElapsedTime = value;
			}
		}

		/// <inheritdoc />
		public TimeSpan? DeltaTime
		{
			get
			{
				if (!_columns.Contains(Core.Columns.DeltaTime))
					throw new NoSuchColumnException(Core.Columns.DeltaTime);

				return _logEntry.DeltaTime;
			}
			set
			{
				if (!_columns.Contains(Core.Columns.DeltaTime))
					throw new NoSuchColumnException(Core.Columns.DeltaTime);

				_logEntry.DeltaTime = value;
			}
		}

		/// <inheritdoc />
		public string Message
		{
			get
			{
				if (!_columns.Contains(Core.Columns.Message))
					throw new NoSuchColumnException(Core.Columns.Message);

				return _logEntry.Message;
			}
			set
			{
				if (!_columns.Contains(Core.Columns.Message))
					throw new NoSuchColumnException(Core.Columns.Message);

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

		/// <inheritdoc />
		public bool Contains(IColumnDescriptor column)
		{
			return _columns.Contains(column);
		}

		#endregion
	}
}