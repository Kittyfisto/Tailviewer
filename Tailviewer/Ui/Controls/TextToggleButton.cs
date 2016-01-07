using System.Windows;
using System.Windows.Controls.Primitives;

namespace Tailviewer.Ui.Controls
{
	public class TextToggleButton : ToggleButton
	{
		static TextToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TextToggleButton), new FrameworkPropertyMetadata(typeof(TextToggleButton)));
		}
	}
}
