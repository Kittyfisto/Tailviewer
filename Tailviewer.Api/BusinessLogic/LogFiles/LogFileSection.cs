using System;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Basically a handle to a specific portion of the logfile.
	///     Call <see cref="ILogFile.GetSection" /> to actually obtain the data for that portion.
	/// </summary>
	public struct LogFileSection
		: IEquatable<LogFileSection>
	{
		public static readonly LogFileSection Reset;

		public readonly int Count;
		public readonly LogLineIndex Index;
		public readonly bool IsInvalidate;

		static LogFileSection()
		{
			Reset = new LogFileSection(LogLineIndex.Invalid, 0);
		}

		public static LogFileSection Invalidate(LogLineIndex index, int count)
		{
			return new LogFileSection(index, count, true);
		}

		private LogFileSection(LogLineIndex index, int count, bool isInvalidate)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));

			Index = index;
			Count = count;
			IsInvalidate = isInvalidate;
		}

		public LogFileSection(LogLineIndex index, int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));

			Index = index;
			Count = count;
			IsInvalidate = false;
		}

		public bool IsReset => this == Reset;

		public int LastIndex => Index + Count - 1;

		/// <summary>
		/// Tests if this and the given section represent the same section: [Index, Index+Count)
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(LogFileSection other)
		{
			return Index == other.Index && Count == other.Count;
		}

		public bool IsEndOfSection(LogLineIndex index)
		{
			return index >= Index + Count;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			if (Index == LogLineIndex.Invalid && Count == 0)
				return "Reset";

			if (IsInvalidate)
				return string.Format("Invalidated [{0}, #{1}]", Index, Count);

			return string.Format("Changed [{0}, #{1}]", Index, Count);
		}

		public static LogFileSection MinimumBoundingLine(LogFileSection lhs, LogFileSection rhs)
		{
			LogLineIndex minIndex = LogLineIndex.Min(lhs.Index, rhs.Index);
			LogLineIndex maxIndex = LogLineIndex.Max(lhs.Index + lhs.Count, rhs.Index + rhs.Count);
			int count = maxIndex - minIndex;

			return new LogFileSection(minIndex, count);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogFileSection && Equals((LogFileSection) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return ((int) Index*397) ^ Count;
			}
		}

		public static bool operator ==(LogFileSection left, LogFileSection right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LogFileSection left, LogFileSection right)
		{
			return !left.Equals(right);
		}
	}
}