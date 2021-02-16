namespace Tailviewer.Plugins
{
	/// <summary>
	///     This plugin needs to be implemented when introducing a log format to tailviewer that it does not natively support.
	/// </summary>
	/// <remarks>
	///     TODO: Delete <see cref="ITextLogFileParserPlugin"/> and rename this one to take its place
	/// </remarks>
	[Service]
	public interface ITextLogFileParserPlugin2
		: IPlugin
	{
		/// <summary>
		///     Creates a new parser which is used to parse a log file of the given format.
		///     <see cref="ITextLogFileParser.Parse" /> is called for each log entry in no particular
		///     order.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		ILogSource CreateParser(IServiceContainer services, ILogSource source);
	}
}