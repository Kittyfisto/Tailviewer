using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	///     This interface may be implemented to add an outline to a log file which displays custom content.
	/// </summary>
	/// <remarks>
	///     The outline is part of the right-side panel pane and allows the user to quickly inspect
	///     important information about a log file. Tailviewer will display some generic stats in the outline,
	///     however a plugin will be able to deliver very specific, yet important information here.
	/// </remarks>
	/// <remarks>
	///     Version 1
	///     Initial definition
	/// </remarks>
	[PluginInterfaceVersion(version: 1)]
	public interface ILogFileOutlinePlugin
		: IPlugin
	{
		/// <summary>
		///     A list of regular expressions which may be used to select which log files use this plugin
		///     to display a synopsis.
		/// </summary>
		IReadOnlyList<Regex> SupportedFileNames { get; }

		/// <summary>
		///     Creates a new view model to represent the given log file.
		/// </summary>
		/// <param name="services">A service container which may be used to construct types required by this plugin and to inject custom parameters if necessary</param>
		/// <param name="logFile"></param>
		/// <returns></returns>
		ILogFileOutlineViewModel CreateViewModel(IServiceContainer services, ILogFile logFile);

		/// <summary>
		///     Creates a new control which presents the given view model.
		/// </summary>
		/// <param name="services">A service container which may be used to construct types required by this plugin and to inject custom parameters if necessary</param>
		/// <param name="viewModel"></param>
		/// <returns></returns>
		FrameworkElement CreateContentPresenterFor(IServiceContainer services, ILogFileOutlineViewModel viewModel);
	}
}