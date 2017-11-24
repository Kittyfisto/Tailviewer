using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using log4net;
using Metrolib;
using Metrolib.Controls;
using Tailviewer.Ui.Controls.DataSourceTree;
using Tailviewer.Ui.Controls.LogView;
using Tailviewer.Ui.Controls.MainPanel;
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
			DependencyProperty.Register("FocusLogFileSearchCommand", typeof(ICommand), typeof(MainWindow),
			                            new PropertyMetadata(default(ICommand)));

		public static readonly DependencyProperty FocusDataSourceSearchCommandProperty =
			DependencyProperty.Register("FocusDataSourceSearchCommand", typeof(ICommand), typeof(MainWindow),
			                            new PropertyMetadata(default(ICommand)));



		public static readonly DependencyProperty NewQuickFilterCommandProperty = DependencyProperty.Register(
		                                                                                                      "NewQuickFilterCommand",
		                                                                                                      typeof(ICommand
		                                                                                                      ),
		                                                                                                      typeof(
			                                                                                                      MainWindow),
		                                                                                                      new
			                                                                                                      PropertyMetadata(default
			                                                                                                                       (ICommand
			                                                                                                                       )))
			;

		private readonly ApplicationSettings _settings;

		public MainWindow(ApplicationSettings settings)
		{
			if (settings == null) throw new ArgumentNullException(nameof(settings));

			_settings = settings;
			FocusLogFileSearchCommand = new DelegateCommand(FocusLogFileSearch);
			FocusDataSourceSearchCommand = new DelegateCommand(FocusDataSourceSearch);
			NewQuickFilterCommand = new DelegateCommand(NewQuickFilter);
			

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

		public ICommand NewQuickFilterCommand
		{
			get { return (ICommand) GetValue(NewQuickFilterCommandProperty); }
			set { SetValue(NewQuickFilterCommandProperty, value); }
		}

		public void OnShowMainwindow()
		{
			Dispatcher.BeginInvoke(new Action(() =>
			{
				Log.InfoFormat("Ensuring main window is visible because another process asked us to...");
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

		private void OnLocationChanged(object sender, EventArgs eventArgs)
		{
			_settings.MainWindow.UpdateFrom(this);
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			_settings.MainWindow.UpdateFrom(this);
		}

		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			DragLayer.OnMouseMove(e);
		}

		private void FocusLogFileSearch()
		{
			var grid = VisualTreeHelper.GetChild(PART_Content, childIndex: 0) as Grid;
			if (grid != null)
			{
				var logViewerControl = VisualTreeHelper.GetChild(grid, childIndex: 1) as LogViewerControl;
				logViewerControl?.PART_SearchBox.Focus();
			}
		}

		private void FocusDataSourceSearch()
		{
			DataSourcesControl.Instance?.FocusSearch();
		}

		private void NewQuickFilter()
		{
			var viewModel = DataContext as MainWindowViewModel;
			var logViewModel = viewModel?.SelectedMainPanel as LogViewMainPanelViewModel;
			if (logViewModel != null)
			{
				var model = logViewModel.AddQuickFilter();
				logViewModel.ShowQuickFilters();

				// Whelp, this is ugly: I just want to ensure that upon creating a new filter, the text-box is focused
				// so the user can start typing. I guess it would be prettier if we were to create a custom control
				// that takes care of it, but I'm feeling lazy...
				// (The operation is dispatched because the control is created later on and we chose a low priority to
				// ensure that this method is invoked *after* the control has been created).
				Dispatcher.BeginInvoke(new Action(() =>
				{
					var boxes = this.FindVisualChildren<FilterTextBox>().ToList();
					var textBox = boxes.LastOrDefault(x => x.DataContext == model);
					textBox?.Focus();
				}), DispatcherPriority.Background);
			}
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
				e.Effects = DragDropEffects.Link;
			else
				DataSourcesControl.Instance?.HandleDrag(e);
			e.Handled = true;
		}
	}
}