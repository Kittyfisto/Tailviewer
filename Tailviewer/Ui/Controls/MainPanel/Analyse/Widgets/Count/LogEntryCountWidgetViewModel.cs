using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Analysis.Analysers.Count;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.Ui.Controls.QuickFilter;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.Count
{
	public sealed class EntryCountWidgetViewModel
		: AbstractWidgetViewModel
	{
		private long? _count;
		private string _caption;
		private readonly QuickFiltersViewModel _quickFilters;

		public EntryCountWidgetViewModel(IDataSourceAnalyser dataSourceAnalyser)
			: base(dataSourceAnalyser)
		{
			Title = "Line Count";
			Caption = "Line(s)";
			var filters = new QuickFilters(new Core.Settings.QuickFilters());
			_quickFilters = new QuickFiltersViewModel(filters);
		}

		public long? Count
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

		public override void Update()
		{
			LogEntryCountResult result;
			if (TryGetResult(out result))
			{
				Count = result.Count;
			}
			else
			{
				Count = null;
			}
		}
	}
}