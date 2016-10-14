using System;
using System.Collections.Generic;
using System.Linq;

namespace Tailviewer.BusinessLogic.ActionCenter
{
	public sealed class ActionCenter
		: IActionCenter
	{
		private readonly object _syncRoot;
		private readonly List<INotification> _notifications;

		public ActionCenter()
		{
			_syncRoot = new object();
			_notifications = new List<INotification>();
		}

		public void Add(INotification notification)
		{
			lock (_syncRoot)
			{
				_notifications.Add(notification);
			}

			var fn = NotificationAdded;
			if (fn != null)
				fn(notification);
		}

		public IEnumerable<INotification> Notifications
		{
			get
			{
				lock (_syncRoot)
				{
					return _notifications.ToList();
				}
			}
		}

		public event Action<INotification> NotificationAdded;
	}

	public interface IActionCenter
	{
		void Add(INotification notification);
		IEnumerable<INotification> Notifications { get; }
		event Action<INotification> NotificationAdded;
	}
}