using System;
using System.Diagnostics.Contracts;

namespace Tailviewer.BusinessLogic.LogTables
{
	/// <summary>
	///     Represents a modification in a <see cref="ILogTable" />.
	/// </summary>
	public struct LogTableModification
		: IEquatable<LogTableModification>
	{
		public static readonly LogTableModification Reset;

		public readonly bool IsInvalidate;
		public readonly LogTableSection Section;
		public readonly ILogTableSchema Schema;

		static LogTableModification()
		{
			Reset = new LogTableModification(LogEntryIndex.Invalid, 0);
		}

		public LogTableModification(ILogTableSchema schema)
		{
			if (schema == null)
				throw new ArgumentNullException(nameof(schema));

			Schema = schema;
			Section = new LogTableSection(LogEntryIndex.Invalid, 0);
			IsInvalidate = false;
		}

		public LogTableModification(LogEntryIndex index, int count, bool isInvalidate = false)
		{
			Section = new LogTableSection(index, count);
			IsInvalidate = isInvalidate;
			Schema = null;
		}

		[Pure]
		public static LogTableModification Invalidate(int firstIndex, int invalidateCount)
		{
			return new LogTableModification(firstIndex, invalidateCount, true);
		}

		public bool Equals(LogTableModification other)
		{
			return Section.Equals(other.Section) && IsInvalidate.Equals(other.IsInvalidate);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogTableModification && Equals((LogTableModification) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Section.GetHashCode() * 397) ^ IsInvalidate.GetHashCode();
			}
		}

		public static bool operator ==(LogTableModification left, LogTableModification right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LogTableModification left, LogTableModification right)
		{
			return !left.Equals(right);
		}

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