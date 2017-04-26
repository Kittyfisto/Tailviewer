using System.Windows;

namespace NewDesign.Dashboard.Widgets
{
	public static class Widget
	{
		public static readonly DependencyProperty IsEditingProperty = DependencyProperty.RegisterAttached(
			"IsEditing", typeof(bool), typeof(Widget), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

		public static void SetIsEditing(DependencyObject element, bool value)
		{
			element.SetValue(IsEditingProperty, value);
		}

		public static bool GetIsEditing(DependencyObject element)
		{
			return (bool) element.GetValue(IsEditingProperty);
		}
	}
}