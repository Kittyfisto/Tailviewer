namespace Tailviewer.Ui.Controls
{
	public partial class AboutControl
	{
		public AboutControl()
		{
			InitializeComponent();

			DataContext = new AboutViewModel();
		}
	}
}
