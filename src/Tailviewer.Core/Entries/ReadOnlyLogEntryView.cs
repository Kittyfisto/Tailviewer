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
				if (!_columns.Contains(Core.Columns.RawContent))
					throw new NoSuchColumnException(Core.Columns.RawContent);

				return _logEntry.RawContent;
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

		/// <inheritdoc />
		public override string ToString()
		{
			return ReadOnlyLogEntryExtensions.ToString(this);
		}

		#endregion
	}
}