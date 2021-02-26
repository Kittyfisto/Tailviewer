using Tailviewer.Ui.LogView;

namespace Tailviewer.Ui
{
	internal sealed class NavigationService
		: INavigationService
	{
		public LogViewMainPanelViewModel LogViewer;
		public MainWindowViewModel MainWindow;

		#region Implementation of INavigationService

		public bool NavigateTo(LogLineIndex line)
		{
			if (MainWindow == null)
				return false;

			if (LogViewer == null)
				return false;

			MainWindow.SelectRawEntry();
			return LogViewer.RequestBringIntoView(line);
		}

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
