using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using log4net;
using Metrolib;
using Tailviewer.Ui.Controls.DataSourceTree;
using Tailviewer.Ui.ViewModels;
using ApplicationSettings = Tailviewer.Settings.ApplicationSettings;

namespace Tailviewer.Ui.Controls
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
		: SingleApplicationHelper.IMessageListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly DependencyProperty FocusLogFileSearchCommandProperty =
			DependencyProperty.Register("FocusLogFileSearchCommand", typeof (ICommand), typeof (MainWindow),
			                            new PropertyMetadata(default(ICommand)));

		public static readonly DependencyProperty FocusDataSourceSearchCommandProperty =
			DependencyProperty.Register("FocusDataSourceSearchCommand", typeof (ICommand), typeof (MainWindow),
			                            new PropertyMetadata(default(ICommand)));

		private readonly ApplicationSettings _settings;

		internal MainWindow(ApplicationSettings settings)
		{
			if (settings == null) throw new ArgumentNullException(nameof(settings));

			_settings = settings;
			FocusLogFileSearchCommand = new DelegateCommand(FocusLogFileSearch);
			FocusDataSourceSearchCommand = new DelegateCommand(FocusDataSourceSearch);

			InitializeComponent();
			SizeChanged += OnSizeChanged;
			LocationChanged += OnLocationChanged;
			Closing += OnClosing;
			DragEnter += OnDragEnter;
			DragOver += OnDragOver;
			Drop += OnDrop;
			MouseMove += OnMouseMove;

			DragLayer.AdornerLayer = PART_DragDecorator.AdornerLayer;
		}

		private void OnLocationChanged(object sender, EventArgs eventArgs)
		{
			_settings.MainWindow.UpdateFrom(this);
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			_settings.MainWindow.UpdateFrom(this);
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
			//PART_LogFileView.FocusStringFilter();
		}

		private void FocusDataSourceSearch()
		{
			DataSourcesControl.Instance?.FocusSearch();
		}

		private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
		{
			_settings.MainWindow.UpdateFrom(this);
			_settings.Save();
		}

		private void OnDragOver(object sender, DragEventArgs e)
		{
			HandleDrag(e);
		}

		private void OnDragEnter(object sender, DragEventArgs e)
		{
			HandleDrag(e);
		}

		private void OnDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				// Note that you can have more than one file.
				var files = (string[]) e.Data.GetData(DataFormats.FileDrop);

				// Assuming you have one file that you care about, pass it off to whatever
				// handling code you have defined.
				((MainWindowViewModel) DataContext).OpenFiles(files);
			}
			else
			{
				// Why the fuck is the main window even asked to handle
				// the drag when the mouse is clearly over the data source tree?!
				DataSourcesControl.Instance?.PartDataSourcesOnDrop(sender, e);
			}
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
				// Why the fuck is the main window even asked to handle
				// the drag when the mouse is clearly over the data source tree?!
				DataSourcesControl.Instance?.HandleDrag(e);
			}
			e.Handled = true;
		}

		public void OnShowMainwindow()
		{
			Dispatcher.BeginInvoke(new Action(() =>
			{
				Log.InfoFormat("Ensuring main window is visible because another process asked us to...");
				//if (!IsFocused)
				//	Focus();
				Activate();
				BringIntoView();
			}));
		}

		public void OnOpenDataSource(string dataSourceUri)
		{
			Dispatcher.BeginInvoke(new Action(() =>
			{
				Log.InfoFormat("Opening data source because another process asked us to...");
				var viewModel = DataContext as MainWindowViewModel;
				viewModel?.OpenFile(dataSourceUri);
			}));
		}
	}
}