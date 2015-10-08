namespace SharpTail.BusinessLogic
{
	public static class LogFileExtensions
	{
		public static string[] GetSection(this ILogFile logFile, LogFileSection section)
		{
			var entries = new string[section.Count];
			logFile.GetSection(section, entries);
			return entries;
		}
	}
}