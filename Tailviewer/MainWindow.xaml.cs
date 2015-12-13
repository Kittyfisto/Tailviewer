using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Tailviewer.Settings;

namespace Tailviewer
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public static readonly DependencyProperty FocusLogFileSearchCommandProperty =
			DependencyProperty.Register("FocusLogFileSearchCommand", typeof (ICommand), typeof (MainWindow), new PropertyMetadata(default(ICommand)));

		public ICommand FocusLogFileSearchCommand
		{
			get { return (ICommand) GetValue(FocusLogFileSearchCommandProperty); }
			set { SetValue(FocusLogFileSearchCommandProperty, value); }
		}

		public MainWindow()
		{
			FocusLogFileSearchCommand = new DelegateCommand(FocusLogFileSearch);

			InitializeComponent();
			Loaded += OnLoaded;
			Closing += OnClosing;
		}

		private void FocusLogFileSearch()
		{
			PART_LogFileView.FocusStringFilter();
		}

		private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
		{
			ApplicationSettings.Current.MainWindow.UpdateFrom(this);
			ApplicationSettings.Current.Save();
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
				((MainWindowViewModel)DataContext).OpenFiles(files);
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