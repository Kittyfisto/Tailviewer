namespace Tailviewer.BusinessLogic
{
	public static class LogFileExtensions
	{
		public static LogEntry[] GetSection(this ILogFile logFile, LogFileSection section)
		{
			var entries = new LogEntry[section.Count];
			logFile.GetSection(section, entries);
			return entries;
		}
	}
}