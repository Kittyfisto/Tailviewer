using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls
{
	public class DataSourcesControl : Control
	{
		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof(IEnumerable<DataSourceViewModel>), typeof(DataSourcesControl), new PropertyMetadata(null));

		public static readonly DependencyProperty SelectedItemProperty =
			DependencyProperty.Register("SelectedItem", typeof (DataSourceViewModel), typeof (DataSourcesControl), new PropertyMetadata(null, OnSelectedItemChanged));

		private static void OnSelectedItemChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((DataSourcesControl)dependencyObject).OnSelectedItemChanged((DataSourceViewModel)args.OldValue, (DataSourceViewModel)args.NewValue);
		}

		private void OnSelectedItemChanged(DataSourceViewModel oldValue, DataSourceViewModel newValue)
		{
			if (oldValue != null)
			{
				oldValue.IsOpen = false;
			}
			if (newValue != null)
			{
				newValue.IsOpen = true;
			}
		}

		public DataSourceViewModel SelectedItem
		{
			get { return (DataSourceViewModel) GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

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