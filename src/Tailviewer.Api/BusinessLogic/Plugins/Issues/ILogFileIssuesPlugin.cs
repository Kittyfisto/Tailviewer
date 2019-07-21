using System.Collections.Generic;
using System.Text.RegularExpressions;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Plugins.Issues
{
	/// <summary>
	///     This interface may be implemented to present a list of issues (extracted from the log file)
	///     to the user.
	/// </summary>
	/// <remarks>
	///     The issue list is part of the right-side panel pane and allows the user to quickly inspect
	///     important issues within a log file. Tailviewer won't display anything here by default:
	///     A plugin must be implemented in order to automatically analyze a plugin for very specific errors
	///     which can programmatically be identified and would otherwise take a long time to analyze by hand.
	/// </remarks>
	/// <remarks>
	///     Version 1
	///     Initial definition
	/// </remarks>
	[PluginInterfaceVersion(version: 1)]
	public interface ILogFileIssuesPlugin
		: IPlugin
	{
		/// <summary>
		///     A list of regular expressions which may be used to select which log files use this plugin
		///     to analyse issues.
		/// </summary>
		IReadOnlyList<Regex> SupportedFileNames { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="services"></param>
		/// <param name="logFile"></param>
		/// <returns></returns>
		ILogFileIssueAnalyser CreateAnalyser(IServiceContainer services,
		                                     ILogFile logFile);
	}
}
