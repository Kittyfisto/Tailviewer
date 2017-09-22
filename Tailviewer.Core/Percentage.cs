using System;

namespace Tailviewer.Core
{
	public struct Percentage
		: IEquatable<Percentage>
			, IComparable<Percentage>
	{
		public static readonly Percentage HundredPercent;
		public static readonly Percentage Zero;

		static Percentage()
		{
			Zero = new Percentage(0);
			HundredPercent = new Percentage(1);
		}

		public override string ToString()
		{
			return string.Format("{0}%", Percent);
		}

		public bool Equals(Percentage other)
		{
			return RelativeValue.Equals(other.RelativeValue);
		}

		public int CompareTo(Percentage other)
		{
			return RelativeValue.CompareTo(other.RelativeValue);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Percentage && Equals((Percentage) obj);
		}

		public override int GetHashCode()
		{
			return RelativeValue.GetHashCode();
		}

		public static bool operator ==(Percentage left, Percentage right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Percentage left, Percentage right)
		{
			return !left.Equals(right);
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

		public static Percentage Of(int current, int count)
		{
			return new Percentage((float) current / count);
		}

		public static bool operator >=(Percentage lhs, Percentage rhs)
		{
			return lhs.RelativeValue >= rhs.RelativeValue;
		}

		public static bool operator >(Percentage lhs, Percentage rhs)
		{
			return lhs.RelativeValue > rhs.RelativeValue;
		}

		public static bool operator <=(Percentage lhs, Percentage rhs)
		{
			return lhs.RelativeValue <= rhs.RelativeValue;
		}

		public static bool operator <(Percentage lhs, Percentage rhs)
		{
			return lhs.RelativeValue < rhs.RelativeValue;
		}
	}
}