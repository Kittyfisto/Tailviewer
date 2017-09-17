using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets
{
	/// <summary>
	///     The base class for all <see cref="IWidgetViewModel" /> implementations.
	/// </summary>
	public abstract class AbstractWidgetViewModel
		: IWidgetViewModel
	{
		private bool _isEditing;
		private string _title;

		/// <summary>
		/// </summary>
		protected AbstractWidgetViewModel(bool canBeEdited = true)
		{
			CanBeEdited = canBeEdited;
			DeleteCommand = new DelegateCommand(Delete);
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

		public bool CanBeEdited { get; }

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

		public ICommand DeleteCommand { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		public event Action<IWidgetViewModel> OnDelete;

		private void Delete()
		{
			OnDelete?.Invoke(this);
		}

		protected virtual void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}