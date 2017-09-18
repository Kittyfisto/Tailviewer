using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.LineCount
{
	public sealed class LineCountWidgetViewModel
		: AbstractWidgetViewModel
	{
		private int _count;
		private string _caption;
		private QuickFiltersSidePanelViewModel _quickFilters;

		public LineCountWidgetViewModel()
		{
			Title = "Line Count";
			
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

		public QuickFiltersSidePanelViewModel QuickFilters
		{
			get { return _quickFilters; }
		}
	}
}