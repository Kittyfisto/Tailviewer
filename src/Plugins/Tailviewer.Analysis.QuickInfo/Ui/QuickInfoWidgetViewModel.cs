using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Metrolib;
using Tailviewer.Analysis.QuickInfo.BusinessLogic;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;
using Tailviewer.Templates.Analysis;

namespace Tailviewer.Analysis.QuickInfo.Ui
{
	public sealed class QuickInfoWidgetViewModel
		: AbstractWidgetViewModel
	{
		private readonly QuickInfoWidgetConfiguration _viewConfiguration;
		private readonly QuickInfoAnalyserConfiguration _analyserConfiguration;

		private readonly Dictionary<Guid, QuickInfoViewModel> _quickInfosById;
		private readonly ObservableCollection<QuickInfoViewModel> _quickInfos;
		private readonly DelegateCommand2 _addQuickInfoCommand;

		public QuickInfoWidgetViewModel(IServiceContainer services,
		                                IWidgetTemplate template,
		                                IDataSourceAnalyser dataSourceAnalyser)
			: base(services, template, dataSourceAnalyser)
		{
			_viewConfiguration = template.Configuration as QuickInfoWidgetConfiguration ?? new QuickInfoWidgetConfiguration();
			_analyserConfiguration = AnalyserConfiguration as QuickInfoAnalyserConfiguration ?? new QuickInfoAnalyserConfiguration();

			_quickInfosById = new Dictionary<Guid, QuickInfoViewModel>();
			_quickInfos = new ObservableCollection<QuickInfoViewModel>();
			_addQuickInfoCommand = new DelegateCommand2(AddQuickInfo);

			foreach (var quickInfo in _viewConfiguration.Titles)
			{
				var analysis = _analyserConfiguration.QuickInfos.FirstOrDefault(x => x.Id == quickInfo.Id);
				if (analysis != null)
				{
					AddQuickInfo(quickInfo.Id, quickInfo, analysis);
				}
			}

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
			var viewConfig = new QuickInfoViewConfiguration(id);
			var analyserConfig = new QuickInfoConfiguration(id);
			AddQuickInfo(id, viewConfig, analyserConfig);

			_viewConfiguration.Add(viewConfig);
			_analyserConfiguration.Add(analyserConfig);

			EmitTemplateModified();
		}

		private void AddQuickInfo(Guid id, QuickInfoViewConfiguration viewConfig, QuickInfoConfiguration analyserConfig)
		{
			var viewModel = new QuickInfoViewModel(id, viewConfig, analyserConfig);
			viewModel.OnRemoved += OnQuickInfoRemoved;
			viewModel.IsEditing = IsEditing;

			_quickInfosById.Add(id, viewModel);
			_quickInfos.Add(viewModel);
		}

		private void OnQuickInfoRemoved(QuickInfoViewModel viewModel)
		{
			var id = viewModel.Id;
			_quickInfosById.Remove(id);
			_quickInfos.Remove(viewModel);

			_viewConfiguration.Remove(id);
			_analyserConfiguration.Remove(id);
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