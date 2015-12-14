using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls
{
	internal class QuickFilterControl : Control
	{
		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof (IEnumerable<QuickFilterViewModel>), typeof (QuickFilterControl),
			                            new PropertyMetadata(default(IEnumerable<QuickFilterViewModel>)));

		public static readonly DependencyProperty AddCommandProperty =
			DependencyProperty.Register("AddCommand", typeof (ICommand), typeof (QuickFilterControl),
			                            new PropertyMetadata(default(ICommand)));

		static QuickFilterControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (QuickFilterControl),
			                                         new FrameworkPropertyMetadata(typeof (QuickFilterControl)));
		}

		public ICommand AddCommand
		{
			get { return (ICommand) GetValue(AddCommandProperty); }
			set { SetValue(AddCommandProperty, value); }
		}

		public IEnumerable<QuickFilterViewModel> ItemsSource
		{
			get { return (IEnumerable<QuickFilterViewModel>) GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}
	}
}