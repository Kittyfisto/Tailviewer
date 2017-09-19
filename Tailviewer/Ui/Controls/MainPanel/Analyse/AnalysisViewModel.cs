using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse
{
	/// <summary>
	///     Represents a running analysis of one or more data sources.
	/// </summary>
	public sealed class AnalysisViewModel
		: INotifyPropertyChanged
	{
		private readonly IAnalyserGroup _analyser;
		private readonly DelegateCommand _addPageCommand;

		private readonly ObservableCollection<AnalysisPageViewModel> _pages;
		private string _name;

		public AnalysisViewModel(IAnalyserGroup analyser)
		{
			if (analyser == null)
				throw new ArgumentNullException(nameof(analyser));

			_analyser = analyser;
			_pages = new ObservableCollection<AnalysisPageViewModel>();
			AddPage();
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
			var page = new AnalysisPageViewModel(_analyser);
			page.OnDelete += PageOnOnDelete;
			_pages.Add(page);
			UpdateCanBeDeleted();
		}

		private void UpdateCanBeDeleted()
		{
			for (int i = 0; i < _pages.Count; ++i)
			{
				_pages[i].CanBeDeleted = _pages.Count > 1;
			}
		}

		private void PageOnOnDelete(AnalysisPageViewModel analysisPageViewModel)
		{
			_pages.Remove(analysisPageViewModel);
			UpdateCanBeDeleted();
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}