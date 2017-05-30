using System.Collections.Generic;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.SidePanel;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public sealed class AnalyseMainPanelViewModel
		: AbstractMainPanelViewModel
	{
		private readonly ISidePanelViewModel[] _sidePanels;
		private AnalysisViewModel _analysis;
		private bool _isAnalysisSelected;

		public AnalyseMainPanelViewModel(IApplicationSettings applicationSettings)
			: base(applicationSettings)
		{
			_sidePanels = new ISidePanelViewModel[]
			{
				new AnalysesSidePanel()
			};
		}

		public AnalysisViewModel Analysis
		{
			get { return _analysis; }
			private set
			{
				if (value == _analysis)
					return;

				_analysis = value;
				EmitPropertyChanged();
			}
		}

		public override IEnumerable<ISidePanelViewModel> SidePanels => _sidePanels;

		public bool IsAnalysisSelected
		{
			get { return _isAnalysisSelected; }
			set
			{
				if (value == _isAnalysisSelected)
					return;

				_isAnalysisSelected = value;
				EmitPropertyChanged();
			}
		}

		public override void Update()
		{
			
		}
	}
}