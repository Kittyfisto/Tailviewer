using System.Windows;
using System.Windows.Controls;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets
{
	public class WidgetHost
		: Control
	{
		static WidgetHost()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(WidgetHost), new FrameworkPropertyMetadata(typeof(WidgetHost)));
		}
	}
}
