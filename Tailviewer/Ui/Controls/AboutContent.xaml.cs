namespace Tailviewer.Ui.Controls
{
	public partial class AboutContent
	{
		public AboutContent()
		{
			InitializeComponent();

			DataContext = new AboutViewModel();
		}
	}
}
