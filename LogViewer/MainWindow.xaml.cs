using System.Windows;
using Tailviewer.BusinessLogic.LogFiles;

namespace LogViewer
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private LogFile _logFile;

		public MainWindow()
		{
			InitializeComponent();

			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			_logFile = new LogFile(@"E:\Code\Tailviewer\Tailviewer.Test\TestData\1Mb.txt");
			_logFile.Start();
			_logFile.Wait();

			PART_LogViewer.LogFile = _logFile;
		}
	}
}