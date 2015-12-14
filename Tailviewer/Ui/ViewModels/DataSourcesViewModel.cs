using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Tailviewer.Settings;
using DataSource = Tailviewer.BusinessLogic.DataSource;
using DataSources = Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class DataSourcesViewModel
	{
		private readonly DataSources _dataSources;
		private readonly ObservableCollection<DataSourceViewModel> _viewModels;
		private readonly ApplicationSettings _settings;

		public DataSourcesViewModel(ApplicationSettings settings, DataSources dataSources)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			if (dataSources == null) throw new ArgumentNullException("dataSources");

			_settings = settings;
			_viewModels = new ObservableCollection<DataSourceViewModel>();
			_dataSources = dataSources;
			foreach (var dataSource in dataSources)
			{
				Add(dataSource);
			}
		}

		public void Update()
		{
			foreach (var dataSource in _viewModels)
			{
				dataSource.Update();
			}
		}

		public ObservableCollection<DataSourceViewModel> Observable
		{
			get { return _viewModels; }
		}

		public DataSourceViewModel GetOrAdd(string fileName)
		{
			var fullName = Path.GetFullPath(fileName);
			var viewModel =
				_viewModels.FirstOrDefault(x => string.Equals(x.FullName, fullName, StringComparison.InvariantCultureIgnoreCase));
			if (viewModel == null)
			{
				var dataSource = _dataSources.Add(fileName);
				viewModel = Add(dataSource);
				_settings.Save();
			}

			return viewModel;
		}

		private DataSourceViewModel Add(DataSource dataSource)
		{
			var viewModel = new DataSourceViewModel(dataSource);
			viewModel.Remove += OnRemove;
			_viewModels.Add(viewModel);
			return viewModel;
		}

		private void OnRemove(DataSourceViewModel viewModel)
		{
			_viewModels.Remove(viewModel);
			_dataSources.Remove(viewModel.DataSource);
			_settings.Save();
		}
	}
}