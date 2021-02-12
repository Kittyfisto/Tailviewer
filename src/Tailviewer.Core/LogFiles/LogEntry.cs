using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     An <see cref="ILogEntry" /> implementation which holds all data in memory (although in a memory intensive fashion).
	/// </summary>
	/// <remarks>
	///     DO NOT use Lists/Arrays of this type to store a bunch of data. Use <see cref="LogEntryArray"/> or <see cref="LogEntryList"/> instea.d
	/// </remarks>
	public sealed class LogEntry
		: AbstractLogEntry
	{
		private readonly List<ILogFileColumn> _columns;
		private readonly Dictionary<ILogFileColumn, object> _values;

		/// <summary>
		/// </summary>
		public LogEntry()
			: this(LogFileColumns.Minimum)
		{}

		/// <summary>
		/// </summary>
		public LogEntry(params ILogFileColumn[] columns)
			: this((IEnumerable<ILogFileColumn>) columns)
		{
		}

		/// <summary>
		/// </summary>
		public LogEntry(IEnumerable<ILogFileColumn> columns)
		{
			_columns = new List<ILogFileColumn>(columns);
			_values = new Dictionary<ILogFileColumn, object>(_columns.Count);
			foreach (var column in _columns)
			{
				_values.Add(column, column.DefaultValue);
			}
		}

		/// <summary>
		/// </summary>
		public LogEntry(IReadOnlyDictionary<ILogFileColumn, object> columnValues)
			: this(columnValues.Keys)
		{
			foreach (var pair in columnValues)
			{
				SetValue(pair.Key, pair.Value);
			}
		}

		/// <inheritdoc />
		public override IReadOnlyList<ILogFileColumn> Columns => _columns;

		/// <summary>
		///     Adds a new column to this log entry.
		/// </summary>
		/// <param name="column"></param>
		public void Add(ILogFileColumn column)
		{
			Add(column, column.DefaultValue);
			;
		}

		/// <summary>
		///     Adds a new column with the given value to this log entry.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="value"></param>
		public void Add(ILogFileColumn column, object value)
		{
			_values.Add(column, value);
			_columns.Add(column);
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
		public override T GetValue<T>(ILogFileColumn<T> column)
		{
			return (T) GetValue((ILogFileColumn) column);
		}

		/// <inheritdoc />
		public override bool TryGetValue<T>(ILogFileColumn<T> column, out T value)
		{
			object tmp;
			if (!TryGetValue(column, out tmp))
			{
				value = column.DefaultValue;
				return false;
			}

			value = (T) tmp;
			return true;
		}

		/// <inheritdoc />
		public override object GetValue(ILogFileColumn column)
		{
			object value;
			if (!TryGetValue(column, out value))
				throw new NoSuchColumnException(column);

			return value;
		}

		/// <inheritdoc />
		public override bool TryGetValue(ILogFileColumn column, out object value)
		{
			if (!_values.TryGetValue(column, out value))
			{
				value = column.DefaultValue;
				return false;
			}

			return true;
		}

		/// <inheritdoc />
		public override void SetValue(ILogFileColumn column, object value)
		{
			if (!LogFileColumn.IsAssignableFrom(column, value))
				throw new ArgumentException(string.Format("The value '{0}' of type '{1}' cannot be assigned to column '{2}' of type '{3}'",
				                                          value, value?.GetType(),
				                                          column, column.DataType));

			if (!_columns.Contains(column))
			{
				_columns.Add(column);
				_values.Add(column, value);
			}
			else
			{
				_values[column] = value;
			}
		}

		/// <inheritdoc />
		public override void SetValue<T>(ILogFileColumn<T> column, T value)
		{
			SetValue((ILogFileColumn)column, (object)value);
		}
	}
}