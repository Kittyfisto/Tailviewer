using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Tailviewer.Ui.Controls.DataSourceTree
{
	public sealed class DataSourceArrangeAdorner
		: Adorner
	{
		private readonly Pen _pen;
		private readonly DataSourceDropType _type;

		public DataSourceArrangeAdorner(TreeViewItem dropTarget,
		                                DataSourceDropType dropType)
			: base(dropTarget)
		{
			_pen = new Pen(Brushes.DodgerBlue, 2);
			_type = dropType;
		}

		public DataSourceDropType DropType
		{
			get { return _type; }
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			if (_type.HasFlag(DataSourceDropType.ArrangeTop))
			{
				drawingContext.DrawLine(_pen, new Point(0, 0), new Point(ActualWidth, 0));
			}
			else if (_type.HasFlag(DataSourceDropType.ArrangeBottom))
			{
				drawingContext.DrawLine(_pen, new Point(0, ActualHeight), new Point(ActualWidth, ActualHeight));
			}
		}
	}
}