using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Tailviewer.Ui.Controls.DataSourceTree
{
	public sealed class DataSourceDropAdorner
		: Adorner
	{
		private readonly Pen _pen;

		public DataSourceDropAdorner(TreeViewItem dropTarget)
			: base(dropTarget)
		{
			_pen = new Pen(Brushes.DodgerBlue, 2);
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			var rect = new Rect(0, 0, ActualWidth, ActualHeight);
			drawingContext.DrawRectangle(null,
			                             _pen,
			                             rect);
		}
	}
}