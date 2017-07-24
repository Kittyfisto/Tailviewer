using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters
{
	public sealed class OrFilter
		: ILogEntryFilter
	{
		public bool PassesFilter(LogLine logLine)
		{
			throw new NotImplementedException();
		}

		public List<LogLineMatch> Match(LogLine line)
		{
			throw new NotImplementedException();
		}

		public bool PassesFilter(IEnumerable<LogLine> logEntry)
		{
			throw new NotImplementedException();
		}

		public void Match(LogLine line, List<LogLineMatch> matches)
		{
			throw new NotImplementedException();
		}
	}
}