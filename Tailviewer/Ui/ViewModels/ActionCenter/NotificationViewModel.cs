using System;
using Tailviewer.BusinessLogic.ActionCenter;

namespace Tailviewer.Ui.ViewModels.ActionCenter
{
	public sealed class NotificationViewModel
		: AbstractNotificationViewModel
	{
		private readonly Notification _notification;

		public NotificationViewModel(Notification notification)
			: base(notification)
		{
			_notification = notification;
		}

		public DateTime Timestamp
		{
			get { return _notification.Timestamp; }
		}

		public string Message
		{
			get { return _notification.Message; }
		}

		public Level Level
		{
			get { return _notification.Level; }
		}
	}
}