using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Tailviewer.Settings;
using DataSource = Tailviewer.BusinessLogic.DataSource;
using QuickFilter = Tailviewer.BusinessLogic.QuickFilter;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class QuickFilterViewModel
		: INotifyPropertyChanged
	{
		private readonly QuickFilter _quickFilter;

		public ICommand RemoveCommand
		{
			get { return _removeCommand; }
		}

		private readonly ICommand _removeCommand;
		private DataSource _dataSource;

		public QuickFilterViewModel(QuickFilter quickFilter, Action<QuickFilterViewModel> onRemove)
		{
			if (quickFilter == null) throw new ArgumentNullException("quickFilter");
			if (onRemove == null) throw new ArgumentNullException("onRemove");

			_quickFilter = quickFilter;
			_removeCommand = new DelegateCommand(() => onRemove(this));
		}

		public QuickFilterViewModel(QuickFilter quickFilter, Action<QuickFilterViewModel> onRemove, DataSource dataSource)
			: this(quickFilter, onRemove)
		{
			if (dataSource == null) throw new ArgumentNullException("dataSource");

			_dataSource = dataSource;
		}

		public DataSource DataSource
		{
			get { return _dataSource; }
			set
			{
				if (value == DataSource)
					return;

				var hadDataSource = _dataSource;
				var before = IsActive;
				_dataSource = value;
				var after = IsActive;

				if ((hadDataSource != null) != (_dataSource != null))
					EmitPropertyChanged("CanBeActivated");

				if (before != after)
					EmitPropertyChanged("IsActive");
			}
		}

		public bool CanBeActivated
		{
			get { return _dataSource != null; }
		}

		public bool IsActive
		{
			get
			{
				var dataSource = _dataSource;
				if (dataSource == null)
					return false;

				return _dataSource.IsQuickFilterActive(_quickFilter.Id);
			}
			set
			{
				if (value == IsActive)
					return;

				var dataSource = _dataSource;
				if (dataSource == null)
					throw new InvalidOperationException();

				if (value)
				{
					// Should I add a sanity check here?
					dataSource.ActivateQuickFilter(_quickFilter.Id);
				}
				else
				{
					dataSource.DeactivateQuickFilter(_quickFilter.Id);
				}
				EmitPropertyChanged();
			}
		}

		public string Value
		{
			get { return _quickFilter.Value; }
			set
			{
				if (value == Value)
					return;

				_quickFilter.Value = value;
				EmitPropertyChanged();
			}
		}

		public QuickFilterType Type
		{
			get { return _quickFilter.Type; }
			set
			{
				if (value == Type)
					return;

				_quickFilter.Type = value;
				EmitPropertyChanged();
			}
		}

		public Guid Id
		{
			get { return _quickFilter.Id; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}