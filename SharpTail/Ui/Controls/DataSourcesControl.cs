using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SharpTail.Ui.ViewModels;

namespace SharpTail.Ui.Controls
{
	public class DataSourcesControl : Control
	{
		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof(IEnumerable<DataSourceViewModel>), typeof(DataSourcesControl), new PropertyMetadata(null));

		public IEnumerable<DataSourceViewModel> ItemsSource
		{
			get { return (IEnumerable<DataSourceViewModel>)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		static DataSourcesControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (DataSourcesControl),
			                                         new FrameworkPropertyMetadata(typeof (DataSourcesControl)));
		}
	}
}