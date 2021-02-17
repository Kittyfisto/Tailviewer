namespace Tailviewer.Plugins
{
	/// <summary>
	///     Responsible for matching log files to a particular format.
	/// </summary>
	[Service]
	public interface ILogFileFormatMatcher
	{
		/// <summary>
		/// </summary>
		/// <remarks>
		///     Tailviewer calls this method in order to find it's <see cref="ILogFileFormat" />.
		///     An implementation should not assume to receive more than the first 512 bytes of the file,
		///     and it WILL be called with less, if the file is smaller in size.
		/// </remarks>
		/// <param name="fileName">The complete file path of the log file.</param>
		/// <param name="initialContent">The first few bytes of the log file.</param>
		/// <param name="format">The format this matcher has decided the log file is in.</param>
		/// <returns>true in case this matcher is 100% certain that the given log file is of a particular format, false otherwise.</returns>
		bool TryMatchFormat(string fileName,
		                    byte[] initialContent,
		                    out ILogFileFormat format);
	}
}