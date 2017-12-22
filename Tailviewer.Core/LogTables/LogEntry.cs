using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tailviewer.Core.LogTables
{
	/// <summary>
	///     This struct represents one row into a <see cref="ILogTable" />.
	///     A row in a table may cover one or more lines in the actual log file, which is referred to as one
	///     log entry in this application.
	/// </summary>
	/// <remarks>
	///     A row consists of a list of fields where each field represents the cell at the n-th row and m-th column.
	/// </remarks>
	[WillBeRemoved]
	public struct LogEntry
		: IEquatable<LogEntry>
	{
		/// <summary>
		///     The values of the individual fields of this log entry.
		/// </summary>
		public readonly object[] Fields;

		/// <summary>
		///     Initializes this log entry with the given fields.
		/// </summary>
		/// <param name="fields"></param>
		public LogEntry(IEnumerable<object> fields)
		{
			if (fields == null)
				throw new ArgumentNullException(nameof(fields));

			Fields = fields.ToArray();
		}

		/// <summary>
		///     Initializes this log entry with the given fields.
		/// </summary>
		/// <param name="fields"></param>
		public LogEntry(params object[] fields)
		{
			if (fields == null)
				throw new ArgumentNullException(nameof(fields));

			Fields = fields;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			var fields = Fields;
			if (fields != null)
			{
				var builder = new StringBuilder();
				for (var i = 0; i < fields.Length; ++i)
				{
					if (i != 0)
						builder.Append("|");
					builder.AppendFormat("{0}", fields[i] ?? "null");
				}
				return builder.ToString();
			}

			return string.Empty;
		}

		/// <inheritdoc />
		public bool Equals(LogEntry other)
		{
			if (ReferenceEquals(Fields, other.Fields))
				return true;

			if (Fields == null || other.Fields == null)
				return false;

			if (Fields.Length != other.Fields.Length)
				return false;

// ReSharper disable LoopCanBeConvertedToQuery
			for (var i = 0; i < Fields.Length; ++i)
// ReSharper restore LoopCanBeConvertedToQuery
				if (!Equals(Fields[i], other.Fields[i]))
					return false;

			return true;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is LogEntry && Equals((LogEntry) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return 0;
		}

		/// <summary>
		///     Compares the two log entries for equality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(LogEntry left, LogEntry right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two log entries for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(LogEntry left, LogEntry right)
		{
			return !left.Equals(right);
		}
	}
}