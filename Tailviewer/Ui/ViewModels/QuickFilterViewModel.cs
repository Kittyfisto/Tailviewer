using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;
using DataSource = Tailviewer.BusinessLogic.DataSource;
using QuickFilter = Tailviewer.BusinessLogic.QuickFilter;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class QuickFilterViewModel
		: INotifyPropertyChanged
	{
		private readonly QuickFilter _quickFilter;
		private readonly ICommand _removeCommand;
		private DataSource _currentDataSource;

		public QuickFilterViewModel(QuickFilter quickFilter, Action<QuickFilterViewModel> onRemove)
		{
			if (quickFilter == null) throw new ArgumentNullException("quickFilter");
			if (onRemove == null) throw new ArgumentNullException("onRemove");

			_quickFilter = quickFilter;
			_removeCommand = new DelegateCommand(() => onRemove(this));
		}

		public ICommand RemoveCommand
		{
			get { return _removeCommand; }
		}

		public DataSource CurrentDataSource
		{
			get { return _currentDataSource; }
			set
			{
				if (value == CurrentDataSource)
					return;

				DataSource hadDataSource = _currentDataSource;
				bool before = IsActive;
				_currentDataSource = value;
				bool after = IsActive;

				if ((hadDataSource != null) != (_currentDataSource != null))
					EmitPropertyChanged("CanBeActivated");

				if (before != after)
					EmitPropertyChanged("IsActive");
			}
		}

		public bool CanBeActivated
		{
			get { return _currentDataSource != null; }
		}

		public bool IsActive
		{
			get
			{
				DataSource dataSource = _currentDataSource;
				if (dataSource == null)
					return false;

				return _currentDataSource.IsQuickFilterActive(_quickFilter.Id);
			}
			set
			{
				if (value == IsActive)
					return;

				DataSource dataSource = _currentDataSource;
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

		public IFilter CreateFilter()
		{
			if (!IsActive)
				return null;

			return _quickFilter.CreateFilter();
		}
	}
}