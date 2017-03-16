using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Tailviewer.BusinessLogic.LogTables
{
	public sealed class SQLiteSchema
		: ILogTableSchema
	{
		private readonly ReadOnlyCollection<SQLiteColumnHeader> _columnHeaders;
		private readonly string _tableName;

		public SQLiteSchema(string tableName)
		{
			if (tableName == null)
				throw new ArgumentNullException(nameof(tableName));

			_tableName = tableName;
			_columnHeaders = new ReadOnlyCollection<SQLiteColumnHeader>(new List<SQLiteColumnHeader>());
		}

		public SQLiteSchema(string tableName, params SQLiteColumnHeader[] columnHeaders)
			: this(tableName, (IEnumerable<SQLiteColumnHeader>)columnHeaders)
		{}

		public SQLiteSchema(string tableName, IEnumerable<SQLiteColumnHeader> columnHeaders)
		{
			if (tableName == null)
				throw new ArgumentNullException(nameof(tableName));
			if (columnHeaders == null)
				throw new ArgumentNullException(nameof(columnHeaders));

			_tableName = tableName;
			_columnHeaders = new ReadOnlyCollection<SQLiteColumnHeader>(new List<SQLiteColumnHeader>(columnHeaders));
		}

		public string TableName
		{
			get { return _tableName; }
		}

		public IReadOnlyCollection<IColumnHeader> ColumnHeaders
		{
			get { return _columnHeaders; }
		}

		public override string ToString()
		{
			return string.Format("{0} [{1}]",
			                     _tableName,
			                     string.Join(", ", _columnHeaders));
		}

		private bool Equals(SQLiteSchema schema)
		{
			ReadOnlyCollection<SQLiteColumnHeader> columns = schema._columnHeaders;
			if (columns.Count != _columnHeaders.Count)
				return false;

			for (int i = 0; i < columns.Count; ++i)
			{
				SQLiteColumnHeader column = columns[i];
				SQLiteColumnHeader columnHeader = _columnHeaders[i];
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
			if (!(obj is SQLiteSchema))
				return false;

			return Equals((SQLiteSchema) obj);
		}
	}
}