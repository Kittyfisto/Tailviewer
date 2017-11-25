using System.Windows;
using Metrolib.Controls;

namespace Tailviewer.Ui.Controls.MainPanel.Raw.QuickNavigation
{
	/// <summary>
	///     The popup which hosts a fancy text-box which displays a list of data sources
	///     which match the entered term.
	/// </summary>
	public sealed class QuickNavigationPopup
		: AutoPopup<SuggestionInputControl>
	{
		static QuickNavigationPopup()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(QuickNavigationPopup),
			                                         new FrameworkPropertyMetadata(typeof(QuickNavigationPopup)));
		}
	}
}