using System.Collections.Generic;

namespace Tailviewer.Core.Filters
{
	/// <summary>
	/// A filter responsible for filtering out log lines that are completely empty
	/// (or only have whitespace characters).
	/// </summary>
	public sealed class EmptyLogLineFilter
		: ILogEntryFilter
	{
		/// <inheritdoc />
		public bool PassesFilter(IReadOnlyLogEntry logLine)
		{
			var message = logLine.RawContent;

			return !string.IsNullOrWhiteSpace(message);
		}

		/// <inheritdoc />
		public List<LogLineMatch> Match(IReadOnlyLogEntry line)
		{
			return new List<LogLineMatch>();
		}

		#region Implementation of ILogEntryFilter

		/// <inheritdoc />
		public bool PassesFilter(IEnumerable<IReadOnlyLogEntry> logEntry)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public void Match(IReadOnlyLogEntry line, List<LogLineMatch> matches)
		{
			throw new System.NotImplementedException();
		}

		#endregion
	}
}