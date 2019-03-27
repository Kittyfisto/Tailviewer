using System;
using System.Diagnostics.Contracts;
using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer
{
	/// <summary>
	///     A globally unique identifier for a <see cref="IDataSourceAnalyser"/> instance.
	/// </summary>
	public struct AnalyserId
		: IEquatable<AnalyserId>
	{
		/// <summary>
		///     The value for an empty id, representing nothing.
		/// </summary>
		public static readonly AnalyserId Empty = new AnalyserId();

		private readonly Guid _value;

		/// <summary>
		///     Initializes this id.
		/// </summary>
		/// <param name="value"></param>
		public AnalyserId(Guid value)
		{
			_value = value;
		}

		/// <summary>
		///     The underlying value of this id.
		/// </summary>
		public Guid Value => _value;

		/// <inheritdoc />
		public bool Equals(AnalyserId other)
		{
			return Value.Equals(other.Value);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is AnalyserId && Equals((AnalyserId) obj);
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
		public static bool operator ==(AnalyserId left, AnalyserId right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two values for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(AnalyserId left, AnalyserId right)
		{
			return !left.Equals(right);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return _value.ToString();
		}

		/// <summary>
		///     Creates a new id that is guarantueed to be globally unique.
		/// </summary>
		/// <returns></returns>
		[Pure]
		public static AnalyserId CreateNew()
		{
			return new AnalyserId(Guid.NewGuid());
		}
	}
}