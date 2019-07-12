using System;
using log4net.Appender;
using log4net.Core;

namespace Tailviewer.PluginRepository
{
	sealed class ColoringConsoleAppender
		: AppenderSkeleton
	{
		private readonly bool _logTimestamps;

		public ColoringConsoleAppender(bool logTimestamps)
		{
			_logTimestamps = logTimestamps;
		}

		#region Overrides of AppenderSkeleton

		protected override void Append(LoggingEvent loggingEvent)
		{
			var previous = Console.ForegroundColor;
			try
			{
				if (loggingEvent.Level == Level.Error ||
				    loggingEvent.Level == Level.Fatal)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("ERROR: {0}", FormatMessage(loggingEvent));
				}
				else if (loggingEvent.Level == Level.Warn)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("WARN: {0}", FormatMessage(loggingEvent));
				}
				else
				{
					Console.WriteLine(FormatMessage(loggingEvent));
				}
			}
			finally
			{
				Console.ForegroundColor = previous;
			}
		}

		private string FormatMessage(LoggingEvent loggingEvent)
		{
			if (_logTimestamps)
				return string.Format("{0} {1}", loggingEvent.TimeStamp, loggingEvent.RenderedMessage);

			return loggingEvent.RenderedMessage;
		}

		#endregion
	}
}