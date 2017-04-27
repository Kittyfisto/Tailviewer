using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using NewDesign.Dashboard.Layout;

namespace NewDesign.Dashboard
{
	public sealed class DashboardControl
		: Control
	{
		public static readonly DependencyProperty LayoutsProperty = DependencyProperty.Register(
			"Layouts", typeof(IEnumerable<WidgetLayoutViewModel>), typeof(DashboardControl), new PropertyMetadata(default(IEnumerable<WidgetLayoutViewModel>)));

		public static readonly DependencyProperty SidePanelsProperty = DependencyProperty.Register(
			"SidePanels", typeof(IEnumerable<ISidePanelViewModel>), typeof(DashboardControl), new PropertyMetadata(default(IEnumerable<ISidePanelViewModel>)));

		public IEnumerable<ISidePanelViewModel> SidePanels
		{
			get { return (IEnumerable<ISidePanelViewModel>) GetValue(SidePanelsProperty); }
			set { SetValue(SidePanelsProperty, value); }
		}

		public IEnumerable<WidgetLayoutViewModel> Layouts
		{
			get { return (IEnumerable<WidgetLayoutViewModel>) GetValue(LayoutsProperty); }
			set { SetValue(LayoutsProperty, value); }
		}

		static DashboardControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DashboardControl), new FrameworkPropertyMetadata(typeof(DashboardControl)));
		}
	}
}