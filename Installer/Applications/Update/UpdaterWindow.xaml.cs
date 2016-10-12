using System;
using System.Windows.Threading;

namespace Installer.Applications.Update
{
	public partial class UpdaterWindow
	{
		private readonly DispatcherTimer _timer;

		public UpdaterWindow(UpdateWindowViewModel updateWindowViewModel)
		{
			InitializeComponent();

			_timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1.0 / 60) };
			_timer.Tick += TimerOnTick;
			_timer.Start();

			DataContext = updateWindowViewModel;
		}

		private void TimerOnTick(object sender, EventArgs eventArgs)
		{
			((UpdateWindowViewModel)DataContext).Update();
		}
	}
}
