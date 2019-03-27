using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace Installer.Applications.Update
{
	public partial class UpdaterWindow
	{
		private readonly DispatcherTimer _timer;
		private readonly UpdateWindowViewModel _model;

		public UpdaterWindow(UpdateWindowViewModel updateWindowViewModel)
		{
			InitializeComponent();

			_timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1.0 / 60) };
			_timer.Tick += TimerOnTick;
			_timer.Start();

			DataContext = _model = updateWindowViewModel;
			updateWindowViewModel.PropertyChanged += UpdateWindowViewModelOnPropertyChanged;
		}

		private void UpdateWindowViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case "HasFailed":
					if (_model.HasFailed)
					{
						Height = 400;
					}
					break;
			}
		}

		private void TimerOnTick(object sender, EventArgs eventArgs)
		{
			_model.Update();
		}
	}
}
