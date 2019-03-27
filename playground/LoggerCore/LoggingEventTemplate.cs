using log4net.Core;

namespace LoggerCore
{
	public struct LoggingEventTemplate
	{
		public Level Level;
		public string Message;

		public LoggingEventTemplate(Level level, string message)
		{
			Level = level;
			Message = message;
		}

		public override string ToString()
		{
			return string.Format("{0} {1}", Level, Message);
		}
	}
}