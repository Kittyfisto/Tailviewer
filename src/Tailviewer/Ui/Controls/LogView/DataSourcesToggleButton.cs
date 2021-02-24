using System.Windows;
using System.Windows.Controls;

namespace Tailviewer.Ui.Controls.LogView
{
	public sealed class DataSourcesToggleButton
		: Control
	{
		static DataSourcesToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DataSourcesToggleButton),
			                                         new FrameworkPropertyMetadata(typeof(DataSourcesToggleButton)));
		}
	}
}