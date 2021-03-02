using System;
using System.Collections.Generic;
using Tailviewer.Api;
using Tailviewer.Core.Columns;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     An <see cref="ILogEntry" /> implementation which holds all data in memory (although in a memory intensive fashion).
	/// </summary>
	/// <remarks>
	///     DO NOT use Lists/Arrays of this type to store a bunch of data. Use <see cref="LogBufferArray"/> or <see cref="LogBufferList"/> instead.
	/// </remarks>
	public sealed class LogEntry
		: AbstractLogEntry
	{
		private readonly List<IColumnDescriptor> _columns;
		private readonly Dictionary<IColumnDescriptor, object> _values;

		/// <summary>
		/// </summary>
		public LogEntry()
			: this(GeneralColumns.Minimum)
		{}

		/// <summary>
		/// </summary>
		public LogEntry(params IColumnDescriptor[] columns)
			: this((IEnumerable<IColumnDescriptor>) columns)
		{
		}

		/// <summary>
		/// </summary>
		public LogEntry(IEnumerable<IColumnDescriptor> columns)
		{
			_columns = new List<IColumnDescriptor>(columns);
			_values = new Dictionary<IColumnDescriptor, object>(_columns.Count);
			foreach (var column in _columns)
			{
				_values.Add(column, column.DefaultValue);
			}
		}

		/// <summary>
		/// </summary>
		public LogEntry(IReadOnlyDictionary<IColumnDescriptor, object> columnValues)
			: this(columnValues.Keys)
		{
			foreach (var pair in columnValues)
			{
				SetValue(pair.Key, pair.Value);
			}
		}

		/// <summary>
		/// </summary>
		public LogEntry(IReadOnlyLogEntry other)
			: this(other.Columns)
		{
			CopyFrom(other);
		}

		/// <inheritdoc />
		public override IReadOnlyList<IColumnDescriptor> Columns => _columns;

		/// <summary>
		///     Adds a new column to this log entry.
		/// </summary>
		/// <param name="column"></param>
		public void Add(IColumnDescriptor column)
		{
			Add(column, column.DefaultValue);
			;
		}

		/// <summary>
		///     Adds a new column with the given value to this log entry.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="value"></param>
		public void Add(IColumnDescriptor column, object value)
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
		public void Add<T>(IColumnDescriptor<T> column, T value)
		{
			_values.Add(column, value);
			_columns.Add(column);
		}

		/// <inheritdoc />
		public override T GetValue<T>(IColumnDescriptor<T> column)
		{
			return (T) GetValue((IColumnDescriptor) column);
		}

		/// <inheritdoc />
		public override bool TryGetValue<T>(IColumnDescriptor<T> column, out T value)
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
		public override object GetValue(IColumnDescriptor column)
		{
			object value;
			if (!TryGetValue(column, out value))
				throw new NoSuchColumnException(column);

			return value;
		}

		/// <inheritdoc />
		public override bool TryGetValue(IColumnDescriptor column, out object value)
		{
			if (!_values.TryGetValue(column, out value))
			{
				value = column.DefaultValue;
				return false;
			}

			return true;
		}

		/// <inheritdoc />
		public override void SetValue(IColumnDescriptor column, object value)
		{
			if (!ColumnDescriptorExtensions.IsAssignableFrom(column, value))
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
		public override void SetValue<T>(IColumnDescriptor<T> column, T value)
		{
			SetValue((IColumnDescriptor)column, (object)value);
		}
	}
}