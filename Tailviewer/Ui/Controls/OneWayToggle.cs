using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tailviewer.Ui.Controls
{
	public class OneWayToggle : ContentControl
	{
		public static readonly DependencyProperty HasLeftBorderProperty =
			DependencyProperty.Register("HasLeftBorder", typeof (bool), typeof (OneWayToggle), new PropertyMetadata(true));

		public static readonly DependencyProperty HasRightBorderProperty =
			DependencyProperty.Register("HasRightBorder", typeof (bool), typeof (OneWayToggle), new PropertyMetadata(true));

		public static readonly DependencyProperty IsPressedProperty =
			DependencyProperty.Register("IsPressed", typeof (bool), typeof (OneWayToggle), new PropertyMetadata(default(bool)));

		public static readonly DependencyProperty IsCheckedProperty =
			DependencyProperty.Register("IsChecked", typeof (bool), typeof (OneWayToggle), new PropertyMetadata(default(bool)));

		public bool IsChecked
		{
			get { return (bool) GetValue(IsCheckedProperty); }
			set { SetValue(IsCheckedProperty, value); }
		}

		public bool IsPressed
		{
			get { return (bool) GetValue(IsPressedProperty); }
			set { SetValue(IsPressedProperty, value); }
		}

		public bool HasRightBorder
		{
			get { return (bool) GetValue(HasRightBorderProperty); }
			set { SetValue(HasRightBorderProperty, value); }
		}

		public bool HasLeftBorder
		{
			get { return (bool) GetValue(HasLeftBorderProperty); }
			set { SetValue(HasLeftBorderProperty, value); }
		}

		static OneWayToggle()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (OneWayToggle), new FrameworkPropertyMetadata(typeof (OneWayToggle)));
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			e.Handled = true;
			IsPressed = true;
			CaptureMouse();

			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			e.Handled = true;
			IsPressed = false;
			IsChecked = true;
			ReleaseMouseCapture();

			base.OnMouseUp(e);
		}
	}
}