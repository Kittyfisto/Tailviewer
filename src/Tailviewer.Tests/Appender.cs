using System.Collections.Generic;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace Tailviewer.Tests
{
	public sealed class Appender
		: AppenderSkeleton
	{
		private readonly string _loggerName;
		private readonly Level[] _levels;
		private readonly object _syncRoot;
		private readonly List<LoggingEvent> _events;

		public static Appender CaptureEvents(string loggerName, params Level[] levels)
		{
			var appender = new Appender(loggerName, levels);
			var hierarchy = (Hierarchy)LogManager.GetRepository();
			hierarchy.Root.AddAppender(appender);
			hierarchy.Configured = true;
			return appender;
		}

		public Appender(string loggerName, Level[] levels)
		{
			_loggerName = loggerName;
			_levels = levels;
			_syncRoot = new object();
			_events = new List<LoggingEvent>();
		}

		public IReadOnlyList<LoggingEvent> Events
		{
			get
			{
				lock (_syncRoot)
				{
					return _events.ToList();
				}
			}
		}

		#region Overrides of AppenderSkeleton

		protected override void Append(LoggingEvent loggingEvent)
		{
			if (!loggingEvent.LoggerName.StartsWith(_loggerName))
				return;

			if (!MatchesLevel(loggingEvent))
				return;

			lock (_syncRoot)
			{
				_events.Add(loggingEvent);
			}
		}

		private bool MatchesLevel(LoggingEvent loggingEvent)
		{
			foreach (var level in _levels)
			{
				if (loggingEvent.Level == level)
					return true;
			}

			return false;
		}

		#endregion
	}
}
