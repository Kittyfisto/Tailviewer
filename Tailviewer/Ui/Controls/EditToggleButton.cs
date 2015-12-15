using System.Windows;
using System.Windows.Controls.Primitives;

namespace Tailviewer.Ui.Controls
{
	public class EditToggleButton : ToggleButton
	{
		static EditToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(EditToggleButton), new FrameworkPropertyMetadata(typeof(EditToggleButton)));
		}
	}
}
