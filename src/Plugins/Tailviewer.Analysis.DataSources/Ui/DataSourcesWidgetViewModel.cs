using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Metrolib;
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
		private readonly Dictionary<DataSourceId, DataSourceViewModel> _dataSourcesById;
		private int _dataSourceCount;

		public DataSourcesWidgetViewModel(IWidgetTemplate template, IDataSourceAnalyser dataSourceAnalyser)
			: base(template, dataSourceAnalyser)
		{
			_dataSources = new ObservableCollection<DataSourceViewModel>();
			_dataSourcesById = new Dictionary<DataSourceId, DataSourceViewModel>();
			CanBeEdited = true;

			Configuration.PropertyChanged += ConfigurationOnPropertyChanged;
		}

		public DataSourcesWidgetConfiguration Configuration => (DataSourcesWidgetConfiguration) ViewConfiguration;

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

		private void ConfigurationOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			EmitTemplateModified();
		}

		public override void Update()
		{
			if (TryGetResult(out DataSourcesResult result))
			{
				foreach (var dataSource in result.DataSources)
				{
					if (!_dataSourcesById.TryGetValue(dataSource.Id, out var viewModel))
					{
						viewModel = new DataSourceViewModel(dataSource,
							(DataSourcesWidgetConfiguration) ViewConfiguration);
						_dataSourcesById.Add(dataSource.Id, viewModel);
						_dataSources.Add(viewModel);
					}

					viewModel.Size = dataSource.SizeInBytes != null
						? Size.FromBytes(dataSource.SizeInBytes.Value)
						: (Size?) null;
					viewModel.Created = dataSource.Created;
					viewModel.LastModified = dataSource.LastModified;
				}

				for(int i = 0; i < _dataSources.Count;)
				{
					var id = _dataSources[i].Id;
					if (result.DataSources.All(x => x.Id != id))
					{
						_dataSourcesById.Remove(id);
						_dataSources.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}

				DataSourceCount = _dataSources.Count;
			}

			base.Update();
		}
	}
}