using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Metrolib;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Analysis;
using Tailviewer.Core.Analysis.Layouts;

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
		private bool _isSelected;
		private IPluginLoader _pluginLoader;
		private double _progress;
		private Task _saveSnapshotTask;

		public AnalysisViewModel(IDispatcher dispatcher,
			AnalysisViewTemplate viewTemplate,
			IAnalysis analyser,
			IAnalysisStorage analysisStorage,
		                         IPluginLoader pluginLoader)
		{
			_dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
			_analyser = analyser ?? throw new ArgumentNullException(nameof(analyser));
			_analysisStorage = analysisStorage ?? throw new ArgumentNullException(nameof(analysisStorage));
			_viewTemplate = viewTemplate ?? throw new ArgumentNullException(nameof(viewTemplate));
			_pluginLoader = pluginLoader ?? throw new ArgumentNullException(nameof(pluginLoader));
			_pages = new ObservableCollection<AnalysisPageViewModel>();
			_addPageCommand = new DelegateCommand(AddNewPage);
			_removeCommand = new DelegateCommand(RemoveThis);
			_captureSnapshotCommand = new DelegateCommand2(CaptureSnapshot)
			{
				CanBeExecuted = true
			};

			if (viewTemplate.Pages.Any())
			{
				foreach (var pageTemplate in viewTemplate.Pages)
				{
					// TODO: Solve this conundrum!
					AddPage((PageTemplate) pageTemplate);
				}
			}
			else
			{
				AddNewPage();
			}

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
			get { return _viewTemplate.Name; }
			set
			{
				if (value == _viewTemplate.Name)
					return;

				_viewTemplate.Name = value;
				EmitPropertyChanged();

				_analysisStorage.SaveAsync(Id);
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

		public void Add(DataSourceId id, ILogFile logFile)
		{
			_analyser.Add(id, logFile);
		}

		public void Remove(DataSourceId id, ILogFile logFile)
		{
			_analyser.Remove(id, logFile);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void Update()
		{
			_selectedPage?.Update();
			Progress = _analyser.Progress.RelativeValue;
		}

		private void AddNewPage()
		{
			var template = new PageTemplate
			{
				Title = "New Page",
				Layout = new HorizontalWidgetLayoutTemplate()
			};
			AddPage(template);
			_viewTemplate.Add(template);
		}

		private void AddPage(PageTemplate template)
		{
			var page = new AnalysisPageViewModel(Id, template, _analyser, _analysisStorage, _pluginLoader);
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
			_viewTemplate.Remove(analysisPageViewModel.Template);
			UpdateCanBeDeleted();
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}