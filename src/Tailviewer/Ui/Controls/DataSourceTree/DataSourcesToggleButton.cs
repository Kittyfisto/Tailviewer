using System.Windows;
using Metrolib.Controls;

namespace Tailviewer.Ui.Controls.DataSourceTree
{
	/// <summary>
	///     The button with which the user toggles the visibility of the <see cref="DataSourcesControl" />.
	/// </summary>
	public sealed class DataSourcesToggleButton
		: FlatToggleButton
	{
		static DataSourcesToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DataSourcesToggleButton),
			                                         new FrameworkPropertyMetadata(typeof(DataSourcesToggleButton)));
		}
	}
}