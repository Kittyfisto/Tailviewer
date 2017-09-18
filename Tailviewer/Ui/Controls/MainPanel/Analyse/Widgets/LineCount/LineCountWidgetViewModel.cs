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

		public LineCountWidgetViewModel()
		{
			Title = "Line Count";
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
	}
}