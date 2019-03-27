namespace Tailviewer.Ui.Controls.MainPanel.About
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
