using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	///     This interface may be implemented to add support for custom log file formats which are not simple
	///     text based files.
	/// </summary>
	/// <remarks>
	///     This interface has been introduced to allow a more fine-grained selection of files and can be implemented
	///     in favor of <see cref="IFileFormatPlugin"/> if the plugin author needs this fine grained control.
	/// </remarks>
	public interface IFileFormatPlugin2
		: IFileFormatPlugin
		, IPlugin
	{
		/// <summary>
		///    A list of regular expressions which may be used to select which files are displayed
		///    using this plugin.
		/// </summary>
		/// <remarks>
		///    IF a plugin implements this interface, then tailviewer will FIRST check this member
		///    and if a filename matches the regular expression, this plugin will be used for display.
		///    If the regular expression does NOT match, then <see cref="IFileFormatPlugin.SupportedExtensions"/>
		///    will be checked for. If that one matches, then this plugin will still be used for display.
		///    ONLY if neither match, will this plugin NOT be used.
		/// </remarks>
		IReadOnlyList<Regex> SupportedFileNames { get; }
	}
}