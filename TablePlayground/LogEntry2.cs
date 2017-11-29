using System;
using System.Collections.Generic;

namespace TablePlayground
{
	public sealed class LogEntry2
	{
		private readonly IReadOnlyDictionary<ILogColumn, object> _columns;

		public LogEntry2(IReadOnlyDictionary<ILogColumn, object> columns)
		{
			if (columns == null)
				throw new ArgumentNullException(nameof(columns));

			_columns = columns;
		}

		public T GetValue<T>(ILogColumn<T> column)
		{
			object value;
			if (!_columns.TryGetValue(column, out value))
				throw new NotImplementedException();

			return (T) value;
		}
	}
}