using System;
using System.Diagnostics.Contracts;

namespace Tailviewer.Core.LogTables
{
	/// <summary>
	///     Represents a modification in a <see cref="ILogTable" />.
	/// </summary>
	public struct LogTableModification
		: IEquatable<LogTableModification>
	{
		/// <summary>
		/// 
		/// </summary>
		public static readonly LogTableModification Reset;

		/// <summary>
		/// 
		/// </summary>
		public readonly bool IsInvalidate;

		/// <summary>
		/// 
		/// </summary>
		public readonly LogTableSection Section;

		/// <summary>
		/// 
		/// </summary>
		public readonly ILogTableSchema Schema;

		static LogTableModification()
		{
			Reset = new LogTableModification(LogEntryIndex.Invalid, 0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="schema"></param>
		public LogTableModification(ILogTableSchema schema)
		{
			if (schema == null)
				throw new ArgumentNullException(nameof(schema));

			Schema = schema;
			Section = new LogTableSection(LogEntryIndex.Invalid, 0);
			IsInvalidate = false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <param name="isInvalidate"></param>
		public LogTableModification(LogEntryIndex index, int count, bool isInvalidate = false)
		{
			Section = new LogTableSection(index, count);
			IsInvalidate = isInvalidate;
			Schema = null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="firstIndex"></param>
		/// <param name="invalidateCount"></param>
		/// <returns></returns>
		[Pure]
		public static LogTableModification Invalidate(int firstIndex, int invalidateCount)
		{
			return new LogTableModification(firstIndex, invalidateCount, true);
		}

		/// <inheritdoc />
		public bool Equals(LogTableModification other)
		{
			return Section.Equals(other.Section) && IsInvalidate.Equals(other.IsInvalidate);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogTableModification && Equals((LogTableModification) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return (Section.GetHashCode() * 397) ^ IsInvalidate.GetHashCode();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(LogTableModification left, LogTableModification right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(LogTableModification left, LogTableModification right)
		{
			return !left.Equals(right);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			if (Section.Index == LogEntryIndex.Invalid && Section.Count == 0)
				return "Reset";

			if (Schema != null)
				return string.Format("Schema changed to {0}", Schema);

			if (IsInvalidate)
				return string.Format("Invalidated {0}", Section);

			return string.Format("Changed {0}", Section);
		}
	}
}