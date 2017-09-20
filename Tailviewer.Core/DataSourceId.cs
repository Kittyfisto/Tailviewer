using System;
using System.Diagnostics.Contracts;

namespace Tailviewer.Core
{
	/// <summary>
	///     Uniquely identifies a data source.
	/// </summary>
	public struct DataSourceId
		: IEquatable<DataSourceId>
	{
		public static readonly DataSourceId Empty = new DataSourceId();

		public bool Equals(DataSourceId other)
		{
			return _value.Equals(other._value);
		}

		public Guid Value => _value;

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is DataSourceId && Equals((DataSourceId) obj);
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		public static bool operator ==(DataSourceId left, DataSourceId right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(DataSourceId left, DataSourceId right)
		{
			return !left.Equals(right);
		}

		private readonly Guid _value;

		public DataSourceId(Guid value)
		{
			_value = value;
		}

		[Pure]
		public static DataSourceId CreateNew()
		{
			return new DataSourceId(Guid.NewGuid());
		}

		public override string ToString()
		{
			return _value.ToString();
		}
	}
}