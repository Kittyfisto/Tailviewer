using System.Collections.Generic;

namespace Tailviewer.Plugins
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
	/// <remarks>
	///     Version 2
	///     Replaced SupportedFileNames regex with list of supported file formats.
	/// </remarks>
	/// <remarks>
	///     Version 3
	///     Added <see cref="ILogSourceIssue"/> interface so plugins can use their own implementations (and store more information internally, for example).
	/// </remarks>
	[PluginInterfaceVersion(version: 3)]
	public interface ILogFileIssuesPlugin
		: IPlugin
	{
		/// <summary>
		///     A list of log file formats supported by this plugin.
		/// </summary>
		/// <remarks>
		///     This can be a well known format known by tailviewer,
		///     a custom format introduced to tailviewer by implementing
		///     <see cref="ILogFileFormatMatcherPlugin" /> or a combination
		///     of the two.
		/// </remarks>
		IReadOnlyList<ILogFileFormat> SupportedFormats { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="services"></param>
		/// <param name="logSource"></param>
		/// <returns></returns>
		ILogSourceIssueAnalyser CreateAnalyser(IServiceContainer services,
		                                       ILogSource logSource);
	}
}
