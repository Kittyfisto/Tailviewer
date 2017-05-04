using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.ActionCenter;

namespace Tailviewer.Ui.Controls.ActionCenter
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

		public virtual void Update()
		{}

		public event Action<INotificationViewModel> OnRemove;

		private void Remove()
		{
			OnRemove?.Invoke(this);
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

		protected void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}