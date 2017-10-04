using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using Metrolib;
using Tailviewer.Ui.Controls.SidePanel;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels.Analyses
{
	/// <summary>
	///     Represents the side panel of available and actually active analyses, snapshots and templates.
	/// </summary>
	public sealed class AnalysesSidePanel
		: AbstractSidePanelViewModel
	{
		private readonly ObservableCollection<AnalysisViewModel> _active;
		private readonly ObservableCollection<AnalysisTemplateViewModel> _available;
		private readonly ObservableCollection<AnalysisSnapshotItemViewModel> _snapshots;
		private bool _hasActiveAnalyses;
		private bool _hasAvailableAnalyses;

		public AnalysesSidePanel()
		{
			_active = new ObservableCollection<AnalysisViewModel>();
			_available = new ObservableCollection<AnalysisTemplateViewModel>();
			_snapshots = new ObservableCollection<AnalysisSnapshotItemViewModel>();

			UpdateTooltip();
			PropertyChanged += OnPropertyChanged;
		}

		private void UpdateTooltip()
		{
			Tooltip = IsSelected
				? "Hide the list of analyses"
				: "Show the list of analyses";
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case nameof(IsSelected):
					UpdateTooltip();
					break;
			}
		}

		public IEnumerable<AnalysisViewModel> Active => _active;
		public IEnumerable<AnalysisTemplateViewModel> Available => _available;
		public IEnumerable<AnalysisSnapshotItemViewModel> Snapshots => _snapshots;

		public override Geometry Icon => Icons.ChartGantt;

		public override string Id => "Analysis";

		public bool HasActiveAnalyses
		{
			get { return _hasActiveAnalyses; }
			private set
			{
				if (value == _hasActiveAnalyses)
					return;

				_hasActiveAnalyses = value;
				EmitPropertyChanged();
			}
		}

		public override void Update()
		{
			HasActiveAnalyses = _active.Count > 0;
			HasAvailableAnalyses = _available.Count > 0;
			UpdateQuickInfo();
		}

		public bool HasAvailableAnalyses
		{
			get { return _hasAvailableAnalyses; }
			private set
			{
				if (value == _hasAvailableAnalyses)
					return;

				_hasAvailableAnalyses = value;
				EmitPropertyChanged();
			}
		}

		public void Add(AnalysisViewModel analysis)
		{
			_active.Add(analysis);
			analysis.OnRemove += AnalysisOnOnRemove;
			Update();
		}

		private void AnalysisOnOnRemove(AnalysisViewModel analysis)
		{
			_active.Remove(analysis);
		}

		private void UpdateQuickInfo()
		{
			switch (_active.Count)
			{
				case 0:
					QuickInfo = null;
					break;

				case 1:
					QuickInfo = _active[0].Name;
					break;

				default:
					QuickInfo = string.Format("{0} analyses", _active.Count);
					break;
			}
		}
	}
}
