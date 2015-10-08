using System;
using System.ComponentModel;
using System.Windows;

namespace SharpTail
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private readonly MainWindowViewModel _viewModel;

		public MainWindow()
		{
			WindowConfiguration config = MainWindowConfiguration.Restore();
			if (config != null)
			{
				config.RestoreTo(this);
			}
			DataContext = _viewModel = new MainWindowViewModel(Dispatcher);

			InitializeComponent();
			Loaded += OnLoaded;
			Closing += OnClosing;
		}

		private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
		{
			WindowConfiguration config = WindowConfiguration.From(this);
			MainWindowConfiguration.Store(config);
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			
		}

		private void MainWindow_OnDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				// Note that you can have more than one file.
				var files = (string[])e.Data.GetData(DataFormats.FileDrop);

				// Assuming you have one file that you care about, pass it off to whatever
				// handling code you have defined.
				_viewModel.OpenFiles(files);
			}
		}

		private void OnDragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effects = DragDropEffects.Link;
			}
			else
			{
				e.Effects = DragDropEffects.None;
			}
		}
	}
}