using System;
using System.Windows.Threading;

namespace Installer
{
	public partial class MainWindow
	{
		private readonly DispatcherTimer _timer;

		public MainWindow(MainWindowViewModel mainWindowViewModel)
		{
			InitializeComponent();

			_timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(1.0/60)};
			_timer.Tick += TimerOnTick;
			_timer.Start();

			DataContext = mainWindowViewModel;
		}

		private void TimerOnTick(object sender, EventArgs eventArgs)
		{
			((MainWindowViewModel)DataContext).Update();
		}
	}
}