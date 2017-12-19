using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Represents a log entry which cannot be modified after it's been constructed.
	/// </summary>
	/// <remarks>
	///     Don't keep many instances of this type in memory as it wastes a lot thereof.
	/// </remarks>
	public sealed class ReadOnlyLogEntry
		: IReadOnlyLogEntry
	{
		private readonly IReadOnlyDictionary<ILogFileColumn, object> _values;

		/// <summary>
		/// </summary>
		/// <param name="values"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public ReadOnlyLogEntry(IReadOnlyDictionary<ILogFileColumn, object> values)
		{
			if (values == null)
				throw new ArgumentNullException(nameof(values));

			_values = values;
		}

		/// <inheritdoc />
		public string RawContent => GetColumnValue(LogFileColumns.RawContent);

		/// <inheritdoc />
		public LogLineIndex Index => GetColumnValue(LogFileColumns.Index);

		/// <inheritdoc />
		public LogLineIndex OriginalIndex => GetColumnValue(LogFileColumns.OriginalIndex);

		/// <inheritdoc />
		public LogEntryIndex LogEntryIndex => GetColumnValue(LogFileColumns.LogEntryIndex);

		/// <inheritdoc />
		public int LineNumber => GetColumnValue(LogFileColumns.LineNumber);

		/// <inheritdoc />
		public int OriginalLineNumber => GetColumnValue(LogFileColumns.OriginalLineNumber);

		/// <inheritdoc />
		public LevelFlags LogLevel => GetColumnValue(LogFileColumns.LogLevel);

		/// <inheritdoc />
		public DateTime? Timestamp => GetColumnValue(LogFileColumns.Timestamp);

		/// <inheritdoc />
		public TimeSpan? ElapsedTime => GetColumnValue(LogFileColumns.ElapsedTime);

		/// <inheritdoc />
		public TimeSpan? DeltaTime => GetColumnValue(LogFileColumns.DeltaTime);

		/// <inheritdoc />
		public T GetColumnValue<T>(ILogFileColumn<T> column)
		{
			return (T) GetColumnValue((ILogFileColumn) column);
		}

		/// <inheritdoc />
		public object GetColumnValue(ILogFileColumn column)
		{
			object value;
			if (!_values.TryGetValue(column, out value))
				throw new NoSuchColumnException(column);

			return value;
		}

		/// <inheritdoc />
		public IReadOnlyList<ILogFileColumn> Columns => _values.Keys.ToList();

		/// <summary>
		/// </summary>
		/// <param name="columns"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="ArgumentException"></exception>
		[Pure]
		public static IReadOnlyLogEntry Create(IReadOnlyList<ILogFileColumn> columns,
		                                       IReadOnlyList<object> values)
		{
			if (columns == null)
				throw new ArgumentNullException(nameof(columns));
			if (values == null)
				throw new ArgumentNullException(nameof(values));
			if (columns.Count != values.Count)
				throw new ArgumentOutOfRangeException();

			var valuesPerColumn = new Dictionary<ILogFileColumn, object>(columns.Count);
			for (var i = 0; i < columns.Count; ++i)
			{
				var column = columns[i];
				var value = values[i];
				if (value != null && !column.DataType.IsInstanceOfType(value))
					throw new ArgumentException(string.Format("Expected value '{0}' to be of type '{1}' but it is not",
					                                          value,
					                                          column.DataType));

				valuesPerColumn.Add(columns[i], values[i]);
			}
			return new ReadOnlyLogEntry(valuesPerColumn);
		}
	}
}