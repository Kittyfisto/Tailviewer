using System;
using log4net.Appender;
using log4net.Core;

namespace Tailviewer.Archiver
{
	public sealed class ConsoleLogger
		: IAppender
	{
		private Level _lastLevel;
		private bool _hasWrittenNewline;

		public void Close()
		{
			WriteNewlineIfNecessary();
		}

		public void DoAppend(LoggingEvent loggingEvent)
		{
			if (loggingEvent.Level == Level.Warn)
			{
				WriteNewlineIfNecessary();
				Console.WriteLine("WARNING: {0}", loggingEvent.RenderedMessage);
				_hasWrittenNewline = true;
			}
			else if (loggingEvent.Level == Level.Error)
			{
				Console.WriteLine("ERROR: {0}", loggingEvent.RenderedMessage);
				_hasWrittenNewline = true;
			}
			else if (loggingEvent.Level == Level.Info)
			{
				var message = loggingEvent.RenderedMessage;
				if (_lastLevel != Level.Info || message != "OK")
				{
					WriteNewlineIfNecessary();
				}
				Console.Write(message);
				_hasWrittenNewline = false;
			}
			_lastLevel = loggingEvent.Level;
		}

		private void WriteNewlineIfNecessary()
		{
			if (!_hasWrittenNewline)
			{
				Console.WriteLine();
				_hasWrittenNewline = true;
			}
		}

		public string Name { get; set; }
	}
}
