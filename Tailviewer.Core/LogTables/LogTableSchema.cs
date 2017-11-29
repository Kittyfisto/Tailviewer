using System.Collections.Generic;
using System.Linq;

namespace Tailviewer.Core.LogTables
{
	internal sealed class LogTableSchema : ILogTableSchema
	{
		private readonly IColumnHeader[] _columns;

		public LogTableSchema(IEnumerable<IColumnHeader> columns)
		{
			_columns = columns.ToArray();
		}

		public IReadOnlyCollection<IColumnHeader> ColumnHeaders => _columns;
	}
}