using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.ActionCenter
{
	public static class ActionCenterExtensions
	{
		public static void AddRange(this IActionCenter actionCenter, IEnumerable<INotification> notifications)
		{
			foreach (var notification in notifications)
			{
				actionCenter.Add(notification);
			}
		}
	}
}