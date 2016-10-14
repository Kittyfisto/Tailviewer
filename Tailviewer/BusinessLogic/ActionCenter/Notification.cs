using System;

namespace Tailviewer.BusinessLogic.ActionCenter
{
	public sealed class Notification
		: INotification
	{
		private readonly DateTime _timestamp;
		private readonly string _title;
		private readonly string _message;
		private readonly Level _level;

		private Notification(Level level, DateTime timestamp, string title, string message)
		{
			_level = level;
			_timestamp = timestamp;
			_title = title;
			_message = message;
		}

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

		public DateTime Timestamp
		{
			get { return _timestamp; }
		}

		public string Message
		{
			get { return _message; }
		}

		public Level Level
		{
			get { return _level; }
		}

		public string Title
		{
			get { return _title; }
		}
	}
}