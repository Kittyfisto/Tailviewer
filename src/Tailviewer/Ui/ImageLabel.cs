using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Tailviewer.Ui
{
	/// <summary>
	///     Displays an image next to some content.
	/// </summary>
	public class ImageLabel : ContentControl
	{
		public static readonly DependencyProperty DarkImageProperty =
			DependencyProperty.Register("DarkImage", typeof (ImageSource), typeof (ImageLabel),
			                            new PropertyMetadata(default(string)));

		public static readonly DependencyProperty LightImageProperty =
			DependencyProperty.Register("LightImage", typeof (ImageSource), typeof (ImageLabel),
			                            new PropertyMetadata(default(string)));

		public static readonly DependencyProperty UseLightColorsProperty =
			DependencyProperty.Register("UseLightColors", typeof (bool), typeof (ImageLabel), new PropertyMetadata(default(bool)));

		static ImageLabel()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (ImageLabel), new FrameworkPropertyMetadata(typeof (ImageLabel)));
		}

		public bool UseLightColors
		{
			get { return (bool) GetValue(UseLightColorsProperty); }
			set { SetValue(UseLightColorsProperty, value); }
		}

		public ImageSource DarkImage
		{
			get { return (ImageSource) GetValue(DarkImageProperty); }
			set { SetValue(DarkImageProperty, value); }
		}

		public ImageSource LightImage
		{
			get { return (ImageSource) GetValue(LightImageProperty); }
			set { SetValue(LightImageProperty, value); }
		}
	}
}