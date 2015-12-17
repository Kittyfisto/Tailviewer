namespace Tailviewer.BusinessLogic
{
	public static class LogFileExtensions
	{
		public static LogLine[] GetSection(this ILogFile logFile, LogFileSection section)
		{
			var entries = new LogLine[section.Count];
			logFile.GetSection(section, entries);
			return entries;
		}
	}
}