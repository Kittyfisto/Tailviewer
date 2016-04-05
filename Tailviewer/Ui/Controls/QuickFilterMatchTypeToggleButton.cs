using System.Windows;
using System.Windows.Controls;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls
{
	internal class QuickFilterMatchTypeToggleButton : Control
	{
		public static readonly DependencyProperty QuickFilterTypeProperty =
			DependencyProperty.Register("QuickFilterMatchType", typeof (QuickFilterMatchType), typeof (QuickFilterMatchTypeToggleButton),
			                            new PropertyMetadata(default(QuickFilterMatchType), OnQuickFilterTypeChanged));

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

		public QuickFilterMatchType QuickFilterMatchType
		{
			get { return (QuickFilterMatchType) GetValue(QuickFilterTypeProperty); }
			set { SetValue(QuickFilterTypeProperty, value); }
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
				QuickFilterMatchType = QuickFilterMatchType.StringFilter;
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
				QuickFilterMatchType = QuickFilterMatchType.WildcardFilter;
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
				QuickFilterMatchType = QuickFilterMatchType.RegexpFilter;
			}
		}

		private static void OnQuickFilterTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((QuickFilterMatchTypeToggleButton) d).OnQuickFilterTypeChanged((QuickFilterMatchType) e.NewValue);
		}

		private void OnQuickFilterTypeChanged(QuickFilterMatchType newValue)
		{
			IsStringChecked = newValue == QuickFilterMatchType.StringFilter;
			IsWildcardChecked = newValue == QuickFilterMatchType.WildcardFilter;
			IsRegexChecked = newValue == QuickFilterMatchType.RegexpFilter;
		}
	}
}