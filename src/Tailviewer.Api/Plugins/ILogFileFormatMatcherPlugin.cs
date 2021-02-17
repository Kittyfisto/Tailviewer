namespace Tailviewer.Plugins
{
	/// <summary>
	///     Responsible for allowing tailviewer to detect additional log file formats.
	/// </summary>
	/// <remarks>
	///     This plugin should be implemented in order to:
	///     - Change encoding a text log file is opened with
	///     - Implement <see cref="ILogFileOutlinePlugin"/> for a specific type of log file NOT known to tailviewer already
	///     - Implement <see cref="ILogFileIssuesPlugin"/> for a specific type of log file NOT known to tailviewer already
	/// </remarks>
	public interface ILogFileFormatMatcherPlugin
		: IPlugin
	{
		/// <summary>
		///     Creates a new matcher which is able to detect log file formats.
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		ILogFileFormatMatcher CreateMatcher(IServiceContainer services);
	}
}