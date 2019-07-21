using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	///     
	/// </summary>
	public interface ITextLogFileParserPlugin
		: IPlugin
	{
		/// <summary>
		///     The list of log file formats this plugin supports.
		/// </summary>
		IReadOnlyList<ILogFileFormat> SupportedFormats { get; }

		/// <summary>
		///     Creates a new parser which is used to parse a log file of the given format.
		///     <see cref="ITextLogFileParser.Parse" /> is called for each log entry in no particular
		///     order.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		ITextLogFileParser CreateParser(IServiceContainer services, ILogFileFormat format);
	}
}