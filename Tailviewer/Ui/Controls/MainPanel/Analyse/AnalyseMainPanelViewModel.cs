using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core.Analysis;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels;
using Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels.Analyses;
using Tailviewer.Ui.Controls.SidePanel;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse
{
	/// <summary>
	///     The view model representing the "analyse" page.
	/// </summary>
	public sealed class AnalyseMainPanelViewModel
		: AbstractMainPanelViewModel
	{
		private readonly ITaskScheduler _taskScheduler;
		private readonly ILogAnalyserEngine _logAnalyserEngine;
		private readonly ISidePanelViewModel[] _sidePanels;
		private readonly ICommand _createAnalysisCommand;

		private readonly AnalysesSidePanel _analysesSidePanel;
		private readonly AnalysisDataSelectionSidePanel _dataSelectionSidePanel;
		private readonly WidgetsSidePanel _widgetsSidePanel;
		private readonly IDispatcher _dispatcher;
		private readonly IAnalysisStorage _analysisStorage;

		private AnalysisViewModel _analysis;

		private bool _isAnalysisSelected;
		private string _windowTitle;
		private string _windowTitleSuffix;

		public AnalyseMainPanelViewModel(IApplicationSettings applicationSettings,
			IDataSources dataSources,
			IDispatcher dispatcher,
			ITaskScheduler taskScheduler,
			ILogAnalyserEngine logAnalyserEngine,
			IAnalysisStorage analysisStorage)
			: base(applicationSettings)
		{
			if (dataSources == null)
				throw new ArgumentNullException(nameof(dataSources));
			if (dispatcher == null)
				throw new ArgumentNullException(nameof(dispatcher));
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));
			if (logAnalyserEngine == null)
				throw new ArgumentNullException(nameof(logAnalyserEngine));
			if (analysisStorage == null)
				throw new ArgumentNullException(nameof(analysisStorage));

			_dispatcher = dispatcher;
			_taskScheduler = taskScheduler;
			_logAnalyserEngine = logAnalyserEngine;
			_analysisStorage = analysisStorage;
			_sidePanels = new ISidePanelViewModel[]
			{
				_analysesSidePanel = new AnalysesSidePanel(dispatcher, taskScheduler, analysisStorage),
				_dataSelectionSidePanel = new AnalysisDataSelectionSidePanel(applicationSettings, dataSources),
				_widgetsSidePanel = new WidgetsSidePanel()
			};
			_createAnalysisCommand = new DelegateCommand(CreateAnalysis);

			SelectedSidePanel = _sidePanels.FirstOrDefault(x => x.Id == applicationSettings.MainWindow?.SelectedSidePanel);
		}

		private void CreateAnalysis()
		{
			var template = new AnalysisTemplate();
			var analyser = new BusinessLogic.Analysis.ActiveAnalysis(template,
				_taskScheduler,
				_logAnalyserEngine,
				TimeSpan.FromMilliseconds(100));
			var analysisViewModel = new AnalysisViewModel(_dispatcher, template, analyser, _analysisStorage);
			analysisViewModel.OnRemove += AnalysisViewModelOnOnRemove;
			_analysesSidePanel.Add(analysisViewModel);
			Analysis = analysisViewModel;
		}

		private void AnalysisViewModelOnOnRemove(AnalysisViewModel analysis)
		{
			if (analysis == Analysis)
				Analysis = null;
		}

		public AnalysisViewModel Analysis
		{
			get { return _analysis; }
			private set
			{
				if (value == _analysis)
					return;

				_analysis = value;
				EmitPropertyChanged();

				IsAnalysisSelected = value != null;
				// So that the side panel may update the list of selected data sources and
				// also tell the analysis when new data sources are (de-)selected.
				_dataSelectionSidePanel.CurrentAnalysis = value;

				UpdateWindowTitle();
			}
		}

		private void UpdateWindowTitle()
		{
			if (_analysis != null)
			{
				WindowTitle = string.Format("{0} - {1}", Constants.MainWindowTitle, _analysis.Name);
				WindowTitleSuffix = _analysis.Name;
			}
			else
			{
				WindowTitle = Constants.MainWindowTitle;
				WindowTitleSuffix = null;
			}
		}

		public override IEnumerable<ISidePanelViewModel> SidePanels => _sidePanels;

		public bool IsAnalysisSelected
		{
			get { return _isAnalysisSelected; }
			private set
			{
				if (value == _isAnalysisSelected)
					return;

				_isAnalysisSelected = value;
				EmitPropertyChanged();
			}
		}

		public ICommand CreateAnalysisCommand => _createAnalysisCommand;

		public string WindowTitle
		{
			get { return _windowTitle; }
			set
			{
				if (value == _windowTitle)
					return;

				_windowTitle = value;
				EmitPropertyChanged();
			}
		}

		public string WindowTitleSuffix
		{
			get { return _windowTitleSuffix; }
			set
			{
				if (value == _windowTitleSuffix)
					return;

				_windowTitleSuffix = value;
				EmitPropertyChanged();
			}
		}

		public override void Update()
		{
			_analysis?.Update();
			SelectedSidePanel?.Update();
		}
	}
}