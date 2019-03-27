using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic.ActionCenter;

namespace Tailviewer.Ui.Controls.ActionCenter
{
	public sealed class ActionCenterViewModel
		: INotifyPropertyChanged
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IActionCenter _actionCenter;
		private readonly IDispatcher _dispatcher;
		private readonly ObservableCollection<INotificationViewModel> _notifications;

		private bool _hasNewMessages;
		private bool _isOpen;
		private int _unreadCount;

		public ActionCenterViewModel()
		{
			_notifications = new ObservableCollection<INotificationViewModel>();
			AddNotification(Changelog.MostRecent);
		}

		public ActionCenterViewModel(IDispatcher dispatcher, IActionCenter actionCenter)
		{
			if (dispatcher == null)
				throw new ArgumentNullException(nameof(dispatcher));
			if (actionCenter == null)
				throw new ArgumentNullException(nameof(actionCenter));

			_dispatcher = dispatcher;
			_notifications = new ObservableCollection<INotificationViewModel>();

			_actionCenter = actionCenter;
			_actionCenter.NotificationAdded += ActionCenterOnNotificationAdded;
			foreach (var notification in actionCenter.Notifications)
				AddNotification(notification);
		}

		public bool HasNewMessages
		{
			get { return _hasNewMessages; }
			private set
			{
				if (value == _hasNewMessages)
					return;

				_hasNewMessages = value;
				EmitPropertyChanged();
			}
		}

		public IEnumerable<INotificationViewModel> Notifications => _notifications;

		public int UnreadCount
		{
			get { return _unreadCount; }
			private set
			{
				if (value == _unreadCount)
					return;

				_unreadCount = value;
				EmitPropertyChanged();

				HasNewMessages = UnreadCount > 0;
			}
		}

		public bool IsOpen
		{
			get { return _isOpen; }
			set
			{
				if (value == _isOpen)
					return;

				_isOpen = value;
				EmitPropertyChanged();
			}
		}

		public void Update()
		{
			if (IsOpen)
			{
				foreach (var notification in _notifications)
				{
					notification.Update();
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void ActionCenterOnNotificationAdded(INotification notification)
		{
			_dispatcher.BeginInvoke(() => AddNotification(notification));
		}

		private void AddNotification(INotification notification)
		{
			var basic = notification as Notification;
			if (basic != null)
			{
				Add(new NotificationViewModel(basic));
			}
			else
			{
				var change = notification as Change;
				if (change != null)
				{
					Add(new ChangeViewModel(change));
				}
				else
				{
					var bug = notification as IBug;
					if (bug != null)
					{
						Add(new BugViewModel(bug));
					}
					else
					{
						var export = notification as IExportAction;
						if (export != null)
							Add(new ExportViewModel(export));
						else
						{
							var build = notification as Build;
							if (build != null)
							{
								Add(new BuildViewModel(build));
							}
							else
								Log.WarnFormat("Unknown notification: {0}", notification);
						}
					}
				}
			}

			if (notification.ForceShow)
				IsOpen = true;

			UpdateUnreadCount();
		}

		private void Add(INotificationViewModel notificationViewModel)
		{
			if (_notifications.Count == BusinessLogic.ActionCenter.ActionCenter.MaximumNotificationCount)
				_notifications.RemoveAt(_notifications.Count - 1);

			notificationViewModel.OnRemove += Remove;
			_notifications.Insert(0, notificationViewModel);
			notificationViewModel.Update();
		}

		private void Remove(INotificationViewModel notificationViewModel)
		{
			_notifications.Remove(notificationViewModel);
			UpdateUnreadCount();
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void UpdateUnreadCount()
		{
			UnreadCount = _notifications.Count(x => !x.IsRead);
		}
	}
}