using System;
using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Filters;

namespace Tailviewer.Core.LogFiles
{
	public static class LogFileExtensions
	{
		public static LogLine[] GetSection(this ILogFile logFile, LogFileSection section)
		{
			var entries = new LogLine[section.Count];
			logFile.GetSection(section, entries);
			return entries;
		}

		public static FilteredLogFile AsFiltered(this ILogFile logFile, ITaskScheduler scheduler, ILogLineFilter logLineFilter, ILogEntryFilter logEntryFilter)
		{
			return AsFiltered(logFile, scheduler, logLineFilter, logEntryFilter, TimeSpan.FromMilliseconds(10));
		}

		public static FilteredLogFile AsFiltered(this ILogFile logFile, ITaskScheduler scheduler, ILogLineFilter logLineFilter, ILogEntryFilter logEntryFilter, TimeSpan maximumWaitTime)
		{
			var file = new FilteredLogFile(scheduler, maximumWaitTime, logFile, logLineFilter, logEntryFilter);
			return file;
		}
	}
}