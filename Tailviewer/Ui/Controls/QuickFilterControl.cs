using System.Windows;
using System.Windows.Controls;

namespace Tailviewer.Ui.Controls
{
	public class QuickFilterControl : Control
	{
		static QuickFilterControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(QuickFilterControl), new FrameworkPropertyMetadata(typeof(QuickFilterControl)));
		}
	}
}
