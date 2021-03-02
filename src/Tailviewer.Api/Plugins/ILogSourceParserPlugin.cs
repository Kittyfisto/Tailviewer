// ReSharper disable once CheckNamespace
namespace Tailviewer.Api
{
	/// <summary>
	///     This plugin needs to be implemented when introducing a log format to tailviewer that it does not natively support.
	/// </summary>
	[Service]
	public interface ILogSourceParserPlugin
		: IPlugin
	{
		/// <summary>
		///     Creates a new log source which aggregates the given one provides access to more columns based on what
		///     could be parsed from the original source.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		[ThreadSafe]
		ILogSource CreateParser(IServiceContainer services, ILogSource source);
	}
}