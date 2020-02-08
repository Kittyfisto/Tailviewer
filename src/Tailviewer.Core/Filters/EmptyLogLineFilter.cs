using System.Collections.Generic;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;

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
		public bool PassesFilter(LogLine logLine)
		{
			var message = logLine.Message;

			return !string.IsNullOrWhiteSpace(message);
		}

		/// <inheritdoc />
		public List<LogLineMatch> Match(LogLine line)
		{
			return new List<LogLineMatch>();
		}

		#region Implementation of ILogEntryFilter

		/// <inheritdoc />
		public bool PassesFilter(IEnumerable<LogLine> logEntry)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public void Match(LogLine line, List<LogLineMatch> matches)
		{
			throw new System.NotImplementedException();
		}

		#endregion
	}
}