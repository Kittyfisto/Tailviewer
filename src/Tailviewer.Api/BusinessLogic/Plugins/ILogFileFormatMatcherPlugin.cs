namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	///     Responsible for allowing tailviewer to detect additional log file formats.
	/// </summary>
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