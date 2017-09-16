using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse
{
	/// <summary>
	///     Represents a running analysis of one or more data sources.
	/// </summary>
	public sealed class AnalysisViewModel
		: INotifyPropertyChanged
	{
		private readonly DelegateCommand _addPageCommand;

		private readonly ObservableCollection<AnalysisPageViewModel> _pages;
		private string _name;

		public AnalysisViewModel()
		{
			_pages = new ObservableCollection<AnalysisPageViewModel>();
			_addPageCommand = new DelegateCommand(AddPage);
			_name = "Unsaved analysis";
		}

		public IEnumerable<AnalysisPageViewModel> Pages => _pages;

		public string Name
		{
			get { return _name; }
			set
			{
				if (value == _name)
					return;

				_name = value;
				EmitPropertyChanged();
			}
		}

		public ICommand AddPageCommand => _addPageCommand;
		public event PropertyChangedEventHandler PropertyChanged;

		private void AddPage()
		{
			_pages.Add(new AnalysisPageViewModel());
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}