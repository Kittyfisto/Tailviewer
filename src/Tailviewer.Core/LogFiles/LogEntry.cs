using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     An <see cref="ILogEntry" /> implementation which holds all data in memory (although in a memory intensive fashion).
	/// </summary>
	/// <remarks>
	///     DO NOT use Lists/Arrays of this type to store a bunch of data. Use <see cref="LogEntryArray"/> or <see cref="LogEntryList"/> instead.
	/// </remarks>
	public sealed class LogEntry
		: AbstractLogEntry
	{
		private readonly List<ILogFileColumnDescriptor> _columns;
		private readonly Dictionary<ILogFileColumnDescriptor, object> _values;

		/// <summary>
		/// </summary>
		public LogEntry()
			: this(LogFileColumns.Minimum)
		{}

		/// <summary>
		/// </summary>
		public LogEntry(params ILogFileColumnDescriptor[] columns)
			: this((IEnumerable<ILogFileColumnDescriptor>) columns)
		{
		}

		/// <summary>
		/// </summary>
		public LogEntry(IEnumerable<ILogFileColumnDescriptor> columns)
		{
			_columns = new List<ILogFileColumnDescriptor>(columns);
			_values = new Dictionary<ILogFileColumnDescriptor, object>(_columns.Count);
			foreach (var column in _columns)
			{
				_values.Add(column, column.DefaultValue);
			}
		}

		/// <summary>
		/// </summary>
		public LogEntry(IReadOnlyDictionary<ILogFileColumnDescriptor, object> columnValues)
			: this(columnValues.Keys)
		{
			foreach (var pair in columnValues)
			{
				SetValue(pair.Key, pair.Value);
			}
		}

		/// <inheritdoc />
		public override IReadOnlyList<ILogFileColumnDescriptor> Columns => _columns;

		/// <summary>
		///     Adds a new column to this log entry.
		/// </summary>
		/// <param name="column"></param>
		public void Add(ILogFileColumnDescriptor column)
		{
			Add(column, column.DefaultValue);
			;
		}

		/// <summary>
		///     Adds a new column with the given value to this log entry.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="value"></param>
		public void Add(ILogFileColumnDescriptor column, object value)
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
		public void Add<T>(ILogFileColumnDescriptor<T> column, T value)
		{
			_values.Add(column, value);
			_columns.Add(column);
		}

		/// <inheritdoc />
		public override T GetValue<T>(ILogFileColumnDescriptor<T> column)
		{
			return (T) GetValue((ILogFileColumnDescriptor) column);
		}

		/// <inheritdoc />
		public override bool TryGetValue<T>(ILogFileColumnDescriptor<T> column, out T value)
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
		public override object GetValue(ILogFileColumnDescriptor column)
		{
			object value;
			if (!TryGetValue(column, out value))
				throw new NoSuchColumnException(column);

			return value;
		}

		/// <inheritdoc />
		public override bool TryGetValue(ILogFileColumnDescriptor column, out object value)
		{
			if (!_values.TryGetValue(column, out value))
			{
				value = column.DefaultValue;
				return false;
			}

			return true;
		}

		/// <inheritdoc />
		public override void SetValue(ILogFileColumnDescriptor column, object value)
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
		public override void SetValue<T>(ILogFileColumnDescriptor<T> column, T value)
		{
			SetValue((ILogFileColumnDescriptor)column, (object)value);
		}
	}
}