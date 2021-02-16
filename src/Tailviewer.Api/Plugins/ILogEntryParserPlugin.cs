using System.Collections.Generic;

namespace Tailviewer.Plugins
{
	/// <summary>
	///     This plugin needs to be implemented when introducing a log format to tailviewer that it does not natively support.
	/// </summary>
	[Service]
	public interface ILogEntryParserPlugin
		: IPlugin
	{
		/// <summary>
		///     The list of log file formats this plugin supports.
		/// </summary>
		/// <remarks>
		///     This can be a well known format known by tailviewer,
		///     a custom format introduced to tailviewer by implementing
		///     <see cref="ILogFileFormatMatcherPlugin" /> or a combination
		///     of the two.
		/// </remarks>
		IReadOnlyList<ILogFileFormat> SupportedFormats { get; }

		/// <summary>
		///     Creates a new parser which is used to parse a log file of the given format.
		///     <see cref="ILogEntryParser.Parse" /> is called for each log entry in no particular
		///     order.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		ILogEntryParser CreateParser(IServiceContainer services, ILogFileFormat format);
	}
}