using Tailviewer.BusinessLogic;
using Tailviewer.Ui.Controls.MainPanel;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui
{
	internal sealed class NavigationService
		: INavigationService
	{
		public LogViewMainPanelViewModel LogViewer;
		public MainWindowViewModel MainWindow;

		#region Implementation of INavigationService

		public bool NavigateTo(DataSourceId dataSource, LogLineIndex line)
		{
			if (MainWindow == null)
				return false;

			if (LogViewer == null)
				return false;

			MainWindow.SelectRawEntry();
			return LogViewer.RequestBringIntoView(dataSource, line);
		}

		#endregion
	}
}
