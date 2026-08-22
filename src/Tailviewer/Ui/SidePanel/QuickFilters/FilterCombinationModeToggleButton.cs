using System.Windows;
using System.Windows.Controls;
using Tailviewer.Core;

namespace Tailviewer.Ui.SidePanel.QuickFilters
{
	/// <summary>
	///     A toggle button which allows the user to chose whether active quick filters
	///     are combined via a logical AND or a logical OR.
	/// </summary>
	public sealed class FilterCombinationModeToggleButton
		: Control
	{
		public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
			"Mode", typeof(FilterCombinationMode), typeof(FilterCombinationModeToggleButton),
			new PropertyMetadata(FilterCombinationMode.And, OnModeChanged));

		public static readonly DependencyProperty IsAndCheckedProperty = DependencyProperty.Register(
			"IsAndChecked", typeof(bool), typeof(FilterCombinationModeToggleButton),
			new PropertyMetadata(true, OnIsAndCheckedChanged));

		public static readonly DependencyProperty IsOrCheckedProperty = DependencyProperty.Register(
			"IsOrChecked", typeof(bool), typeof(FilterCombinationModeToggleButton),
			new PropertyMetadata(false, OnIsOrCheckedChanged));

		static FilterCombinationModeToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterCombinationModeToggleButton),
				new FrameworkPropertyMetadata(typeof(FilterCombinationModeToggleButton)));
		}

		public FilterCombinationMode Mode
		{
			get { return (FilterCombinationMode) GetValue(ModeProperty); }
			set { SetValue(ModeProperty, value); }
		}

		public bool IsAndChecked
		{
			get { return (bool) GetValue(IsAndCheckedProperty); }
			set { SetValue(IsAndCheckedProperty, value); }
		}

		public bool IsOrChecked
		{
			get { return (bool) GetValue(IsOrCheckedProperty); }
			set { SetValue(IsOrCheckedProperty, value); }
		}

		private static void OnIsAndCheckedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((FilterCombinationModeToggleButton) dependencyObject).OnIsAndCheckedChanged((bool) args.NewValue);
		}

		private void OnIsAndCheckedChanged(bool newValue)
		{
			if (newValue)
			{
				IsOrChecked = false;
				Mode = FilterCombinationMode.And;
			}
		}

		private static void OnIsOrCheckedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((FilterCombinationModeToggleButton) dependencyObject).OnIsOrCheckedChanged((bool) args.NewValue);
		}

		private void OnIsOrCheckedChanged(bool newValue)
		{
			if (newValue)
			{
				IsAndChecked = false;
				Mode = FilterCombinationMode.Or;
			}
		}

		private static void OnModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((FilterCombinationModeToggleButton) dependencyObject).OnModeChanged((FilterCombinationMode) args.NewValue);
		}

		private void OnModeChanged(FilterCombinationMode newValue)
		{
			IsAndChecked = newValue == FilterCombinationMode.And;
			IsOrChecked = newValue == FilterCombinationMode.Or;
		}
	}
}
