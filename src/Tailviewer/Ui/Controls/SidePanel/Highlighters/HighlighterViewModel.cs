using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.Highlighters;
using Tailviewer.Core.Settings;

namespace Tailviewer.Ui.Controls.SidePanel.Highlighters
{
	public sealed class HighlighterViewModel
		: INotifyPropertyChanged
	{
		private readonly Highlighter _highlighter;
		private readonly ICommand _removeCommand;
		private bool _isActive;
		private bool _isValid;
		private bool _isEditing;

		public HighlighterViewModel(Highlighter highlighter, Action<HighlighterViewModel> onRemove)
		{
			_highlighter = highlighter;
			_removeCommand = new DelegateCommand2(() => onRemove(this));
			_isValid = true;
		}

		public bool IsActive
		{
			get { return _isActive; }
			set
			{
				if (value == _isActive)
					return;

				_isActive = value;
				EmitPropertyChanged();
			}
		}

		public bool IsValid
		{
			get { return _isValid; }
			set
			{
				if (value == _isValid)
					return;

				_isValid = value;
				EmitPropertyChanged();
			}
		}

		public string Value
		{
			get { return _highlighter.Value; }
			set
			{
				if (value == _highlighter.Value)
					return;

				_highlighter.Value = value;
				EmitPropertyChanged();
			}
		}

		public bool IsEditing
		{
			get { return _isEditing; }
			set
			{
				if (value == _isEditing)
					return;

				_isEditing = value;
				EmitPropertyChanged();
			}
		}

		public FilterMatchType MatchType
		{
			get { return _highlighter.MatchType; }
			set
			{
				if (value == _highlighter.MatchType)
					return;

				_highlighter.MatchType = value;
				EmitPropertyChanged();
			}
		}

		public ICommand RemoveCommand => _removeCommand;

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}