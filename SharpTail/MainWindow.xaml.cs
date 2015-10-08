using System.Windows;

namespace SharpTail
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private MainWindowViewModel _viewModel;

		public MainWindow()
		{
			InitializeComponent();
			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			DataContext = _viewModel = new MainWindowViewModel(Dispatcher);
		}
	}
}