using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Filters
{
	internal sealed class InvertFilter : ILogEntryFilter
	{
		private readonly ILogEntryFilter _filter;

		public InvertFilter(ILogEntryFilter filter)
		{
			if (filter == null)
				throw new ArgumentNullException("filter");

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

		public List<FilterMatch> Match(LogLine line)
		{
			// We don't mark any text because we would have to mark ALL text excluding
			// the actual filter text (since we're the inversion of the inner filter).
			// This is really not helpful and thus we don't mark any text at all...
			return new List<FilterMatch>();
		}

		public void Match(LogLine line, List<FilterMatch> matches)
		{
			// We don't mark any text because we would have to mark ALL text excluding
			// the actual filter text (since we're the inversion of the inner filter).
			// This is really not helpful and thus we don't mark any text at all...
		}
	}
}