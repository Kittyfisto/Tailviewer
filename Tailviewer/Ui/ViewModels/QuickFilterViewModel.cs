using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.Core.Filters;
using Tailviewer.Core.Settings;
using Tailviewer.Settings;
using QuickFilter = Tailviewer.BusinessLogic.Filters.QuickFilter;

namespace Tailviewer.Ui.ViewModels
{
	public sealed class QuickFilterViewModel
		: INotifyPropertyChanged
	{
		private readonly QuickFilter _quickFilter;
		private readonly ICommand _removeCommand;
		private IDataSource _currentDataSource;
		private bool _isEditing;
		private bool _isValid;

		public QuickFilterViewModel(QuickFilter quickFilter, Action<QuickFilterViewModel> onRemove)
		{
			if (quickFilter == null) throw new ArgumentNullException(nameof(quickFilter));
			if (onRemove == null) throw new ArgumentNullException(nameof(onRemove));

			_quickFilter = quickFilter;
			_removeCommand = new DelegateCommand(() => onRemove(this));

			UpdateValidity();
		}

		public bool IsValid
		{
			get { return _isValid; }
			private set
			{
				if (value == _isValid)
					return;

				_isValid = value;
				EmitPropertyChanged();
			}
		}

		public ICommand RemoveCommand => _removeCommand;

		public IDataSource CurrentDataSource
		{
			get { return _currentDataSource; }
			set
			{
				if (value == CurrentDataSource)
					return;

				IDataSource hadDataSource = _currentDataSource;
				bool before = IsActive;
				_currentDataSource = value;
				bool after = IsActive;

				if ((hadDataSource != null) != (_currentDataSource != null))
					EmitPropertyChanged("CanBeActivated");

				if (before != after)
					EmitPropertyChanged("IsActive");
			}
		}

		public bool CanBeActivated => _currentDataSource != null;

		public bool IsActive
		{
			get
			{
				IDataSource dataSource = _currentDataSource;
				if (dataSource == null)
					return false;

				return _currentDataSource.IsQuickFilterActive(_quickFilter.Id);
			}
			set
			{
				if (value == IsActive)
					return;

				IDataSource dataSource = _currentDataSource;
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

		public bool IsInverted
		{
			get { return _quickFilter.IsInverted; }
			set
			{
				if (value == IsInverted)
					return;

				_quickFilter.IsInverted = value;
				UpdateValidity();
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
				UpdateValidity();
				EmitPropertyChanged();

				if (_currentDataSource != null)
					IsActive = true;
			}
		}

		public QuickFilterMatchType MatchType
		{
			get { return _quickFilter.MatchType; }
			set
			{
				if (value == MatchType)
					return;

				_quickFilter.MatchType = value;
				UpdateValidity();
				EmitPropertyChanged();
			}
		}

		public Guid Id => _quickFilter.Id;

		public event PropertyChangedEventHandler PropertyChanged;

		private void UpdateValidity()
		{
			try
			{
				_quickFilter.CreateFilter();
				IsValid = true;
			}
			catch (ArgumentException)
			{
				IsValid = false;
			}
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public ILogEntryFilter CreateFilter()
		{
			if (!IsActive)
				return null;

			return _quickFilter.CreateFilter();
		}
	}
}