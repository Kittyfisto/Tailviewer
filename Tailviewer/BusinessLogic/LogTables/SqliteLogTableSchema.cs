using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Tailviewer.BusinessLogic.LogTables
{
	public sealed class SqliteLogTableSchema
		: ILogTableSchema
	{
		private readonly ReadOnlyCollection<SqliteColumnHeader> _columnHeaders;

		public SqliteLogTableSchema()
		{
			_columnHeaders = new ReadOnlyCollection<SqliteColumnHeader>(new List<SqliteColumnHeader>());
		}

		public SqliteLogTableSchema(IList<SqliteColumnHeader> columnHeaders)
		{
			_columnHeaders = new ReadOnlyCollection<SqliteColumnHeader>(columnHeaders);
		}

		public IReadOnlyCollection<IColumnHeader> ColumnHeaders
		{
			get { return _columnHeaders; }
		}

		public override string ToString()
		{
			return string.Join(", ", _columnHeaders);
		}

		private bool Equals(SqliteLogTableSchema logTableSchema)
		{
			var columns = logTableSchema._columnHeaders;
			if (columns.Count != _columnHeaders.Count)
				return false;

			for (int i = 0; i < columns.Count; ++i)
			{
				var column = columns[i];
				var columnHeader = _columnHeaders[i];
				if (!Equals(column, columnHeader))
					return false;
			}

			return true;
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is SqliteLogTableSchema))
				return false;

			return Equals((SqliteLogTableSchema) obj);
		}
	}
}