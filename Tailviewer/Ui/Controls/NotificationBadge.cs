using System.Windows;
using System.Windows.Controls;

namespace Tailviewer.Ui.Controls
{
	public class NotificationBadge : ContentControl
	{
		static NotificationBadge()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (NotificationBadge),
			                                         new FrameworkPropertyMetadata(typeof (NotificationBadge)));
		}
	}
}