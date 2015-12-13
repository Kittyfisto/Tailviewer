using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using SharpTail.BusinessLogic;
using SharpTail.Properties;

namespace SharpTail.Ui.ViewModels
{
	public sealed class DataSourceCollection
	{
		private readonly ObservableCollection<DataSourceViewModel> _dataSources;

		public DataSourceCollection()
		{
			_dataSources = new ObservableCollection<DataSourceViewModel>();

			var dataSources = Settings.Default.DataSources;
			if (dataSources != null)
			{
				foreach(var dataSource in dataSources)
				{
					if (dataSource != null)
					{
						Add(dataSource);
					}
				}
			}
		}

		public ObservableCollection<DataSourceViewModel> Observable
		{
			get { return _dataSources; }
		}

		public DataSourceViewModel GetOrAdd(string fileName)
		{
			var fullName = Path.GetFullPath(fileName);
			var viewModel =
				_dataSources.FirstOrDefault(x => string.Equals(x.FullName, fullName, StringComparison.InvariantCultureIgnoreCase));
			if (viewModel == null)
			{
				viewModel = Add(new DataSource(fileName));
			}

			return viewModel;
		}

		private DataSourceViewModel Add(DataSource dataSource)
		{
			var viewModel = new DataSourceViewModel(dataSource);
			viewModel.Remove += OnRemove;
			_dataSources.Add(viewModel);
			Save();
			return viewModel;
		}

		private void OnRemove(DataSourceViewModel viewModel)
		{
			_dataSources.Remove(viewModel);
			Save();
		}

		private void Save()
		{
			var items = _dataSources.Select(x => x.DataSource).ToList();
			Settings.Default.DataSources = items;
			Settings.Default.Save();
		}
	}
}