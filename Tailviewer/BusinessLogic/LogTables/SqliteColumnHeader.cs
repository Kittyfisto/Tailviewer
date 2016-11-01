using System;

namespace Tailviewer.BusinessLogic.LogTables
{
	public sealed class SQLiteColumnHeader
		: IColumnHeader
	{
		private readonly SQLiteDataType _databaseType;
		private readonly string _name;

		public SQLiteColumnHeader(string name, string databaseType)
			: this(name, TryGetDatabaseType(databaseType))
		{
		}

		public SQLiteColumnHeader(string name, SQLiteDataType databaseType)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			_name = name;
			_databaseType = databaseType;
		}

		public SQLiteDataType DatabaseType
		{
			get { return _databaseType; }
		}

		public string Name
		{
			get { return _name; }
		}

		private static SQLiteDataType TryGetDatabaseType(string databaseType)
		{
			switch (databaseType)
			{
				case "DATETIME":
					return SQLiteDataType.DateTime;
				case "TEXT":
					return SQLiteDataType.Text;
				default:
					return SQLiteDataType.Other;
			}
		}

		private bool Equals(SQLiteColumnHeader other)
		{
			return string.Equals(_name, other._name) && Equals(_databaseType, other._databaseType);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is SQLiteColumnHeader && Equals((SQLiteColumnHeader) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_name.GetHashCode()*397) ^
				       _databaseType.GetHashCode();
			}
		}

		public static bool operator ==(SQLiteColumnHeader left, SQLiteColumnHeader right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(SQLiteColumnHeader left, SQLiteColumnHeader right)
		{
			return !Equals(left, right);
		}

		public override string ToString()
		{
			return string.Format("{0} {1}", _name, _databaseType);
		}
	}
}