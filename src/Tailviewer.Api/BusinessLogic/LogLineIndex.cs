using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	///     Represents the index to a specific line in a log.
	///     Lines are number from 0 onwards.
	/// </summary>
	public struct LogLineIndex
		: IEquatable<LogLineIndex>
		, IComparable<LogLineIndex>
	{
		/// <summary>
		///     Value for an invalid index.
		/// </summary>
		public static readonly LogLineIndex Invalid;

		static LogLineIndex()
		{
			Invalid = new LogLineIndex(value: -1);
		}

		/// <summary>
		///     Initializes this index with the given value.
		/// </summary>
		/// <param name="value"></param>
		public LogLineIndex(int value)
		{
			Value = value;
		}

		/// <summary>
		///     The numerical value of this index.
		/// </summary>
		public int Value { get; }

		/// <summary>
		///     Whether or not this index is invalid.
		/// </summary>
		public bool IsInvalid => this == Invalid;

		/// <summary>
		///     Whether or not this index is valid.
		/// </summary>
		public bool IsValid => !IsInvalid;

		/// <inheritdoc />
		public int CompareTo(LogLineIndex other)
		{
			return Value.CompareTo(other.Value);
		}

		/// <inheritdoc />
		public bool Equals(LogLineIndex other)
		{
			if (Value < 0 && other.Value < 0)
				return true;

			return Value == other.Value;
		}

		/// <summary>
		///     Converts the given index to an integer.
		/// </summary>
		/// <param name="value"></param>
		public static explicit operator int(LogLineIndex value)
		{
			return value.Value;
		}

		/// <summary>
		///     Converts the given integer to an index.
		/// </summary>
		/// <param name="value"></param>
		public static implicit operator LogLineIndex(int value)
		{
			return new LogLineIndex(value);
		}

		/// <summary>
		///     Increments the index by one.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static LogLineIndex operator ++(LogLineIndex value)
		{
			return new LogLineIndex(value.Value + 1);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is LogLineIndex && Equals((LogLineIndex) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Value;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			if (this == Invalid)
				return "Invalid";

			return string.Format("#{0}", Value);
		}

		/// <summary>
		///     Compares the two indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator <(LogLineIndex left, LogLineIndex right)
		{
			return left.Value < right.Value;
		}

		/// <summary>
		///     Compares the two indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator >(LogLineIndex left, LogLineIndex right)
		{
			return left.Value > right.Value;
		}

		/// <summary>
		///     Compares the two indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator <=(LogLineIndex left, LogLineIndex right)
		{
			return left.Value <= right.Value;
		}

		/// <summary>
		///     Compares the two indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator >=(LogLineIndex left, LogLineIndex right)
		{
			return left.Value >= right.Value;
		}

		/// <summary>
		///     Returns the sum of the two given indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static LogLineIndex operator +(LogLineIndex left, int right)
		{
			return new LogLineIndex(left.Value + right);
		}

		/// <summary>
		///     Returns the sum of the two given indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static LogLineIndex operator +(int left, LogLineIndex right)
		{
			return new LogLineIndex(left + right.Value);
		}

		/// <summary>
		///     Returns the differences of the two given indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static int operator -(LogLineIndex left, LogLineIndex right)
		{
			return left.Value - right.Value;
		}

		/// <summary>
		///     Compares the two indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(LogLineIndex left, LogLineIndex right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(LogLineIndex left, LogLineIndex right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		///     Returns the minimum of the two given indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="rigth"></param>
		/// <returns></returns>
		public static LogLineIndex Min(LogLineIndex left, LogLineIndex rigth)
		{
			return Math.Min(left.Value, rigth.Value);
		}

		/// <summary>
		///     Returns the maximum of the two given indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static LogLineIndex Max(LogLineIndex left, LogLineIndex right)
		{
			return Math.Max(left.Value, right.Value);
		}

		/// <summary>
		///     Builds and returns the list of indices of the given range.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static IEnumerable<LogLineIndex> Range(LogLineIndex from, LogLineIndex to)
		{
			if (from > to)
			{
				var tmp = to;
				to = from;
				from = tmp;
			}

			var count = to - from + 1;
			var values = new List<LogLineIndex>(count);
			for (var i = 0; i < count; ++i)
				values.Add(from + i);
			return values;
		}
	}
}