namespace Tailviewer.BusinessLogic.LogTables
{
	public sealed class SqliteColumnHeader
		: IColumnHeader
	{
		private readonly string _databaseType;
		private readonly string _name;

		public SqliteColumnHeader(string name, string databaseType)
		{
			_name = name;
			_databaseType = databaseType;
		}

		public string Name
		{
			get { return _name; }
		}

		private bool Equals(SqliteColumnHeader other)
		{
			return string.Equals(_name, other._name) && string.Equals(_databaseType, other._databaseType);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is SqliteColumnHeader && Equals((SqliteColumnHeader) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((_name != null ? _name.GetHashCode() : 0)*397) ^ (_databaseType != null ? _databaseType.GetHashCode() : 0);
			}
		}

		public static bool operator ==(SqliteColumnHeader left, SqliteColumnHeader right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(SqliteColumnHeader left, SqliteColumnHeader right)
		{
			return !Equals(left, right);
		}

		public override string ToString()
		{
			return string.Format("{0} {1}", _name, _databaseType);
		}
	}
}