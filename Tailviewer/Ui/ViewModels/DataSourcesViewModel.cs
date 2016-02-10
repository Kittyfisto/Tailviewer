using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;
using IDataSource = Tailviewer.BusinessLogic.IDataSource;
using DataSources = Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class DataSourcesViewModel
	{
		private readonly DataSources _dataSources;
		private readonly ApplicationSettings _settings;
		private readonly ObservableCollection<IDataSourceViewModel> _viewModels;

		public DataSourcesViewModel(ApplicationSettings settings, DataSources dataSources)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			if (dataSources == null) throw new ArgumentNullException("dataSources");

			_settings = settings;
			_viewModels = new ObservableCollection<IDataSourceViewModel>();
			_dataSources = dataSources;
			foreach (IDataSource dataSource in dataSources)
			{
				Add(dataSource);
			}
		}

		public ObservableCollection<IDataSourceViewModel> Observable
		{
			get { return _viewModels; }
		}

		public void Update()
		{
			foreach (SingleDataSourceViewModel dataSource in _viewModels)
			{
				dataSource.Update();
			}
		}

		public IDataSourceViewModel GetOrAdd(string fileName)
		{
			string fullName = Path.GetFullPath(fileName);
			IDataSourceViewModel viewModel =
				_viewModels.FirstOrDefault(x => Represents(x, fullName));
			if (viewModel == null)
			{
				var dataSource = _dataSources.Add(fileName);
				viewModel = Add(dataSource);
				_settings.Save();
			}

			return viewModel;
		}

		private bool Represents(IDataSourceViewModel dataSourceViewModel, string fullName)
		{
			var file = dataSourceViewModel as SingleDataSourceViewModel;
			if (file == null)
				return false;

			return string.Equals(file.FullName, fullName, StringComparison.InvariantCultureIgnoreCase);
		}

		private IDataSourceViewModel Add(IDataSource dataSource)
		{
			var viewModel = new SingleDataSourceViewModel((SingleDataSource) dataSource);
			viewModel.Remove += OnRemove;
			_viewModels.Add(viewModel);
			return viewModel;
		}

		private void OnRemove(IDataSourceViewModel viewModel)
		{
			_viewModels.Remove(viewModel);
			_dataSources.Remove(viewModel.DataSource);
			_settings.Save();
		}
	}
}