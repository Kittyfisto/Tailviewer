using System;

namespace Tailviewer
{
	/// <summary>
	///     Represents an index to a specific log entry in a log.
	///     Usually there is a 1 to 1 correlation between log entry and log line, but this doesn't need to be the case.
	/// </summary>
	public struct LogEntryIndex
		: IEquatable<LogEntryIndex>
			, IComparable<LogEntryIndex>
	{
		/// <summary>
		///     The value of an invalid index.
		/// </summary>
		public static readonly LogEntryIndex Invalid;

		static LogEntryIndex()
		{
			Invalid = new LogEntryIndex(value: -1);
		}

		/// <summary>
		///     Initializes this index with the given value.
		/// </summary>
		/// <param name="value"></param>
		public LogEntryIndex(int value)
		{
			if (value < -1)
				throw new ArgumentOutOfRangeException(nameof(value));

			Value = value;
		}

		/// <summary>
		///     The numeric value of the index.
		/// </summary>
		public int Value { get; }

		/// <summary>
		///     Tests if this index is <see cref="Invalid" />.
		/// </summary>
		public bool IsInvalid => this == Invalid;

		/// <summary>
		///     Tests if this index is not <see cref="Invalid" />.
		/// </summary>
		public bool IsValid => !IsInvalid;

		/// <summary>
		///     Numerically compares this index to the given one.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(LogEntryIndex other)
		{
			return Value.CompareTo(other.Value);
		}

		/// <summary>
		///     Tests if this and the given index are equal in value.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(LogEntryIndex other)
		{
			return Value == other.Value;
		}

		/// <summary>
		///     Returns the numerical value of the given index.
		/// </summary>
		/// <param name="value"></param>
		public static explicit operator int(LogEntryIndex value)
		{
			return value.Value;
		}

		/// <summary>
		///     Creates an index from the given value.
		/// </summary>
		/// <param name="value"></param>
		public static implicit operator LogEntryIndex(int value)
		{
			return new LogEntryIndex(value);
		}

		/// <summary>
		///     Increments this index.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static LogEntryIndex operator ++(LogEntryIndex value)
		{
			return new LogEntryIndex(value.Value + 1);
		}

		/// <summary>
		///     Tests if this index is equal to the given object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns>true if the given object is a <see cref="LogEntryIndex" /> and is equal in value to this one, false otherwise</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is LogEntryIndex && Equals((LogEntryIndex) obj);
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
		public static bool operator <(LogEntryIndex left, LogEntryIndex right)
		{
			return left.Value < right.Value;
		}

		/// <summary>
		///     Compares the two indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator >(LogEntryIndex left, LogEntryIndex right)
		{
			return left.Value > right.Value;
		}

		/// <summary>
		///     Compares the two indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator <=(LogEntryIndex left, LogEntryIndex right)
		{
			return left.Value <= right.Value;
		}

		/// <summary>
		///     Compares the two indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator >=(LogEntryIndex left, LogEntryIndex right)
		{
			return left.Value >= right.Value;
		}

		/// <summary>
		///     Returns the sum of the two given indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static LogEntryIndex operator +(LogEntryIndex left, int right)
		{
			return new LogEntryIndex(left.Value + right);
		}

		/// <summary>
		///     Returns the sum of the two given indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static LogEntryIndex operator +(int left, LogEntryIndex right)
		{
			return new LogEntryIndex(left + right.Value);
		}

		/// <summary>
		///     Returns the differences of the two given indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static int operator -(LogEntryIndex left, LogEntryIndex right)
		{
			return left.Value - right.Value;
		}

		/// <summary>
		///     Compares the two indices for equality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(LogEntryIndex left, LogEntryIndex right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two indices for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(LogEntryIndex left, LogEntryIndex right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		///     Returns the minimum value of the two given indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static LogEntryIndex Min(LogEntryIndex left, LogEntryIndex right)
		{
			return Math.Min(left.Value, right.Value);
		}

		/// <summary>
		///     Returns the maximum value of the two given indices.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static LogEntryIndex Max(LogEntryIndex left, LogEntryIndex right)
		{
			return Math.Max(left.Value, right.Value);
		}
	}
}