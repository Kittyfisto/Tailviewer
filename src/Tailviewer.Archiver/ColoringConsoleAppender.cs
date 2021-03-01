using System;
using log4net.Appender;
using log4net.Core;

namespace Tailviewer.Archiver
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class ColoringConsoleAppender
		: AppenderSkeleton
	{
		private readonly bool _logTimestamps;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logTimestamps"></param>
		public ColoringConsoleAppender(bool logTimestamps)
		{
			_logTimestamps = logTimestamps;
		}

		#region Overrides of AppenderSkeleton

		/// <inheritdoc />
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