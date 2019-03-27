using System;
using System.Diagnostics.Contracts;

namespace Tailviewer
{
	/// <summary>
	///     A globally unique identifier for an analysis.
	/// </summary>
	public struct AnalysisId
		: IEquatable<AnalysisId>
	{
		/// <summary>
		///     The value for an empty id, representing nothing.
		/// </summary>
		public static readonly AnalysisId Empty = new AnalysisId();

		/// <summary>
		///     Initializes this id.
		/// </summary>
		/// <param name="value"></param>
		public AnalysisId(Guid value)
		{
			Value = value;
		}

		/// <summary>
		///     The underlying value of this id.
		/// </summary>
		public Guid Value { get; }

		/// <inheritdoc />
		public bool Equals(AnalysisId other)
		{
			return Value.Equals(other.Value);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is AnalysisId && Equals((AnalysisId) obj);
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
		public static bool operator ==(AnalysisId left, AnalysisId right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two values for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(AnalysisId left, AnalysisId right)
		{
			return !left.Equals(right);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return Value.ToString();
		}

		/// <summary>
		///     Creates a new id that is guarantueed to be globally unique.
		/// </summary>
		/// <returns></returns>
		[Pure]
		public static AnalysisId CreateNew()
		{
			return new AnalysisId(Guid.NewGuid());
		}
	}
}