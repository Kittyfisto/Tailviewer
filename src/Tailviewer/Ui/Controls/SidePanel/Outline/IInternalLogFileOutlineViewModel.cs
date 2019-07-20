using System.Windows;
using Tailviewer.Ui.Outline;

namespace Tailviewer.Ui.Controls.SidePanel.Outline
{
	internal interface IInternalLogFileOutlineViewModel
		: ILogFileOutlineViewModel
	{
		FrameworkElement TryCreateContent();
	}
}