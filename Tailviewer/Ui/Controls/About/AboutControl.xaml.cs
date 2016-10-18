namespace Tailviewer.Ui.Controls.About
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
