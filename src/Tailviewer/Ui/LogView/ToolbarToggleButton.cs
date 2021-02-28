using System.Windows;
using System.Windows.Media;
using Metrolib.Controls;

namespace Tailviewer.Ui.LogView
{
	public class ToolbarToggleButton
		: FlatToggleButton
	{
		public static readonly DependencyProperty CheckedIconProperty =
			DependencyProperty.Register("CheckedIcon", typeof(Geometry), typeof(ToolbarToggleButton),
			                            new PropertyMetadata(default(Geometry)));

		public static readonly DependencyProperty UncheckedIconProperty = DependencyProperty.Register(
		 "UncheckedIcon", typeof(Geometry), typeof(ToolbarToggleButton), new PropertyMetadata(default(Geometry)));

		static ToolbarToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolbarToggleButton),
			                                         new FrameworkPropertyMetadata(typeof(ToolbarToggleButton)));
		}

		public Geometry UncheckedIcon
		{
			get { return (Geometry) GetValue(UncheckedIconProperty); }
			set { SetValue(UncheckedIconProperty, value); }
		}

		public Geometry CheckedIcon

		{
			get { return (Geometry) GetValue(CheckedIconProperty); }
			set { SetValue(CheckedIconProperty, value); }
		}
	}
}