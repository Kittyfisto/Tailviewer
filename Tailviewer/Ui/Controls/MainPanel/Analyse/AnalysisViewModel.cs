using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Analysis;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse
{
	/// <summary>
	///     Represents a running analysis of one or more data sources.
	/// </summary>
	public sealed class AnalysisViewModel
		: IAnalysisViewModel
	{
		private readonly AnalysisTemplate _template;
		private readonly IAnalyserGroup _analyser;
		private readonly DelegateCommand _addPageCommand;
		private readonly DelegateCommand _removeCommand;
		private readonly ObservableCollection<AnalysisPageViewModel> _pages;
		private AnalysisPageViewModel _selectedPage;

		private string _name;
		private double _progress;

		public AnalysisViewModel(AnalysisTemplate template, IAnalyserGroup analyser)
		{
			if (template == null)
				throw new ArgumentNullException(nameof(template));
			if (analyser == null)
				throw new ArgumentNullException(nameof(analyser));

			_analyser = analyser;
			_template = template;
			_pages = new ObservableCollection<AnalysisPageViewModel>();
			_addPageCommand = new DelegateCommand(AddPage);
			_removeCommand = new DelegateCommand(RemoveThis);
			_name = "Unsaved analysis";

			AddPage();
			_selectedPage = _pages.FirstOrDefault();
		}

		public IAnalysisTemplate Template => _template;

		private void RemoveThis()
		{
			OnRemove?.Invoke(this);
		}

		public AnalysisId Id => _analyser.Id;

		public IEnumerable<AnalysisPageViewModel> Pages => _pages;

		public AnalysisPageViewModel SelectedPage
		{
			get { return _selectedPage; }
			set
			{
				if (value == _selectedPage)
					return;

				_selectedPage = value;
				EmitPropertyChanged();
			}
		}

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
		public ICommand RemoveCommand => _removeCommand;

		public double Progress
		{
			get { return _progress; }
			private set
			{
				if (value == _progress)
					return;

				_progress = value;
				EmitPropertyChanged();
			}
		}

		public event Action<AnalysisViewModel> OnRemove;

		public void Add(ILogFile logFile)
		{
			_analyser.Add(logFile);
		}

		public void Remove(ILogFile logFile)
		{
			_analyser.Remove(logFile);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void Update()
		{
			_selectedPage?.Update();
			Progress = _analyser.Progress.RelativeValue;
		}

		private void AddPage()
		{
			var template = new PageTemplate();
			var page = new AnalysisPageViewModel(template, _analyser);
			page.OnDelete += PageOnOnDelete;

			_pages.Add(page);
			_template.Add(template);

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
			_template.Remove(analysisPageViewModel.Template);
			UpdateCanBeDeleted();
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}