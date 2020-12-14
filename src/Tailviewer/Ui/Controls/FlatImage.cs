using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Tailviewer.Ui.Controls
{
	public class FlatImage
		: Control
	{
		public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
		                                                "Content", typeof(System.Windows.Media.Geometry), typeof(FlatImage), new PropertyMetadata(default(System.Windows.Media.Geometry)));

		public static readonly DependencyProperty InvertedForegroundProperty = DependencyProperty.Register(
		                                                       "InvertedForeground", typeof(Brush), typeof(FlatImage),
		                                                       new PropertyMetadata(default(Brush)));

		public static readonly DependencyProperty IsInvertedProperty = DependencyProperty.Register(
		                                                "IsInverted", typeof(bool), typeof(FlatImage), new PropertyMetadata(default(bool)));

		static FlatImage()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (FlatImage), new FrameworkPropertyMetadata(typeof (FlatImage)));
		}

		public bool IsInverted
		{
			get { return (bool) GetValue(IsInvertedProperty); }
			set { SetValue(IsInvertedProperty, value); }
		}

		public Geometry Content
		{
			get { return (System.Windows.Media.Geometry) GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		public Brush InvertedForeground
		{
			get { return (Brush) GetValue(InvertedForegroundProperty); }
			set { SetValue(InvertedForegroundProperty, value); }
		}
	}
}