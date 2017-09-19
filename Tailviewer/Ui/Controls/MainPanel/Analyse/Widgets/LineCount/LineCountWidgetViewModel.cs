using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.Ui.Controls.QuickFilter;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.LineCount
{
	public sealed class LineCountWidgetViewModel
		: AbstractWidgetViewModel
	{
		private int _count;
		private string _caption;
		private readonly QuickFiltersViewModel _quickFilters;

		public LineCountWidgetViewModel(IDataSourceAnalyser dataSourceAnalyser)
			: base(dataSourceAnalyser)
		{
			Title = "Line Count";
			Caption = "Line(s)";
			var filters = new QuickFilters(new Core.Settings.QuickFilters());
			_quickFilters = new QuickFiltersViewModel(filters);
		}

		public int Count
		{
			get { return _count; }
			private set
			{
				if (value == _count)
					return;

				_count = value;
				EmitPropertyChanged();
			}
		}

		public string Caption
		{
			get { return _caption; }
			set
			{
				if (value == _caption)
					return;

				_caption = value;
				EmitPropertyChanged();
			}
		}

		public QuickFiltersViewModel QuickFilters => _quickFilters;

		protected override ILogAnalyserConfiguration Configuration => null;

		public override void OnUpdate()
		{
			
		}
	}
}