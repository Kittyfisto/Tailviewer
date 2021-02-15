using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	/// 
	/// </summary>
	public static class ReadOnlyLogEntryExtensions
	{
		/// <summary>
		///    Compares the two log entries and returns true if they are equal, false other wise.
		/// </summary>
		/// <remarks>
		///    Two log entries are equal if they have the same columns and the same value their respective columns
		/// </remarks>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <returns></returns>
		public static bool Equals(IReadOnlyLogEntry lhs, IReadOnlyLogEntry rhs)
		{
			if (ReferenceEquals(lhs, rhs))
				return true;

			if (ReferenceEquals(lhs, null))
				return false;

			if (ReferenceEquals(rhs, null))
				return false;

			var columns = new HashSet<IColumnDescriptor>(lhs.Columns);
			foreach (var column in rhs.Columns)
			{
				if (!columns.Remove(column))
					return false;

				var rhsValue = rhs.GetValue(column);
				var lhsValue = lhs.GetValue(column);
				if (!Equals(lhsValue, rhsValue))
					return false;
			}

			if (columns.Count > 0)
				return false;

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logEntry"></param>
		/// <returns></returns>
		public static string ToString(IReadOnlyLogEntry logEntry)
		{
			var buffer = new StringBuilder();
			var columns = logEntry.Columns;
			for(int i = 0; i < columns.Count; ++i)
			{
				if (i != 0)
					buffer.Append(", ");

				var column = columns[i];
				var value = logEntry.GetValue(column);
				buffer.AppendFormat(CultureInfo.InvariantCulture, "{0}: {1}", column.Id, value);
			}

			return buffer.ToString();
		}
	}
}