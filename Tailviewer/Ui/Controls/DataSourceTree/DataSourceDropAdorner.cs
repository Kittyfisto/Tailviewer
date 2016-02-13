using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls.DataSourceTree
{
	internal sealed class DataSourceDropAdorner
		: Adorner
	{
		private readonly IDataSourceViewModel _dataSource;
		private DataSourceDropType _type;
		private readonly Pen _pen;

		public DataSourceDropAdorner(TreeViewItem adornedElement,
		                             IDataSourceViewModel dataSource,
		                             DataSourceDropType type)
			: base(adornedElement)
		{
			_dataSource = dataSource;
			_pen = new Pen(Brushes.DodgerBlue, 2);
			_type = type;
		}

		public IDataSourceViewModel DataSource
		{
			get { return _dataSource; }
		}

		public DataSourceDropType Type
		{
			get { return _type; }
			set
			{
				_type = value;
				InvalidateVisual();
			}
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			switch (_type)
			{
				case DataSourceDropType.ArrangeTop:
					drawingContext.DrawLine(_pen, new Point(0, 0), new Point(ActualWidth, 0));
					break;

				case DataSourceDropType.Group:
					var rect = new Rect(0, 0, ActualWidth, ActualHeight);
					drawingContext.DrawRectangle(null,
												 _pen,
												 rect);
					break;

				case DataSourceDropType.ArrangeBottom:
					drawingContext.DrawLine(_pen, new Point(0, ActualHeight), new Point(ActualWidth, ActualHeight));
					break;
			}
		}
	}
}