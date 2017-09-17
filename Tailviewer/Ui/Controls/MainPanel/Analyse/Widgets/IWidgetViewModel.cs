using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets
{
	public interface IWidgetViewModel
		: INotifyPropertyChanged
	{
		/// <summary>
		///     This flag determines whether or not the user is currently editing
		///     the configuration of this widget.
		/// </summary>
		bool IsEditing { get; set; }

		/// <summary>
		///     When set to true, then the user may change <see cref="IsEditing" /> to true
		///     and then change the widget's configuration. When set to false, then the widget
		///     cannot be edited (presumably because there is nothing to edit).
		/// </summary>
		bool CanBeEdited { get; }

		/// <summary>
		///     The user supplied title of this widget.
		/// </summary>
		/// <remarks>
		///     Should be set to a sensible value after construction.
		/// </remarks>
		string Title { get; set; }

		/// <summary>
		///     The command to delete this widget.
		/// </summary>
		ICommand DeleteCommand { get; }

		/// <summary>
		///     This event is fired when the <see cref="DeleteCommand" /> is executed.
		/// </summary>
		event Action<IWidgetViewModel> OnDelete;
	}
}