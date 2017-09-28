using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Tailviewer.Templates.Analysis;

namespace Tailviewer.Ui.Analysis
{
	/// <summary>
	///     The interface for the view model of a widget.
	/// </summary>
	/// <remarks>
	///     Most of the widget's configuration should only be accessible when <see cref="IsEditing" />
	///     is set to true: Any <see cref="DataTemplate" /> displaying this view model is expected
	///     to change its look to allow the user to change the widget's configuration.
	/// </remarks>
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
		///     Whether or not the analysis has been finished.
		///     The analysis is only ever finished when <see cref="Progress" /> has reached
		///     <see cref="Percentage.HundredPercent" />.
		/// </summary>
		bool IsAnalysisFinished { get; }

		/// <summary>
		///     The current progress of the analysis.
		/// </summary>
		double Progress { get; }

		/// <summary>
		///     A
		/// </summary>
		string ProgressTooltip { get; }

		/// <summary>
		///     The current template of this widget.
		///     Shall be continuously modified whenever changes to this widget
		///     (such as title, view- or analysis configuration) are made.
		/// </summary>
		IWidgetTemplate Template { get; }

		/// <summary>
		///     This method is called periodically to allow the view model to fetch data from the analysis.
		/// </summary>
		void Update();

		/// <summary>
		///     This event is fired when the <see cref="DeleteCommand" /> is executed.
		/// </summary>
		event Action<IWidgetViewModel> OnDelete;
	}
}