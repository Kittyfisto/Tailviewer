using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
		private readonly IDispatcher _dispatcher;
		private readonly AnalysisViewTemplate _viewTemplate;
		private readonly IAnalysis _analyser;
		private readonly IAnalysisStorage _analysisStorage;
		private readonly DelegateCommand _addPageCommand;
		private readonly DelegateCommand _removeCommand;
		private readonly ObservableCollection<AnalysisPageViewModel> _pages;
		private readonly DelegateCommand2 _captureSnapshotCommand;
		private AnalysisPageViewModel _selectedPage;

		private string _name;
		private double _progress;
		private Task _saveSnapshotTask;

		public AnalysisViewModel(IDispatcher dispatcher,
			AnalysisViewTemplate viewTemplate,
			IAnalysis analyser,
			IAnalysisStorage analysisStorage)
		{
			if (dispatcher == null)
				throw new ArgumentNullException(nameof(dispatcher));
			if (viewTemplate == null)
				throw new ArgumentNullException(nameof(viewTemplate));
			if (analyser == null)
				throw new ArgumentNullException(nameof(analyser));
			if (analysisStorage == null)
				throw new ArgumentNullException(nameof(analysisStorage));

			_dispatcher = dispatcher;
			_analyser = analyser;
			_analysisStorage = analysisStorage;
			_viewTemplate = viewTemplate;
			_pages = new ObservableCollection<AnalysisPageViewModel>();
			_addPageCommand = new DelegateCommand(AddPage);
			_removeCommand = new DelegateCommand(RemoveThis);
			_captureSnapshotCommand = new DelegateCommand2(CaptureSnapshot)
			{
				CanBeExecuted = true
			};
			_name = "Unsaved analysis";

			AddPage();
			_selectedPage = _pages.FirstOrDefault();
		}

		private void CaptureSnapshot()
		{
			_saveSnapshotTask = _analysisStorage.SaveSnapshot(_analyser, _viewTemplate);
			UpdateCanCaptureSnapshot(); //< DO NOT CHANGE CALL ORDER
			_saveSnapshotTask.ContinueWith(OnSnapshotSaved);
		}

		private void OnSnapshotSaved(Task task)
		{
			_dispatcher.BeginInvoke(() => OnSnapshotSaved2(task));
		}

		private void OnSnapshotSaved2(Task task)
		{
			_saveSnapshotTask = null;
			UpdateCanCaptureSnapshot();
		}

		private void UpdateCanCaptureSnapshot()
		{
			_captureSnapshotCommand.CanBeExecuted = _saveSnapshotTask == null;
		}

		public ICommand CaptureSnapshotCommand => _captureSnapshotCommand;

		private void RemoveThis()
		{
			OnRemove?.Invoke(this);
		}

		private bool _isSelected;

		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				if (value == _isSelected)
					return;

				_isSelected = value;
				EmitPropertyChanged();
			}
		}

		public AnalysisId Id => _analyser.Id;

		public IEnumerable<IAnalysisPageViewModel> Pages => _pages;

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
			_viewTemplate.Add(template);

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
			_viewTemplate.Remove(analysisPageViewModel.Template);
			UpdateCanBeDeleted();
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}