using System;
using System.Diagnostics.Contracts;

namespace Tailviewer
{
	/// <summary>
	///     Represents a relative value.
	/// </summary>
	public struct Percentage
		: IEquatable<Percentage>
			, IComparable<Percentage>
	{
		/// <summary>
		///     100%.
		/// </summary>
		public static readonly Percentage HundredPercent;

		/// <summary>
		///     0%.
		/// </summary>
		public static readonly Percentage Zero;

		static Percentage()
		{
			Zero = new Percentage(value: 0);
			HundredPercent = new Percentage(value: 1);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("{0}%", Percent);
		}

		/// <inheritdoc />
		public bool Equals(Percentage other)
		{
			return RelativeValue.Equals(other.RelativeValue);
		}

		/// <inheritdoc />
		public int CompareTo(Percentage other)
		{
			return RelativeValue.CompareTo(other.RelativeValue);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is Percentage && Equals((Percentage) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return RelativeValue.GetHashCode();
		}

		/// <summary>
		///     Compares the two percentages for equality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(Percentage left, Percentage right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two percentages for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(Percentage left, Percentage right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		///     Returns the sum of the two percentages.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static Percentage operator +(Percentage left, Percentage right)
		{
			return new Percentage(left.RelativeValue + right.RelativeValue);
		}

		/// <summary>
		///     Returns the difference of the two percentages.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static Percentage operator -(Percentage left, Percentage right)
		{
			return new Percentage(left.RelativeValue - right.RelativeValue);
		}

		private Percentage(float value)
		{
			RelativeValue = value;
		}

		/// <summary>
		///     Returns a number that represents this value in percent [0, 100].
		/// </summary>
		public float Percent => RelativeValue * 100;

		/// <summary>
		///     Returns a number that represents this value in absolut [0, 1].
		/// </summary>
		public float RelativeValue { get; }

		/// <summary>
		///     Creates a <see cref="Percentage" /> from the given value in the range of [0, 100].
		/// </summary>
		/// <param name="percent"></param>
		/// <returns></returns>
		public static Percentage FromPercent(float percent)
		{
			return new Percentage(percent / 100);
		}

		/// <summary>
		///     Returns the fraction between <paramref name="current" /> and <paramref cref="count" />.
		/// </summary>
		/// <param name="current"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static Percentage Of(int current, int count)
		{
			return new Percentage((float) current / count);
		}

		/// <summary>
		///     Compares the two percentages.
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <returns></returns>
		public static bool operator >=(Percentage lhs, Percentage rhs)
		{
			return lhs.RelativeValue >= rhs.RelativeValue;
		}

		/// <summary>
		///     Compares the two percentages.
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <returns></returns>
		public static bool operator >(Percentage lhs, Percentage rhs)
		{
			return lhs.RelativeValue > rhs.RelativeValue;
		}

		/// <summary>
		///     Compares the two percentages.
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <returns></returns>
		public static bool operator <=(Percentage lhs, Percentage rhs)
		{
			return lhs.RelativeValue <= rhs.RelativeValue;
		}

		/// <summary>
		///     Compares the two percentages.
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <returns></returns>
		public static bool operator <(Percentage lhs, Percentage rhs)
		{
			return lhs.RelativeValue < rhs.RelativeValue;
		}

		/// <summary>
		///     Tests if the given percentage is set to <see cref="double.NaN" />.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Pure]
		public static bool IsNan(Percentage value)
		{
			if (double.IsNaN(value.RelativeValue))
				return true;

			return false;
		}
	}
}