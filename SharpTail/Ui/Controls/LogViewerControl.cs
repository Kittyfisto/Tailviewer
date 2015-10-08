using System.Windows;
using System.Windows.Controls;

namespace SharpTail.Ui.Controls
{
	public class LogViewerControl : Control
	{
		static LogViewerControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (LogViewerControl),
			                                         new FrameworkPropertyMetadata(typeof (LogViewerControl)));
		}
	}
}