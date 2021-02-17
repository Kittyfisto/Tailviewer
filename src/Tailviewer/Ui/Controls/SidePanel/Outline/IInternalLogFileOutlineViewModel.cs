using System.Windows;

namespace Tailviewer.Ui.Controls.SidePanel.Outline
{
	internal interface IInternalLogFileOutlineViewModel
		: ILogFileOutlineViewModel
	{
		FrameworkElement TryCreateContent();
	}
}