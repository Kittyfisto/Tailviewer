using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core;
using Tailviewer.Core.Settings;

namespace Tailviewer.Ui.QuickFilter
{
	/// <summary>
	///     The view model to represent a single quick filter:
	///     The filter expression may be modified and various settings be changed
	///     (<see cref="BusinessLogic.Filters.QuickFilter.IsInverted" />,
	///     <see cref="BusinessLogic.Filters.QuickFilter.MatchType" />,
	///     etc..).
	/// </summary>
	public sealed class QuickFilterViewModel
		: INotifyPropertyChanged
	{
		private readonly BusinessLogic.Filters.QuickFilter _quickFilter;
		private IDataSource _currentDataSource;
		private bool _isEditing;
		private bool _isValid;

		public QuickFilterViewModel(BusinessLogic.Filters.QuickFilter quickFilter, Action<QuickFilterViewModel> onRemove)
		{
			if (quickFilter == null) throw new ArgumentNullException(nameof(quickFilter));
			if (onRemove == null) throw new ArgumentNullException(nameof(onRemove));

			_quickFilter = quickFilter;
			RemoveCommand = new DelegateCommand(() => onRemove(this));

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

		public ICommand RemoveCommand { get; }

		public IDataSource CurrentDataSource
		{
			get { return _currentDataSource; }
			set
			{
				if (value == CurrentDataSource)
					return;

				var hadDataSource = _currentDataSource;
				var before = IsActive;
				_currentDataSource = value;
				var after = IsActive;

				if (hadDataSource != null != (_currentDataSource != null))
					EmitPropertyChanged(nameof(CanBeActivated));

				if (before != after)
					EmitPropertyChanged(nameof(IsActive));
			}
		}

		public bool CanBeActivated => _currentDataSource != null;

		public bool IsActive
		{
			get
			{
				var dataSource = _currentDataSource;
				if (dataSource == null)
					return false;

				return _currentDataSource.IsQuickFilterActive(_quickFilter.Id);
			}
			set
			{
				if (value == IsActive)
					return;

				var dataSource = _currentDataSource;
				if (dataSource == null)
					throw new InvalidOperationException();

				if (value)
					dataSource.ActivateQuickFilter(_quickFilter.Id);
				else
					dataSource.DeactivateQuickFilter(_quickFilter.Id);
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

				// If a user toggles the inverted property of a filter then it becomes very clear that the user
				// intents for the filter to be used
				IsActive = true;
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

		public FilterMatchType MatchType
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

		public QuickFilterId Id => _quickFilter.Id;

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