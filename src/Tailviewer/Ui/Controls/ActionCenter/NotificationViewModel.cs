using System;
using Tailviewer.BusinessLogic.ActionCenter;

namespace Tailviewer.Ui.Controls.ActionCenter
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

		public DateTime Timestamp => _notification.Timestamp;

		public string Message => _notification.Message;

		public Level Level => _notification.Level;
	}
}