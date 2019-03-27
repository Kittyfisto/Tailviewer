using System.Windows;
using System.Windows.Controls;

namespace Tailviewer.Ui.Controls.QuickFilter
{
	public class FilterToggleButton : Button
	{
		static FilterToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterToggleButton), new FrameworkPropertyMetadata(typeof(FilterToggleButton)));
		}

		public FilterToggleButton()
		{
			Click += OnClick;
		}

		private void OnClick(object sender, RoutedEventArgs e)
		{
			IsChecked = !IsChecked;
		}

		public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
		                                                "IsChecked", typeof(bool), typeof(FilterToggleButton), new PropertyMetadata(default(bool)));

		public bool IsChecked
		{
			get { return (bool) GetValue(IsCheckedProperty); }
			set { SetValue(IsCheckedProperty, value); }
		}
	}
}
