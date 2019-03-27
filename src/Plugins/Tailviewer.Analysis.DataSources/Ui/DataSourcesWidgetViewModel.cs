using System.Collections.Generic;
using System.Collections.ObjectModel;
using Tailviewer.Analysis.DataSources.BusinessLogic;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;
using Tailviewer.Templates.Analysis;

namespace Tailviewer.Analysis.DataSources.Ui
{
	public sealed class DataSourcesWidgetViewModel
		: AbstractWidgetViewModel
	{
		private readonly ObservableCollection<DataSourceViewModel> _dataSources;
		private readonly Dictionary<string, DataSourceViewModel> _dataSourcesByName;
		private int _dataSourceCount;

		public DataSourcesWidgetViewModel(IWidgetTemplate template, IDataSourceAnalyser dataSourceAnalyser)
			: base(template, dataSourceAnalyser)
		{
			_dataSources = new ObservableCollection<DataSourceViewModel>();
			_dataSourcesByName = new Dictionary<string, DataSourceViewModel>();
		}

		public IEnumerable<DataSourceViewModel> DataSources => _dataSources;

		public int DataSourceCount
		{
			get => _dataSourceCount;
			private set
			{
				if (value == _dataSourceCount)
					return;

				_dataSourceCount = value;
				EmitPropertyChanged();
			}
		}

		public override void Update()
		{
			DataSourcesResult result;
			if (TryGetResult(out result))
			{
				foreach (var dataSource in result.DataSources)
				{
					if (!_dataSourcesByName.ContainsKey(dataSource.Name))
					{
						var viewModel = new DataSourceViewModel(dataSource);
						_dataSourcesByName.Add(dataSource.Name, viewModel);
						_dataSources.Add(viewModel);
					}
				}
				// TODO: Remove invalid/old
				DataSourceCount = _dataSources.Count;
			}

			base.Update();
		}
	}
}
