using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Threading;
using log4net;

namespace Tailviewer.BusinessLogic.ActionCenter
{
	public sealed class ActionCenter
		: IActionCenter
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Due to adding notifications when the application encounters an unhandled exception (or similar),
		/// it's possible that notifications are added non-stop. This would eventually consume all available
		/// memory (due to the items-control not being virtualized) and render the application unusable.
		/// </summary>
		public const int MaximumNotificationCount = 99;

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
				if (_notifications.Count == MaximumNotificationCount)
					_notifications.RemoveAt(0);

				_notifications.Add(notification);
			}

			NotificationAdded?.Invoke(notification);
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

		public void ReportUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
		{
			Exception exception = args.Exception;

			Log.ErrorFormat("Caught unexpected exception on dispatcher: {0}", exception);

			var notification = new UnhandledException(exception);
			Add(notification);

			args.Handled = true;
		}

		public void ReportUnhandledException(object sender, UnobservedTaskExceptionEventArgs args)
		{
			Exception exception = args.Exception;

			Log.ErrorFormat("Caught unexpected exception on task: {0}", exception);

			var notification = new UnhandledException(exception);
			Add(notification);

			args.SetObserved();
		}
	}
}