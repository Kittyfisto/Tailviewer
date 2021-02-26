namespace Tailviewer.Ui.About
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
