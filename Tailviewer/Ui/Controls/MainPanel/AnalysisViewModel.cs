using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public sealed class AnalysisViewModel
		: INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private readonly ObservableCollection<AnalysisPageViewModel> _pages;
		private readonly DelegateCommand _addPageCommand;
		private readonly IDataSourceAnalysis _analysis;

		public IEnumerable<AnalysisPageViewModel> Pages => _pages;

		public AnalysisViewModel(IDataSourceAnalysis analysis)
		{
			if (analysis == null)
				throw new ArgumentNullException(nameof(analysis));

			_analysis = analysis;
			_pages = new ObservableCollection<AnalysisPageViewModel>();
			_addPageCommand = new DelegateCommand(AddPage);
		}

		private void AddPage()
		{
			_pages.Add(new AnalysisPageViewModel());
		}

		public ICommand AddPageCommand => _addPageCommand;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}