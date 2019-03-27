using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.ActionCenter
{
	public interface IActionCenter
	{
		void Add(INotification notification);
		IEnumerable<INotification> Notifications { get; }
		event Action<INotification> NotificationAdded;
	}
}