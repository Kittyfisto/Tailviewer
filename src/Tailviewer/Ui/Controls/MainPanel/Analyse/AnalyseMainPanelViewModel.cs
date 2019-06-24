using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Metrolib;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.DataSources;
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
		private readonly ISidePanelViewModel[] _sidePanels;
		private readonly ICommand _createAnalysisCommand;

		private readonly AnalysesSidePanel _analysesSidePanel;
		private readonly AnalysisDataSelectionSidePanel _dataSelectionSidePanel;
		private readonly WidgetsSidePanel _widgetsSidePanel;
		private readonly IDispatcher _dispatcher;

		private AnalysisViewModel _selectedAnalysis;

		private bool _isAnalysisSelected;
		private string _windowTitle;
		private string _windowTitleSuffix;

		public AnalyseMainPanelViewModel(IServiceContainer services,
		                                 IApplicationSettings applicationSettings,
		                                 IDataSources dataSources,
		                                 IAnalysisStorage analysisStorage)
			: base(applicationSettings)
		{
			if (dataSources == null)
				throw new ArgumentNullException(nameof(dataSources));
			if (analysisStorage == null)
				throw new ArgumentNullException(nameof(analysisStorage));

			_dispatcher = services.Retrieve<IDispatcher>();
			_sidePanels = new ISidePanelViewModel[]
			{
				_analysesSidePanel = new AnalysesSidePanel(services, analysisStorage),
				_dataSelectionSidePanel = new AnalysisDataSelectionSidePanel(applicationSettings, dataSources),
				_widgetsSidePanel = new WidgetsSidePanel(services.Retrieve<IPluginLoader>())
			};
			_createAnalysisCommand = new DelegateCommand(CreateAnalysis);
			_analysesSidePanel.PropertyChanged += AnalysesSidePanelOnPropertyChanged;

			SelectedSidePanel = _sidePanels.FirstOrDefault(x => x.Id == applicationSettings.MainWindow?.SelectedSidePanel);
		}

		private void AnalysesSidePanelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(AnalysesSidePanel.SelectedAnalysis):
					SelectedAnalysis = _analysesSidePanel.SelectedAnalysis;
					break;
			}
		}

		private void CreateAnalysis()
		{
			var viewModel = _analysesSidePanel.CreateNewAnalysis();
			viewModel.OnRemove += AnalysisViewModelOnOnRemove;
			SelectedAnalysis = viewModel;
		}

		private void AnalysisViewModelOnOnRemove(AnalysisViewModel analysis)
		{
			if (analysis == SelectedAnalysis)
				SelectedAnalysis = null;
		}

		public AnalysisViewModel SelectedAnalysis
		{
			get { return _selectedAnalysis; }
			set
			{
				if (value == _selectedAnalysis)
					return;

				if (_selectedAnalysis != null)
				{
					_selectedAnalysis.PropertyChanged -= OnSelectedAnalysisPropertyChanged;
				}

				_selectedAnalysis = value;

				if (_selectedAnalysis != null)
				{
					_selectedAnalysis.PropertyChanged += OnSelectedAnalysisPropertyChanged;
				}

				EmitPropertyChanged();

				IsAnalysisSelected = value != null;
				// So that the side panel may update the list of selected data sources and
				// also tell the analysis when new data sources are (de-)selected.
				_dataSelectionSidePanel.CurrentAnalysis = value;

				UpdateWindowTitle();
			}
		}

		private void OnSelectedAnalysisPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(AnalysisViewModel.Name):
					UpdateWindowTitle();
					break;
			}
		}

		private void UpdateWindowTitle()
		{
			if (_selectedAnalysis != null)
			{
				WindowTitle = string.Format("{0} - {1}", Constants.MainWindowTitle, _selectedAnalysis.Name);
				WindowTitleSuffix = _selectedAnalysis.Name;
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
			_selectedAnalysis?.Update();

			_analysesSidePanel.Update(); //< TODO: Remove this hack! It ensures that we create a view model for the "current" analysis
			if (SelectedSidePanel != _analysesSidePanel)
				SelectedSidePanel?.Update();
		}
	}
}