using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;
using DataSource = Tailviewer.BusinessLogic.DataSource;
using DataSources = Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Ui.ViewModels
{
	internal sealed class DataSourcesViewModel
	{
		private readonly DataSources _dataSources;
		private readonly ApplicationSettings _settings;
		private readonly ObservableCollection<DataSourceViewModel> _viewModels;

		public DataSourcesViewModel(ApplicationSettings settings, DataSources dataSources)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			if (dataSources == null) throw new ArgumentNullException("dataSources");

			_settings = settings;
			_viewModels = new ObservableCollection<DataSourceViewModel>();
			_dataSources = dataSources;
			foreach (DataSource dataSource in dataSources)
			{
				Add(dataSource);
			}
		}

		public ObservableCollection<DataSourceViewModel> Observable
		{
			get { return _viewModels; }
		}

		public void Update()
		{
			foreach (DataSourceViewModel dataSource in _viewModels)
			{
				dataSource.Update();
			}
		}

		public DataSourceViewModel GetOrAdd(string fileName)
		{
			string fullName = Path.GetFullPath(fileName);
			DataSourceViewModel viewModel =
				_viewModels.FirstOrDefault(x => string.Equals(x.FullName, fullName, StringComparison.InvariantCultureIgnoreCase));
			if (viewModel == null)
			{
				DataSource dataSource = _dataSources.Add(fileName);
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