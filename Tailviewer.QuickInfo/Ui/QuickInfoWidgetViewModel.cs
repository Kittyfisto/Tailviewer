using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;
using Tailviewer.QuickInfo.BusinessLogic;

namespace Tailviewer.QuickInfo.Ui
{
	public sealed class QuickInfoWidgetViewModel
		: AbstractWidgetViewModel
	{
		private readonly QuickInfoWidgetConfiguration _viewConfiguration;
		private readonly QuickInfoAnalyserConfiguration _analyserConfiguration;

		private readonly Dictionary<Guid, QuickInfoViewModel> _quickInfosById;
		private readonly ObservableCollection<QuickInfoViewModel> _quickInfos;
		private readonly DelegateCommand2 _addQuickInfoCommand;

		public QuickInfoWidgetViewModel(IDataSourceAnalyser dataSourceAnalyser,
			QuickInfoWidgetConfiguration viewConfiguration)
			: base(dataSourceAnalyser, viewConfiguration)
		{
			_viewConfiguration = viewConfiguration;
			_analyserConfiguration = AnalyserConfiguration as QuickInfoAnalyserConfiguration;

			_quickInfosById = new Dictionary<Guid, QuickInfoViewModel>();
			_quickInfos = new ObservableCollection<QuickInfoViewModel>();
			_addQuickInfoCommand = new DelegateCommand2(AddQuickInfo);

			PropertyChanged += OnPropertyChanged;
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case nameof(IsEditing):
					foreach (var viewModel in _quickInfos)
					{
						viewModel.IsEditing = IsEditing;
					}
					break;
			}
		}

		private void AddQuickInfo()
		{
			var id = Guid.NewGuid();
			var viewConfig = new QuickInfoViewConfiguration();
			var analyserConfig = new QuickInfoConfiguration();
			var viewModel = new QuickInfoViewModel(id, viewConfig, analyserConfig);
			viewModel.OnRemoved += OnQuickInfoRemoved;
			viewModel.IsEditing = IsEditing;

			_viewConfiguration.Titles.Add(id, viewConfig);
			_analyserConfiguration.QuickInfos.Add(id, analyserConfig);
			_quickInfosById.Add(id, viewModel);
			_quickInfos.Add(viewModel);
		}

		private void OnQuickInfoRemoved(QuickInfoViewModel viewModel)
		{
			var id = viewModel.Id;
			_quickInfosById.Remove(id);
			_quickInfos.Remove(viewModel);

			_viewConfiguration.Titles.Remove(id);
			_analyserConfiguration.QuickInfos.Remove(id);
			viewModel.OnRemoved -= OnQuickInfoRemoved;
		}

		public IEnumerable<QuickInfoViewModel> QuickInfos => _quickInfos;

		public ICommand AddQuickInfoCommand => _addQuickInfoCommand;

		public override void Update()
		{
			QuickInfoResult result;
			if (TryGetResult(out result))
			{
				foreach (var pair in result.QuickInfos)
				{
					QuickInfoViewModel viewModel;
					if (_quickInfosById.TryGetValue(pair.Key, out viewModel))
					{
						viewModel.Result = pair.Value;
					}
					else
					{
						// This is interesting... log it?
					}
				}

				// If we've configured a value, but it's not in the result,
				// then we have to blank it out, otherwise the user might think
				// that this is still current...
				foreach (var pair in _quickInfosById)
				{
					if (!result.QuickInfos.ContainsKey(pair.Key))
					{
						pair.Value.Result = null;
					}
				}
			}
			else
			{
				// For as long as we don't have any result
				foreach (var pair in _quickInfosById)
				{
					pair.Value.Result = null;
				}
			}
		}
	}
}