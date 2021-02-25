﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Tailviewer.Ui.ViewModels.ContextMenu
{
	/// <summary>
	///     Base class for context menu view models.
	/// </summary>
	public abstract class AbstractMenuViewModel
		: IMenuViewModel
		, INotifyPropertyChanged
	{
		private ICommand _command;
		private string _header;
		private bool _isChecked;
		private Path _icon;
		private string _toolTip;

		public event PropertyChangedEventHandler PropertyChanged;

		public event Action<bool> IsCheckedChanged;

		protected void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#region Implementation of IContextMenuViewModel

		public string Header
		{
			get { return _header; }
			set
			{
				if (value == _header)
					return;

				_header = value;
				EmitPropertyChanged();
			}
		}

		public string ToolTip
		{
			get { return _toolTip; }
			set
			{
				if (value == _toolTip)
					return;
				_toolTip = value;
				EmitPropertyChanged();
			}
		}

		public Path Icon
		{
			get { return _icon; }
			set
			{
				if (value == _icon)
					return;
				_icon = value;
				EmitPropertyChanged();
			}
		}

		public ICommand Command
		{
			get { return _command; }
			set
			{
				if (value == _command)
					return;

				_command = value;
				EmitPropertyChanged();
			}
		}

		public abstract bool IsCheckable { get; }

		public bool IsChecked
		{
			get { return _isChecked; }
			set
			{
				if (value == _isChecked)
					return;
				_isChecked = value;

				EmitPropertyChanged();
				EmitIsCheckedChanged(value);
			}
		}

		private void EmitIsCheckedChanged(bool isChecked)
		{
			var fn = IsCheckedChanged;
			fn?.Invoke(isChecked);
		}

		#endregion
	}
}