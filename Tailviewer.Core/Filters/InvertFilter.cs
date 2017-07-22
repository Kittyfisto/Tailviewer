using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters
{
	public sealed class InvertFilter : ILogEntryFilter
	{
		private readonly ILogEntryFilter _filter;

		public InvertFilter(ILogEntryFilter filter)
		{
			if (filter == null)
				throw new ArgumentNullException(nameof(filter));

			_filter = filter;
		}

		public bool PassesFilter(IEnumerable<LogLine> logEntry)
		{
			return !_filter.PassesFilter(logEntry);
		}

		public bool PassesFilter(LogLine logLine)
		{
			return !_filter.PassesFilter(logLine);
		}

		public List<LogLineMatch> Match(LogLine line)
		{
			// We don't mark any text because we would have to mark ALL text excluding
			// the actual filter text (since we're the inversion of the inner filter).
			// This is really not helpful and thus we don't mark any text at all...
			return new List<LogLineMatch>();
		}

		public void Match(LogLine line, List<LogLineMatch> matches)
		{
			// We don't mark any text because we would have to mark ALL text excluding
			// the actual filter text (since we're the inversion of the inner filter).
			// This is really not helpful and thus we don't mark any text at all...
		}
	}
}