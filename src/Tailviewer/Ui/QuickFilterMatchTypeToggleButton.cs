using System.Windows;
using System.Windows.Controls;
using Tailviewer.Core;

namespace Tailviewer.Ui
{
	public class QuickFilterMatchTypeToggleButton : Control
	{
		public static readonly DependencyProperty FilterTypeProperty =
			DependencyProperty.Register("FilterMatchType", typeof (FilterMatchType),
			                            typeof (QuickFilterMatchTypeToggleButton),
			                            new PropertyMetadata(default(FilterMatchType), OnFilterTypeChanged));

		public static readonly DependencyProperty IsStringCheckedProperty =
			DependencyProperty.Register("IsStringChecked", typeof (bool), typeof (QuickFilterMatchTypeToggleButton),
			                            new PropertyMetadata(true, OnIsStringCheckedChanged));

		public static readonly DependencyProperty IsWildcardCheckedProperty =
			DependencyProperty.Register("IsWildcardChecked", typeof (bool), typeof (QuickFilterMatchTypeToggleButton),
			                            new PropertyMetadata(false, OnIsWildcardCheckedChanged));

		public static readonly DependencyProperty IsRegexCheckedProperty =
			DependencyProperty.Register("IsRegexChecked", typeof (bool), typeof (QuickFilterMatchTypeToggleButton),
			                            new PropertyMetadata(false, OnIsRegexCheckedChanged));

		static QuickFilterMatchTypeToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (QuickFilterMatchTypeToggleButton),
			                                         new FrameworkPropertyMetadata(typeof (QuickFilterMatchTypeToggleButton)));
		}

		public bool IsRegexChecked
		{
			get { return (bool) GetValue(IsRegexCheckedProperty); }
			set { SetValue(IsRegexCheckedProperty, value); }
		}

		public bool IsWildcardChecked
		{
			get { return (bool) GetValue(IsWildcardCheckedProperty); }
			set { SetValue(IsWildcardCheckedProperty, value); }
		}

		public bool IsStringChecked
		{
			get { return (bool) GetValue(IsStringCheckedProperty); }
			set { SetValue(IsStringCheckedProperty, value); }
		}

		public FilterMatchType FilterMatchType
		{
			get { return (FilterMatchType) GetValue(FilterTypeProperty); }
			set { SetValue(FilterTypeProperty, value); }
		}

		private static void OnIsStringCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((QuickFilterMatchTypeToggleButton) d).OnIsStringCheckedChanged((bool) e.NewValue);
		}

		private void OnIsStringCheckedChanged(bool newValue)
		{
			if (newValue)
			{
				IsWildcardChecked = false;
				IsRegexChecked = false;
				FilterMatchType = FilterMatchType.SubstringFilter;
			}
		}

		private static void OnIsWildcardCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((QuickFilterMatchTypeToggleButton) d).OnIsWildcardCheckedChanged((bool) e.NewValue);
		}

		private void OnIsWildcardCheckedChanged(bool newValue)
		{
			if (newValue)
			{
				IsStringChecked = false;
				IsRegexChecked = false;
				FilterMatchType = FilterMatchType.WildcardFilter;
			}
		}

		private static void OnIsRegexCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((QuickFilterMatchTypeToggleButton) d).OnIsRegexCheckedChanged((bool) e.NewValue);
		}

		private void OnIsRegexCheckedChanged(bool newValue)
		{
			if (newValue)
			{
				IsStringChecked = false;
				IsWildcardChecked = false;
				FilterMatchType = FilterMatchType.RegexpFilter;
			}
		}

		private static void OnFilterTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((QuickFilterMatchTypeToggleButton) d).OnFilterTypeChanged((FilterMatchType) e.NewValue);
		}

		private void OnFilterTypeChanged(FilterMatchType newValue)
		{
			IsStringChecked = newValue == FilterMatchType.SubstringFilter;
			IsWildcardChecked = newValue == FilterMatchType.WildcardFilter;
			IsRegexChecked = newValue == FilterMatchType.RegexpFilter;
		}
	}
}