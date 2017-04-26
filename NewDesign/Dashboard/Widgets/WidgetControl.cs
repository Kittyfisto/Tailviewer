using System.Windows;
using System.Windows.Controls;

namespace NewDesign.Dashboard.Widgets
{
	public sealed class WidgetControl
		: ContentControl
	{
		static WidgetControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(WidgetControl), new FrameworkPropertyMetadata(typeof(WidgetControl)));
		}
	}
}