using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Tailviewer.Ui.Controls.DataSourceTree
{
	public sealed class DataSourceDragAdorner
		: Adorner
	{
		private readonly AdornerLayer _adornerLayer;
		private readonly ContentPresenter _contentPresenter;
		private double _leftOffset;
		private double _topOffset;

		public DataSourceDragAdorner(object data,
		                             DataTemplate dataTemplate,
		                             UIElement adornedElement,
		                             AdornerLayer adornerLayer)
			: base(adornedElement)
		{
			_adornerLayer = adornerLayer;

			_contentPresenter = new ContentPresenter {Content = data, ContentTemplate = dataTemplate, Opacity = 0.75};

			_adornerLayer.Add(this);
		}

		protected override int VisualChildrenCount
		{
			get { return 1; }
		}

		protected override Size MeasureOverride(Size constraint)
		{
			_contentPresenter.Measure(constraint);
			return _contentPresenter.DesiredSize;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			_contentPresenter.Arrange(new Rect(finalSize));
			return finalSize;
		}

		protected override Visual GetVisualChild(int index)
		{
			return _contentPresenter;
		}

		public void UpdatePosition(double left, double top)
		{
			_leftOffset = left;
			_topOffset = top;
			_adornerLayer?.Update(AdornedElement);
		}

		public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
		{
			var result = new GeneralTransformGroup();
			result.Children.Add(base.GetDesiredTransform(transform));
			result.Children.Add(new TranslateTransform(_leftOffset, _topOffset));
			return result;
		}

		public void Destroy()
		{
			_adornerLayer.Remove(this);
		}
	}
}