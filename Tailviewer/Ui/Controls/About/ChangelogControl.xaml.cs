namespace Tailviewer.Ui.Controls.About
{
	public partial class ChangelogControl
	{
		public ChangelogControl()
		{
			InitializeComponent();

			DataContext = new ChangelogViewModel();
		}
	}
}
