using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.SidePanel;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels
{
	public sealed class AnalysisDataSelectionSidePanel
		: AbstractSidePanelViewModel
	{
		private readonly Dictionary<DataSourceId, AnalysisDataSourceViewModel> _dataSourcesById;
		private readonly ObservableCollection<AnalysisDataSourceViewModel> _dataSourceViewModels;
		private readonly IDataSources _dataSources;
		private AnalysisViewModel _currentAnalysis;

		public AnalysisDataSelectionSidePanel(IApplicationSettings settings, IDataSources dataSources)
		{
			_dataSources = dataSources;
			_dataSourcesById = new Dictionary<DataSourceId, AnalysisDataSourceViewModel>();
			_dataSourceViewModels = new ObservableCollection<AnalysisDataSourceViewModel>();

			Update();
		}

		public AnalysisViewModel CurrentAnalysis
		{
			get { return _currentAnalysis; }
			set
			{
				if (value == _currentAnalysis)
					return;

				_currentAnalysis = value;
				EmitPropertyChanged();

				foreach (var dataSource in _dataSourceViewModels)
				{
					dataSource.CurrentAnalysis = value;
				}
			}
		}

		public override Geometry Icon => Icons.Database;

		public override string Id => "Analysis.DataSelection";

		public IEnumerable<AnalysisDataSourceViewModel> DataSources => _dataSourceViewModels;

		public override void Update()
		{
			// #1: Add new data sources and update them
			foreach (var dataSource in _dataSources)
			{
				// For now we'll just display every single data source
				// and completely skip merged data sources.
				var single = dataSource as SingleDataSource;
				if (single != null)
				{
					AnalysisDataSourceViewModel viewModel;
					if (!_dataSourcesById.TryGetValue(single.Id, out viewModel))
					{
						viewModel = new AnalysisDataSourceViewModel(dataSource);
						_dataSourcesById.Add(single.Id, viewModel);
						_dataSourceViewModels.Add(viewModel);
					}

					viewModel.Update();
				}
			}

			// #2: Remove old data sources
			for (int i = 0; i < _dataSourceViewModels.Count;)
			{
				var viewModel = _dataSourceViewModels[i];
				if (!_dataSources.Contains(viewModel.Id))
				{
					_dataSourceViewModels.RemoveAt(i);
					_dataSourcesById.Remove(viewModel.Id);
				}
				else
				{
					++i;
				}
			}
		}
	}
}