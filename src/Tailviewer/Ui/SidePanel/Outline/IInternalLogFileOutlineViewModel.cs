using System.Windows;
using Tailviewer.Api;

namespace Tailviewer.Ui.SidePanel.Outline
{
	internal interface IInternalLogFileOutlineViewModel
		: ILogFileOutlineViewModel
	{
		FrameworkElement TryCreateContent();
	}
}