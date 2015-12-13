using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public static readonly DependencyProperty FocusLogFileSearchCommandProperty =
			DependencyProperty.Register("FocusLogFileSearchCommand", typeof (ICommand), typeof (MainWindow),
			                            new PropertyMetadata(default(ICommand)));

		public static readonly DependencyProperty FocusDataSourceSearchCommandProperty =
			DependencyProperty.Register("FocusDataSourceSearchCommand", typeof (ICommand), typeof (MainWindow),
			                            new PropertyMetadata(default(ICommand)));

		public MainWindow()
		{
			FocusLogFileSearchCommand = new DelegateCommand(FocusLogFileSearch);
			FocusDataSourceSearchCommand = new DelegateCommand(FocusDataSourceSearch);

			InitializeComponent();
			Closing += OnClosing;
		}

		public ICommand FocusDataSourceSearchCommand
		{
			get { return (ICommand) GetValue(FocusDataSourceSearchCommandProperty); }
			set { SetValue(FocusDataSourceSearchCommandProperty, value); }
		}

		public ICommand FocusLogFileSearchCommand
		{
			get { return (ICommand) GetValue(FocusLogFileSearchCommandProperty); }
			set { SetValue(FocusLogFileSearchCommandProperty, value); }
		}

		private void FocusLogFileSearch()
		{
			PART_LogFileView.FocusStringFilter();
		}

		private void FocusDataSourceSearch()
		{
			PART_DataSources.FocusSearch();
		}

		private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
		{
			ApplicationSettings.Current.MainWindow.UpdateFrom(this);
			ApplicationSettings.Current.Save();
		}

		private void MainWindow_OnDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				// Note that you can have more than one file.
				var files = (string[]) e.Data.GetData(DataFormats.FileDrop);

				// Assuming you have one file that you care about, pass it off to whatever
				// handling code you have defined.
				((MainWindowViewModel) DataContext).OpenFiles(files);
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