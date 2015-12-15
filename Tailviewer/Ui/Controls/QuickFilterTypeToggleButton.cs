using System.Windows;
using System.Windows.Controls;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls
{
	internal class QuickFilterTypeToggleButton : Control
	{
		public static readonly DependencyProperty QuickFilterTypeProperty =
			DependencyProperty.Register("QuickFilterType", typeof (QuickFilterType), typeof (QuickFilterTypeToggleButton),
			                            new PropertyMetadata(default(QuickFilterType), OnQuickFilterTypeChanged));

		public static readonly DependencyProperty IsStringCheckedProperty =
			DependencyProperty.Register("IsStringChecked", typeof (bool), typeof (QuickFilterTypeToggleButton),
			                            new PropertyMetadata(true, OnIsStringCheckedChanged));

		public static readonly DependencyProperty IsWildcardCheckedProperty =
			DependencyProperty.Register("IsWildcardChecked", typeof (bool), typeof (QuickFilterTypeToggleButton),
			                            new PropertyMetadata(false, OnIsWildcardCheckedChanged));

		public static readonly DependencyProperty IsRegexCheckedProperty =
			DependencyProperty.Register("IsRegexChecked", typeof (bool), typeof (QuickFilterTypeToggleButton),
			                            new PropertyMetadata(false, OnIsRegexCheckedChanged));

		static QuickFilterTypeToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (QuickFilterTypeToggleButton),
			                                         new FrameworkPropertyMetadata(typeof (QuickFilterTypeToggleButton)));
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

		public QuickFilterType QuickFilterType
		{
			get { return (QuickFilterType) GetValue(QuickFilterTypeProperty); }
			set { SetValue(QuickFilterTypeProperty, value); }
		}

		private static void OnIsStringCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((QuickFilterTypeToggleButton) d).OnIsStringCheckedChanged((bool) e.NewValue);
		}

		private void OnIsStringCheckedChanged(bool newValue)
		{
			if (newValue)
			{
				IsWildcardChecked = false;
				IsRegexChecked = false;
				QuickFilterType = QuickFilterType.StringFilter;
			}
		}

		private static void OnIsWildcardCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((QuickFilterTypeToggleButton) d).OnIsWildcardCheckedChanged((bool) e.NewValue);
		}

		private void OnIsWildcardCheckedChanged(bool newValue)
		{
			if (newValue)
			{
				IsStringChecked = false;
				IsRegexChecked = false;
				QuickFilterType = QuickFilterType.WildcardFilter;
			}
		}

		private static void OnIsRegexCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((QuickFilterTypeToggleButton) d).OnIsRegexCheckedChanged((bool) e.NewValue);
		}

		private void OnIsRegexCheckedChanged(bool newValue)
		{
			if (newValue)
			{
				IsStringChecked = false;
				IsWildcardChecked = false;
				QuickFilterType = QuickFilterType.RegexpFilter;
			}
		}

		private static void OnQuickFilterTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((QuickFilterTypeToggleButton) d).OnQuickFilterTypeChanged((QuickFilterType) e.NewValue);
		}

		private void OnQuickFilterTypeChanged(QuickFilterType newValue)
		{
			IsStringChecked = newValue == QuickFilterType.StringFilter;
			IsWildcardChecked = newValue == QuickFilterType.WildcardFilter;
			IsRegexChecked = newValue == QuickFilterType.RegexpFilter;
		}
	}
}