using System;
using System.ComponentModel;
using Tailviewer.Core.Analysis;
using Tailviewer.Ui.Analysis;
using Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts
{
	/// <summary>
	///     Responsible for controlling *how* a list of widgets is displayed as well as for
	///     allowing the user to add new widgets and rearrange existing widgets.
	/// </summary>
	/// <remarks>
	///     It is expected that the accompanying control takes care of dropping <see cref="WidgetFactoryViewModel" />s
	///     as well as <see cref="IWidgetViewModel" />s.
	/// </remarks>
	public interface IWidgetLayoutViewModel
		: INotifyPropertyChanged
	{
		/// <summary>
		///     A template which describes the current layout.
		///     This place should be used to store information about this layout which should be preserved in between sessions.
		/// </summary>
		IWidgetLayoutTemplate Template { get; }

		/// <summary>
		///     Adds the given widget to this layout.
		/// </summary>
		/// <param name="widget"></param>
		void Add(WidgetViewModelProxy widget);

		/// <summary>
		///     Removes the given widget from this layout.
		/// </summary>
		/// <param name="widget"></param>
		void Remove(WidgetViewModelProxy widget);

		/// <summary>
		///     This event is fired when this layout requests that the given widget shall
		///     be added. It is expected that handlers of this event call <see cref="Add" />
		///     again, if the request is granted.
		/// </summary>
		/// <remarks>
		///     This event is used while dropping widgets onto the layout (from the widgets side panel):
		///     Once the user has made the drop, this event is fired and the widget has been added.
		/// </remarks>
		event Action<IWidgetPlugin> RequestAdd;
	}
}