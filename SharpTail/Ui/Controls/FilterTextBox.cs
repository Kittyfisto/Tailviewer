using System.Windows;
using System.Windows.Controls;

namespace SharpTail.Ui.Controls
{
	public class FilterTextBox : Control
	{
		public static readonly DependencyProperty FilterTextProperty =
			DependencyProperty.Register("FilterText", typeof (string), typeof (FilterTextBox), new PropertyMetadata(default(string)));

		public static readonly DependencyProperty WatermarkProperty =
			DependencyProperty.Register("Watermark", typeof (string), typeof (FilterTextBox), new PropertyMetadata(default(string)));

		private TextBox _filterInput;

		public string Watermark
		{
			get { return (string) GetValue(WatermarkProperty); }
			set { SetValue(WatermarkProperty, value); }
		}

		public string FilterText
		{
			get { return (string) GetValue(FilterTextProperty); }
			set { SetValue(FilterTextProperty, value); }
		}

		static FilterTextBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterTextBox), new FrameworkPropertyMetadata(typeof(FilterTextBox)));
		}

		public FilterTextBox()
		{
			GotFocus += OnGotFocus;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_filterInput = (TextBox)GetTemplateChild("PART_FilterInput");
		}

		private void OnGotFocus(object sender, RoutedEventArgs routedEventArgs)
		{
			_filterInput.Focus();
		}
	}
}
