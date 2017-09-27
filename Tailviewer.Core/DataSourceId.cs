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
		/// <summary>
		///     The value for an empty id, representing nothing.
		/// </summary>
		public static readonly DataSourceId Empty = new DataSourceId();

		/// <inheritdoc />
		public bool Equals(DataSourceId other)
		{
			return Value.Equals(other.Value);
		}

		/// <summary>
		///     The underlying value of this id.
		/// </summary>
		public Guid Value { get; }

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is DataSourceId && Equals((DataSourceId) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		/// <summary>
		///     Compares the two values for equality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(DataSourceId left, DataSourceId right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two values for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(DataSourceId left, DataSourceId right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		///     Initializes this id.
		/// </summary>
		/// <param name="value"></param>
		public DataSourceId(Guid value)
		{
			Value = value;
		}

		/// <summary>
		///     Creates a new id that is guarantueed to be globally unique.
		/// </summary>
		/// <returns></returns>
		[Pure]
		public static DataSourceId CreateNew()
		{
			return new DataSourceId(Guid.NewGuid());
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return Value.ToString();
		}
	}
}