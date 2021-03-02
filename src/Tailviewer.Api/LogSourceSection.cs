using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Tailviewer.Api
{
	/// <summary>
	///     Describes a contiguous section of the <see cref="IReadOnlyLogEntry"/>s of an <see cref="ILogSource"/>.
	/// </summary>
	public struct LogSourceSection
		: IEquatable<LogSourceSection>
		, IReadOnlyList<LogLineIndex>
	{
		/// <summary>
		/// The number of log entries in this section
		/// </summary>
		public int Count;

		/// <summary>
		/// The index of the first log entry in this section.
		/// </summary>
		public readonly LogLineIndex Index;

		/// <summary>
		///     Creates a new section which represents the given
		///     section [<paramref name="index" />, <paramref name="index" /> + <paramref name="count" />).
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		[DebuggerStepThrough]
		public LogSourceSection(LogLineIndex index, int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));

			Index = index;
			Count = count;
		}

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
		public bool Equals(LogSourceSection other)
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
			return $"[{Index}, #{Count}]";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		[Pure]
		public LogSourceSection? Intersect(LogSourceSection other)
		{
			var greatestStart = Index > other.Index ? Index : other.Index;
			int smallestEnd = LastIndex < other.LastIndex ? LastIndex : other.LastIndex;

			//no intersection
			if (greatestStart > smallestEnd)
			{
				return null;
			}

			return new LogSourceSection(greatestStart, smallestEnd - greatestStart + 1);
		}

		/// <summary>
		///     Creates a new section which spawns from the lowest <see cref="Index" />
		///     of the given two sections to the greatest <see cref="LastIndex" />.
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <returns></returns>
		public static LogSourceSection MinimumBoundingLine(LogSourceSection lhs, LogSourceSection rhs)
		{
			LogLineIndex minIndex = LogLineIndex.Min(lhs.Index, rhs.Index);
			LogLineIndex maxIndex = LogLineIndex.Max(lhs.Index + lhs.Count, rhs.Index + rhs.Count);
			int count = maxIndex - minIndex;

			return new LogSourceSection(minIndex, count);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogSourceSection && Equals((LogSourceSection) obj);
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
		public static bool operator ==(LogSourceSection left, LogSourceSection right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two given sections for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(LogSourceSection left, LogSourceSection right)
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
	}
}