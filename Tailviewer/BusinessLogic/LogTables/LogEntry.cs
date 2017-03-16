using System;
using System.Collections.Generic;
using System.Linq;

namespace Tailviewer.BusinessLogic.LogTables
{
	/// <summary>
	///     This struct represents one row into a <see cref="ILogTable" />.
	///     A row in a table may cover one or more lines in the actual log file, which is referred to as one
	///     log entry in this application.
	/// </summary>
	/// <remarks>
	///     A row consists of a list of fields where each field represents the cell at the n-th row and m-th column.
	/// </remarks>
	public struct LogEntry
		: IEquatable<LogEntry>
	{
		public readonly object[] Fields;

		public LogEntry(IEnumerable<object> fields)
		{
			if (fields == null)
				throw new ArgumentNullException(nameof(fields));

			Fields = fields.ToArray();
		}

		public LogEntry(params object[] fields)
		{
			if (fields == null)
				throw new ArgumentNullException(nameof(fields));

			Fields = fields;
		}

		public override string ToString()
		{
			var fields = Fields;
			if (fields != null)
				return string.Join(", ", fields);

			return string.Empty;
		}

		public bool Equals(LogEntry other)
		{
			if (ReferenceEquals(Fields, other.Fields))
				return true;

			if (Fields == null || other.Fields == null)
				return false;

			if (Fields.Length != other.Fields.Length)
				return false;

// ReSharper disable LoopCanBeConvertedToQuery
			for (int i = 0; i < Fields.Length; ++i)
// ReSharper restore LoopCanBeConvertedToQuery
			{
				if (!Equals(Fields[i], other.Fields[i]))
					return false;
			}

			return true;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogEntry && Equals((LogEntry) obj);
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public static bool operator ==(LogEntry left, LogEntry right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LogEntry left, LogEntry right)
		{
			return !left.Equals(right);
		}
	}
}