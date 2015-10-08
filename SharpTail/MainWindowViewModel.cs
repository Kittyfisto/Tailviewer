using System.Windows.Threading;
using SharpTail.BusinessLogic;
using SharpTail.Ui;
using SharpTail.Ui.ViewModels;

namespace SharpTail
{
	public sealed class MainWindowViewModel
	{
		private readonly LogViewerViewModel _logViewModel;

		public MainWindowViewModel()
		{}

		public MainWindowViewModel(Dispatcher dispatcher)
		{
			_logViewModel = new LogViewerViewModel(
				new UiDispatcher(dispatcher),
				LogFile.FromFile(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.log"));
		}

		public LogViewerViewModel LogViewModel
		{
			get { return _logViewModel; }
		}
	}
}