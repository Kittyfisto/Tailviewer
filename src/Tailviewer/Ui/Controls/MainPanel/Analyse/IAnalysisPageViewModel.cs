using System.ComponentModel;
using System.Windows.Input;
using Tailviewer.Core.Analysis;
using Tailviewer.Ui.Analysis;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse
{
	/// <summary>
	///     Represents a single page of an analysis.
	///     A page consists of zero or more <see cref="IWidgetViewModel" />s which are
	///     presented using a <see cref="IWidgetLayoutViewModel" />.
	///     The layout of a page can be changed through <see cref="PageLayout" />.
	/// </summary>
	public interface IAnalysisPageViewModel
		: INotifyPropertyChanged
	{
		/// <summary>
		///     The type of layout used to display widgets.
		/// </summary>
		PageLayout PageLayout { get; set; }

		/// <summary>
		///     The layout which holds the widgets to be displayed.
		/// </summary>
		/// <remarks>
		///     The widgets themselves may only be accessed via the actual sub-class implementing
		///     <see cref="IWidgetLayoutViewModel" /> because the interaction between UI and view model
		///     pretty much depend on which type of layout this is.
		/// </remarks>
		IWidgetLayoutViewModel Layout { get; }

		/// <summary>
		///     Whether or not any widgets are on this page.
		/// </summary>
		bool HasWidgets { get; }

		string Name { get; set; }

		ICommand DeletePageCommand { get; }

		bool CanBeDeleted { get; set; }

		PageTemplate Template { get; }

		void Update();
	}
}