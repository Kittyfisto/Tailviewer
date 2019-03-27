using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;

namespace Tailviewer.Analysis.Count.Ui
{
	public sealed class FilterViewModel
		: INotifyPropertyChanged
	{
		private readonly DelegateCommand _removeCommand;
		private readonly Core.Settings.QuickFilter _filter;

		public FilterViewModel(Core.Settings.QuickFilter filter)
		{
			if (filter == null)
				throw new ArgumentNullException(nameof(filter));

			_filter = filter;
			_removeCommand = new DelegateCommand(Remove);
		}

		private void Remove()
		{
			OnRemoved?.Invoke(this);
		}

		public ICommand RemoveCommand => _removeCommand;

		public string Value
		{
			get { return _filter.Value; }
			set
			{
				if (value == _filter.Value)
					return;

				_filter.Value = value;
				EmitPropertyChanged();
			}
		}

		public Core.Settings.QuickFilter Filter => _filter;

		public event Action<FilterViewModel> OnRemoved;
		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}