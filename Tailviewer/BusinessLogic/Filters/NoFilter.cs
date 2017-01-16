using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Filters
{
	/// <summary>
	///     An <see cref="ILogLineFilter" /> implementation that passes every log line.
	/// </summary>
	public sealed class NoFilter
		: ILogEntryFilter
	{
		public bool PassesFilter(LogLine logLine)
		{
			return true;
		}

		public List<LogLineMatch> Match(LogLine line)
		{
			return new List<LogLineMatch>();
		}

		public bool PassesFilter(IEnumerable<LogLine> logEntry)
		{
			return true;
		}

		public void Match(LogLine line, List<LogLineMatch> matches)
		{
			
		}
	}
}