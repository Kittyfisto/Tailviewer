using System;
using System.Threading.Tasks;
using Tailviewer.BusinessLogic.Filters;

namespace Tailviewer.BusinessLogic.LogFiles
{
	public static class LogFileExtensions
	{
		public static LogLine[] GetSection(this ILogFile logFile, LogFileSection section)
		{
			var entries = new LogLine[section.Count];
			logFile.GetSection(section, entries);
			return entries;
		}

		public static FilteredLogFile AsFiltered(this ILogFile logFile, ITaskScheduler scheduler, ILogEntryFilter filter)
		{
			return AsFiltered(logFile, scheduler, filter, TimeSpan.FromMilliseconds(10));
		}

		public static FilteredLogFile AsFiltered(this ILogFile logFile, ITaskScheduler scheduler, ILogEntryFilter filter, TimeSpan maximumWaitTime)
		{
			var file = new FilteredLogFile(scheduler, logFile, filter);
			file.Start(maximumWaitTime);
			return file;
		}
	}
}