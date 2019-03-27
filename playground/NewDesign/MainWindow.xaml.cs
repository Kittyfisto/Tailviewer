namespace NewDesign
{
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = new MainWindowViewModel();
		}
	}
}
