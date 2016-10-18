using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.ActionCenter;

namespace Tailviewer.Ui.ViewModels.ActionCenter
{
	public abstract class AbstractNotificationViewModel
		: INotificationViewModel
		, INotifyPropertyChanged
	{
		private readonly INotification _notification;
		private readonly ICommand _removeCommand;
		private bool _isRead;
		private string _title;

		protected AbstractNotificationViewModel(INotification notification)
		{
			_notification = notification;
			_removeCommand = new DelegateCommand(Remove);
			_title = _notification.Title;
		}

		public event Action<INotificationViewModel> OnRemove;

		private void Remove()
		{
			var fn = OnRemove;
			if (fn != null)
				fn(this);
		}

		public ICommand RemoveCommand
		{
			get { return _removeCommand; }
		}

		public string Title
		{
			get { return _title; }
			set
			{
				if (value == _title)
					return;

				_title = value;
				EmitPropertyChanged();
			}
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