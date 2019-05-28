using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Basically a handle to a specific portion of the logfile.
	///     Call <see cref="ILogFile.GetSection" /> to actually obtain the data for that portion.
	/// </summary>
	/// <remarks>
	/// TODO: Rename to LogEntryRange as it will soon describe a range of log entries, see #140
	/// </remarks>
	public struct LogFileSection
		: IEquatable<LogFileSection>
		, IReadOnlyList<LogLineIndex>
	{
		/// <summary>
		/// This value is used to represent a section which does not exist.
		/// Is used when a log file's source no longer exists, for example.
		/// </summary>
		public static readonly LogFileSection Reset;

		/// <summary>
		/// The number of lines in this section
		/// </summary>
		public int Count;

		/// <summary>
		/// The index of the first line in this section.
		/// </summary>
		public readonly LogLineIndex Index;

		/// <summary>
		/// Whether or not this section represents an addition (=False)
		/// or an invalidation (i.e. Removal, True).
		/// </summary>
		public readonly bool IsInvalidate;

		static LogFileSection()
		{
			Reset = new LogFileSection(LogLineIndex.Invalid, 0);
		}

		/// <summary>
		///     Creates a new section which represents an invalidation of the given
		///     section [<paramref name="index" />, <paramref name="index" /> + <paramref name="count" />).
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public static LogFileSection Invalidate(LogLineIndex index, int count)
		{
			return new LogFileSection(index, count, true);
		}

		[DebuggerStepThrough]
		private LogFileSection(LogLineIndex index, int count, bool isInvalidate)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));

			Index = index;
			Count = count;
			IsInvalidate = isInvalidate;
		}

		/// <summary>
		///     Creates a new section which represents the given
		///     section [<paramref name="index" />, <paramref name="index" /> + <paramref name="count" />).
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		[DebuggerStepThrough]
		public LogFileSection(LogLineIndex index, int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));

			Index = index;
			Count = count;
			IsInvalidate = false;
		}

		/// <summary>
		///     Whether or not this section represents a complete reset of the source.
		/// </summary>
		public bool IsReset => this == Reset;

		/// <summary>
		///     The last valid index of this section.
		///     Is always <see cref="Index" /> + <see cref="Count" /> - 1.
		/// </summary>
		public int LastIndex => Index + Count - 1;

		/// <summary>
		///     Tests if this and the given section represent the same section: [Index, Index+Count)
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(LogFileSection other)
		{
			return Index == other.Index && Count == other.Count;
		}

		/// <summary>
		///     Tests if the given index is part of this section.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		[Pure]
		public bool Contains(LogLineIndex index)
		{
			return index >= Index &&
			       index < Index + Count;
		}

		/// <summary>
		///     Tests if the given index is greater than <see cref="LastIndex" />.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		[Pure]
		public bool IsEndOfSection(LogLineIndex index)
		{
			return index >= Index + Count;
		}

		/// <inheritdoc />
		public IEnumerator<LogLineIndex> GetEnumerator()
		{
			return new LogFileSectionEnumerator(Index, Count);
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

		/// <summary>
		///     Creates a new section which spawns from the lowest <see cref="Index" />
		///     of the given two sections to the greatest <see cref="LastIndex" />.
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <returns></returns>
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

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		///     Compares the two given sections for equality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(LogFileSection left, LogFileSection right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two given sections for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(LogFileSection left, LogFileSection right)
		{
			return !left.Equals(right);
		}

		sealed class LogFileSectionEnumerator
			: IEnumerator<LogLineIndex>
		{
			private readonly LogLineIndex _start;
			private readonly int _count;
			private int _i;

			public LogFileSectionEnumerator(LogLineIndex start, int count)
			{
				_start = start;
				_count = count;
				Reset();
			}

			public void Dispose()
			{}

			public bool MoveNext()
			{
				if (++_i >= _count)
					return false;

				return true;
			}

			public void Reset()
			{
				_i = -1;
			}

			public LogLineIndex Current
			{
				get
				{
					if (_i < 0 || _i >= _count)
						throw new InvalidOperationException();

					return _start + _i;
				}
			}

			object IEnumerator.Current => Current;
		}

		int IReadOnlyCollection<LogLineIndex>.Count => Count;

		/// <inheritdoc />
		public LogLineIndex this[int index]
		{
			get
			{
				if (index < 0 || index >= Count)
					throw new IndexOutOfRangeException();

				return Index + index;
			}
		}

		/// <summary>
		/// Splits up this section into multiple ones if it:
		/// - Is neither reset, nor invalidates
		/// - Appends more than the given amount of rows
		/// </summary>
		/// <param name="maxCount"></param>
		/// <returns></returns>
		public IEnumerable<LogFileSection> Split(int maxCount)
		{
			if (maxCount <= 0)
				throw new ArgumentException("You need to specify a maximum count greater than 0!");

			if (IsInvalidate || IsReset || Count == 0)
			{
				return new[] {this};
			}

			return SplitAppend(maxCount);
		}

		private IEnumerable<LogFileSection> SplitAppend(int maxCount)
		{
			var nextIndex = Index;
			var remainingCount = Count;
			while (remainingCount > maxCount)
			{
				yield return new LogFileSection(nextIndex, maxCount);

				nextIndex += maxCount;
				remainingCount -= maxCount;
			}

			if (remainingCount > 0)
			{
				yield return new LogFileSection(nextIndex, remainingCount);
			}
		}
	}
}