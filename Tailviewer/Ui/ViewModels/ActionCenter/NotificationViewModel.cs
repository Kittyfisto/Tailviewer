using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tailviewer.BusinessLogic.ActionCenter;

namespace Tailviewer.Ui.ViewModels.ActionCenter
{
	public sealed class NotificationViewModel
		: INotificationViewModel
		, INotifyPropertyChanged
	{
		private readonly Notification _notification;
		private bool _isRead;

		public NotificationViewModel(Notification notification)
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

		public bool IsRead
		{
			get { return _isRead; }
			set
			{
				if (value == _isRead)
					return;

				_isRead = value;
				EmitPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}