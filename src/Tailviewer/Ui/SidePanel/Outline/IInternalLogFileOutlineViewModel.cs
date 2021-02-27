using System.Windows;

namespace Tailviewer.Ui.SidePanel.Outline
{
	internal interface IInternalLogFileOutlineViewModel
		: ILogFileOutlineViewModel
	{
		FrameworkElement TryCreateContent();
	}
}