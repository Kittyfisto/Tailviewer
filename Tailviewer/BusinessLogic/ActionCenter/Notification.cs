using System;

namespace Tailviewer.BusinessLogic.ActionCenter
{
	public sealed class Notification
		: INotification
	{
		private Notification(Level level, DateTime timestamp, string title, string message)
		{
			Level = level;
			Timestamp = timestamp;
			Title = title;
			Message = message;
		}

		public DateTime Timestamp { get; }

		public string Message { get; }

		public Level Level { get; }

		public string Title { get; }

		public bool ForceShow => false;

		public static Notification CreateInfo(string title, string message)
		{
			return new Notification(Level.Info, DateTime.Now, title, message);
		}

		public static Notification CreateWarning(string title, string message)
		{
			return new Notification(Level.Warning, DateTime.Now, title, message);
		}

		public static Notification CreateError(string title, string message)
		{
			return new Notification(Level.Error, DateTime.Now, title, message);
		}
	}
}