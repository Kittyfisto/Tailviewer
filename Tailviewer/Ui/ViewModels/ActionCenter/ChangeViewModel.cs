using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tailviewer.BusinessLogic.ActionCenter;

namespace Tailviewer.Ui.ViewModels.ActionCenter
{
	public sealed class ChangeViewModel
		: INotificationViewModel
		, INotifyPropertyChanged
	{
		private readonly Change _change;
		private bool _isRead;

		public ChangeViewModel(Change change)
		{
			if (change == null)
				throw new ArgumentNullException("change");

			_change = change;
		}

		public Version Version
		{
			get { return _change.Version; }
		}

		public IEnumerable<string> Features
		{
			get { return _change.Features; }
		}

		public IEnumerable<string> Bugfixes
		{
			get { return _change.Bugfixes; }
		}

		public IEnumerable<string> Misc
		{
			get { return _change.Misc; }
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