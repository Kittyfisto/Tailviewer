// ReSharper disable once CheckNamespace
namespace Tailviewer.Api
{
	/// <summary>
	///     This plugin needs to be implemented when introducing a log format to tailviewer that it does not natively support.
	/// </summary>
	[Service]
	[PluginInterfaceVersion(2)]
	public interface ILogEntryParserPlugin
		: IPlugin
	{
		/// <summary>
		///     Creates a new parser which is used to parse a log file of the given format.
		///     <see cref="ILogEntryParser.Parse" /> is called for each log entry in no particular
		///     order.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		[ThreadSafe]
		ILogEntryParser CreateParser(IServiceContainer services, ILogFileFormat format);
	}
}