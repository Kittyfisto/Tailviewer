using System.Collections.Generic;
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
		: AbstractLogEntry
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
		/// 
		/// </summary>
		public LogEntry2(params ILogFileColumn[] columns)
			: this((IEnumerable<ILogFileColumn>)columns)
		{}

		/// <summary>
		/// 
		/// </summary>
		public LogEntry2(IEnumerable<ILogFileColumn> columns)
			: this()
		{
			foreach (var column in columns)
			{
				Add(column);
			}
		}

		/// <summary>
		///     Adds a new column to this log entry.
		/// </summary>
		/// <param name="column"></param>
		public void Add(ILogFileColumn column)
		{
			_values.Add(column, column.DefaultValue);
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
		public override IReadOnlyList<ILogFileColumn> Columns => _columns;

		/// <inheritdoc />
		public override void SetValue(ILogFileColumn column, object value)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_values[column] = value;
		}

		/// <inheritdoc />
		public override void SetValue<T>(ILogFileColumn<T> column, T value)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_values[column] = value;
		}
	}
}