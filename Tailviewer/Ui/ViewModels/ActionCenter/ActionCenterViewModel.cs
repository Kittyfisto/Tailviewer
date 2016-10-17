using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Metrolib;
using Tailviewer.BusinessLogic.ActionCenter;

namespace Tailviewer.Ui.ViewModels.ActionCenter
{
	public sealed class ActionCenterViewModel
		: INotifyPropertyChanged
	{
		private readonly IActionCenter _actionCenter;
		private readonly ObservableCollection<INotificationViewModel> _notifications;
		private readonly IDispatcher _dispatcher;

		private bool _hasNewMessages;
		private int _unreadCount;
		private bool _isOpen;

		public ActionCenterViewModel()
		{
			_notifications = new ObservableCollection<INotificationViewModel>();
			AddNotification(ChangeLog.MostRecent);
		}

		public ActionCenterViewModel(IDispatcher dispatcher, IActionCenter actionCenter)
		{
			if (dispatcher == null)
				throw new ArgumentNullException("dispatcher");
			if (actionCenter == null)
				throw new ArgumentNullException("actionCenter");

			_dispatcher = dispatcher;
			_notifications = new ObservableCollection<INotificationViewModel>();

			_actionCenter = actionCenter;
			_actionCenter.NotificationAdded += ActionCenterOnNotificationAdded;
			foreach (var notification in actionCenter.Notifications)
			{
				AddNotification(notification);
			}
		}

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
			}

			UpdateUnreadCount();
		}

		private void Add(INotificationViewModel notificationViewModel)
		{
			notificationViewModel.OnRemove += Remove;
			_notifications.Insert(0, notificationViewModel);
		}

		private void Remove(INotificationViewModel notificationViewModel)
		{
			_notifications.Remove(notificationViewModel);
			UpdateUnreadCount();
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

		public IEnumerable<INotificationViewModel> Notifications
		{
			get { return _notifications; }
		}

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

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		private void UpdateUnreadCount()
		{
			UnreadCount = _notifications.Count(x => !x.IsRead);
		}
	}
}