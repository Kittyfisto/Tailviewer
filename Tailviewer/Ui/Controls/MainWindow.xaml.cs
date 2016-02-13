using System;
using System.ComponentModel;
using System.Diagnostics;
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

		private readonly ApplicationSettings _settings;

		internal MainWindow(ApplicationSettings settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");

			_settings = settings;
			FocusLogFileSearchCommand = new DelegateCommand(FocusLogFileSearch);
			FocusDataSourceSearchCommand = new DelegateCommand(FocusDataSourceSearch);

			InitializeComponent();
			Closing += OnClosing;
			DragOver += OnDragOver;
			MouseMove += OnMouseMove;
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

		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			DragLayer.OnMouseMove(e);
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
			_settings.MainWindow.UpdateFrom(this);
			_settings.Save();
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

		private void OnDragOver(object sender, DragEventArgs e)
		{
			HandleDrag(e);
		}

		private void OnDragEnter(object sender, DragEventArgs e)
		{
			HandleDrag(e);
		}

		private void HandleDrag(DragEventArgs e)
		{
			DragLayer.UpdateAdornerPosition(e);
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effects = DragDropEffects.Link;
			}
			else
			{
				e.Effects = DragDropEffects.None;
			}
			e.Handled = true;
		}
	}
}